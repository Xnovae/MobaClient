
/*
Author: liyonghelpme
Email: 233242872@qq.com
*/

/*
Author: liyonghelpme
Email: 233242872@qq.com
*/
using UnityEngine;
using System.Collections;
using Google.ProtocolBuffers;

namespace KBEngine
{
	//Response Packet Format
	public class Packet
	{
		public System.UInt32 msglen = 0;
		public byte flowId;
		public byte moduleId;
		public byte msgid = 0;
		public byte responseFlag;

		public IMessageLite protoBody;
        public byte[] data;


		public Packet(uint len, uint fid, byte module, byte msg, byte resflag, IMessageLite pb) {
			//Debug.Log ("receive packet" );
			msglen = len;
			flowId = (byte)fid;
			moduleId = module;
			msgid = msg;
			responseFlag = resflag;

			protoBody = pb;
			//Debug.Log ("Packet:: readPacket "+fid);
			//Debug.Log ("Packet:: readPacket " + protoBody.GetType ().FullName);
		}

		public Packet() {
		}
	}

	public class PacketHolder{
		public Packet packet;
	}

}
