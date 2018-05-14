using System;
using System.Collections.Generic;

namespace MyLib
{
	public class KCP
	{
		private UInt32 sendId = 0;

		private UInt32 windSz = 10;
		private UInt32 winStart = 0;
		private UInt32 winEnd = 3;

		//每次滑动2 但是总长度4
		private UInt32 ackWinStart = 0;
		private UInt32 ackWinEnd = 6;


		//10ms
		public double interval = 0.05;
		private double accTime = 0;
		private UInt32 maxRecvNum = 999;


		//未发送过
		private Queue<KCPPacket> sendQueue = new Queue<KCPPacket>();
		private Queue<KCPPacket> recvQueue = new Queue<KCPPacket>();


		//已经发送过
		private LinkedList<KCPPacket> sendBuf = new LinkedList<KCPPacket>();
		private Queue<KCPPacket> recvBuf = new Queue<KCPPacket>();

		private HashSet<UInt32> ackYet = new HashSet<uint>();

		private LoopList recvList;
        public bool IsClose = false;

        public System.Action closeEventHandler;
		//类似于UDPClient   TCPClient  connect  send recv
		//发送报文增加额外的头部 确定 序号和 ACK
		//创建
		public KCP()
		{
			winStart = 0;
			winEnd = windSz;
			ackWinStart = 0;
			ackWinEnd = windSz * 2;

			recvList = new LoopList(windSz * 2);
		}


		//User to KCP
		public void Send(byte[] data)
		{
			var pack = new KCPPacket()
			{
				data = data,
			};
			sendQueue.Enqueue(pack);
		}

		//KCP to User
		public byte[] Recv()
		{
			byte[] data = null;
			if (recvBuf.Count > 0)
			{
				var pack = recvBuf.Dequeue();
				data = pack.data;
			}
			return data;
		}

		//UDP to KCP
		public void Input(byte[] data, int length)
		{
			var pack = new KCPPacket();
			pack.DecodeData(data, length);
			lock (recvQueue)
			{
				recvQueue.Enqueue(pack);
			}
		}

		//KCP to UDP
		public System.Action<KCPPacket> outputFunc;


		//超时重传等机制更新 
		public void Update(double delta)
		{
			accTime += delta;
            if (accTime >= interval && !IsClose)
			{
				accTime -= interval;
				HandleRecv();
				HandleAcked();
				HandleSend();
			}
		}

		private void PutPackInWindow()
		{
			//可以发送新的报文的条件窗口可以滑动了
			//窗口大小2 当sendId >= 0 < 2 的时候可以发送
			while(sendBuf.Count < windSz && sendQueue.Count > 0)
			{
				var newSn = sendId;
				var inWin = CheckInWin(newSn, winStart, winEnd);

				if (inWin)
				{
					var seg = sendQueue.Dequeue();
					sendBuf.AddLast(seg);
					seg.cmd = KCPPacketCMD.CMD_PUSH;
					seg.ackTimeout = 1;
					seg.sn = sendId++;
				}
				else {
					break;
				}
			}
		}

        /// <summary>
        /// 上层控制关闭 
        /// </summary>
        public void CloseKCP() {
            IsClose = true;
        }

        private void Close() {
            IsClose = true;    
            if(closeEventHandler != null) {
                closeEventHandler();
            }
        }

		private void SendWin()
		{

			//调用底层发送
			if (sendBuf.Count > 0)
			{
				for (var it = sendBuf.First; it != null;)
				{
					var seg = it.Value;
					var next = it.Next;
					//尚未发送
					if (seg.cmd == KCPPacketCMD.CMD_PUSH)
					{
						seg.cmd = KCPPacketCMD.CMD_WAITACK;
						seg.EncodeFull();
                        seg.sendTime = Util.GetTimeNow();
						outputFunc(seg);
					}
					else if (seg.cmd == KCPPacketCMD.CMD_WAITACK)
					{
						if (seg.ackTimeout <= 0)
						{
							seg.ackTimeout = 1;
                            var now = Util.GetTimeNow();
                            if(now-seg.sendTime > 1) {
                                Close(); 
                            }else {
							    outputFunc(seg);
                            }
						}
						else {
							seg.ackTimeout--;
						}
					}
					it = next;
				}
			}
		}

		private void HandleAcked()
		{
			if (sendBuf.Count > 0)
			{
				for (var it = sendBuf.First; it != null;)
				{
					var seg = it.Value;
					var next = it.Next;
					if (seg.cmd == KCPPacketCMD.CMD_ACKED)
					{
						if (it == sendBuf.First)
						{
							winStart++;
							winEnd++;
							Console.WriteLine("SendWinSZ:" + winStart + ":" + winEnd);
						}
						sendBuf.Remove(it);
					}
					it = next;
				}
			}
		}

		private void HandleSend()
		{
			PutPackInWindow();
			SendWin();
		}

		private bool CheckInWin(UInt32 id, UInt32 st, UInt32 end)
		{
			var inWin = false;
			if (st > end)
			{
				inWin = id >= st || id < end;
			}
			else {
				inWin = id >= st && id < end;
			}
			return inWin;
		}

		private void CheckLoopList()
		{
			while (true)
			{
				var pack = recvList.TakePacket();
				if (pack != null)
				{
					recvBuf.Enqueue(pack);
				}
				else {
					break;
				}
			}
		}
		private void HandleRecv()
		{
			var c = 0;
			while (c <= maxRecvNum)
			{
				KCPPacket recPack = null;
				lock (recvQueue)
				{
					//服务器接收到客户端的ACK
					if (recvQueue.Count > 0)
					{
						recPack = recvQueue.Dequeue();
					}
				}

				if (recPack != null)
				{
					c++;
					var seg = recPack;
					if (seg.isAck)
					{
						foreach (var s in sendBuf)
						{
							if (s.sn == seg.sn)
							{
								s.cmd = KCPPacketCMD.CMD_ACKED;
								break;
							}
						}
					}
					else {//只发送一次ACK给服务器 失败则服务器自己报文重发
						  //服务器发送的新的报文 只接受服务器区间内的报文
						  //拒绝服务器重复的报文winStart = 0 winEnd = 2
						  //如果处理过了则不再处理
						  //服务器和每个客户端有独立的连接和独立的序号顺序
						  //连接建立的时刻就是 0 2 确定了
						var srvSn = seg.sn;
						var ret = CheckInWin(srvSn, ackWinStart, ackWinEnd);

						//窗口边界的怎么处理不知道是否已经验证过了两倍大小的接受窗口
						//sendSize 2  ackSz = 4
						if (ret)
						{
							var sendACK = new KCPPacket();
							sendACK.isAck = true;
							sendACK.sn = seg.sn;
							sendACK.EncodeFull();
							//sendQueue.Enqueue(sendACK);
							outputFunc(sendACK);//立即发送ACK

							var tailHalfST = ackWinStart + windSz;
							var tailHalfEND = ackWinEnd;
							//数据出现在后半端，则可以移动接受窗口了
							if (CheckInWin(srvSn, tailHalfST, tailHalfEND))
							{
								ackWinStart++;
								ackWinEnd++;
								ackYet.Remove(ackWinStart - 1);
								recvList.MoveStart();
								Console.WriteLine("WinSz:" + ackWinStart + ":" + ackWinEnd);
							}

							if (!ackYet.Contains(srvSn))
							{
								ackYet.Add(srvSn);
								//recvBuf.Enqueue(seg);

								recvList.AddPacket(seg, srvSn - ackWinStart);
								CheckLoopList();
								Console.WriteLine("Ack:" + srvSn);
							}
						}
					}
				}
				else {
					break;
				}
			}

		}
	}
}

