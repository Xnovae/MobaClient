using System;
using System.Net;

namespace MyLib
{
	public enum KCPPacketCMD
	{
		CMD_PUSH,
		CMD_WAITACK,
		CMD_ACKED,
	}

	//isACK seqId data
	public class KCPPacket
	{
		public byte[] data;
		public KCPPacketCMD cmd;
		public UInt32 sn;
		public byte[] fullData;
		public bool acked = false;
		public bool isAck = false;
		public int ackTimeout = 1;
        public double sendTime = 0;
		public IPEndPoint remoteEnd;
		public void EncodeFull()
		{
			if (data != null)
			{
				fullData = new byte[data.Length + 4 + 1];
				if (isAck)
				{
					fullData[0] = 1;
				}
				else {
					fullData[0] = 0;
				}

				var bd = BitConverter.GetBytes(sn);
				Array.Copy(bd, 0, fullData, 1, 4);
				Array.Copy(data, 0, fullData, 5, data.Length);
			}
			else {
				fullData = new byte[0 + 4 + 1];
				if (isAck)
				{
					fullData[0] = 1;
				}
				else {
					fullData[0] = 0;
				}

				var bd = BitConverter.GetBytes(sn);
				Array.Copy(bd, 0, fullData, 1, 4);
			}
		}

		public void DecodeData(byte[] rcData, int len)
		{
			isAck = rcData[0] == 1;
			fullData = rcData;
			sn = BitConverter.ToUInt32(rcData, 1);
			data = new byte[len - 5];
			Array.Copy(rcData, 5, data, 0, len - 5);
		}
	}
}

