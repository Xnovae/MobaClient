using System;
using System.Collections.Generic;
namespace MyLib
{
	public class LoopList
	{
		private UInt32 start = 0;
		private UInt32 readPos = 0;
		private List<KCPPacket> recvSlots = new List<KCPPacket>();
		private UInt32 total;
		public LoopList(UInt32 n)
		{
			total = n;
			for (var i = 0; i < n; i++)
			{
				recvSlots.Add(null);
			}
		}


		//将新来的报文放到某个slot上面
		public void AddPacket(KCPPacket pack, UInt32 index)
		{
			var id = (start + index) % (total);
			recvSlots[(int)id] = pack;
		}

		//尝试从Slot中获取头部的报文
		public KCPPacket TakePacket()
		{
			int cp = (int)readPos;
			if (recvSlots[cp] == null)
			{
				return null;
			}
			readPos++;
			readPos %= total;
			return recvSlots[cp];
		}

		//移动start的标位置
		public void MoveStart()
		{
			recvSlots[(int)start] = null;
			start++;
			start %= total;
		}
	}

}

