
/*
Author: liyonghelpme
Email: 233242872@qq.com
*/

/*
Author: liyonghelpme
Email: 233242872@qq.com
*/
using Google.ProtocolBuffers;

namespace KBEngine
{
  	using UnityEngine; 
	using System; 
	using System.Collections;
	using System.Collections.Generic;
	using MyLib;

    public class Bundle 
    {
        private static Queue<Bundle> pool = new Queue<Bundle>();

        private static Bundle GetBundle()
        {
            if (pool.Count > 0)
            {
                var q = pool.Dequeue();
                return q;
            }
            else
            {
                return new Bundle();
            }
        }

        public static void ReturnBundle(Bundle bundle)
        {
            bundle.Reset();
            pool.Enqueue(bundle);
        }

        private void Reset()
        {
            moduleId = 0;
            msgId = 0;
            numMessage = 0;
            stream.clear();
        }

        public static List<string> sendMsg = new List<string>();
		public static List<string> recvMsg = new List<string> ();

		private static byte flowId = 1;

    	private MemoryStream stream = new MemoryStream();
		public int numMessage = 0;

		public byte moduleId;
		public byte msgId;
		public Bundle()
		{
		}

		public void newMessage(System.Type type) {
			fini (false);
#if DEBUG
			sendMsg.Add("Bundle:: 开始发送消息 Message is " + type.Name+" "+flowId);
			if (sendMsg.Count > 30) {
				sendMsg.RemoveRange(0, sendMsg.Count-30);
			}

            Log.Net ("Bundle:: 开始发送消息 Message is " + type.Name);
#endif

			var pa = Util.GetMsgID (type.Name);
			moduleId = pa.moduleId;
			msgId = pa.messageId;
			
			numMessage += 1;
		}

		
		
		public void fini(bool issend)
		{
		}

        /*
		public IEnumerator SendCoroutine(NetworkInterface networkInterface, uint fId, PacketHolder par) {
			fini (true);
			//Debug.Log ("message Number " + streamList.Count);
			bool resp = false;
			if(networkInterface.valid())
			{
                networkInterface.send(stream.getbuffer(), delegate(Packet p) {
                    par.packet = p;
                    resp = true;
                }, fId);
                Log.Sys("SendNetworkSuc");
			}
			else
			{
				Dbg.ERROR_MSG("Bundle::send: networkInterface invalid!");  
			}
			
			//streamList.Clear();
			//stream = new MemoryStream();
		    float passTime = 0;
			while (!resp && passTime < 5) {
				yield return null;
			    passTime += Time.deltaTime;
			}
		    if (!resp)
		    {
		        Debug.LogError("RequestTimeOut: "+fId+" con ");
		    }
		}

        private void sendImm(NetworkInterface networkInterface, uint fId)
        {
            fini(true);
            if (networkInterface.valid())
            {
                networkInterface.send(stream.getbuffer(), delegate(Packet p)
                {
                }, fId);
            }
            else
            {
                Dbg.ERROR_MSG("Bundle::send: networkInterface invalid!");
            }
        }

        public static IEnumerator sendSimple(UnityEngine.MonoBehaviour mo, IBuilderLite build, PacketHolder par) {
		
			var bundle = GetBundle();
			var data = build.WeakBuild();

			bundle.newMessage (data.GetType ());
			var fid = bundle.writePB (data);
			Log.Net ("send Simple "+bundle+" d "+data+" fd "+fid);
			yield return mo.StartCoroutine(bundle.SendCoroutine(KBEngine.KBEngineApp.app.networkInterface(), fid, par));
		}
        */

        public static Packet GetPacketFid(IBuilderLite build) {
            var p = new Packet();

            var bundle = GetBundle();
            var data = build.WeakBuild();
            Log.Net("GetPacket: "+data);
            bundle.newMessage(data.GetType());
            var fid = bundle.writePB(data);
            var buff = bundle.stream.getbuffer();
            p.flowId = (byte)fid;
            p.data = buff;
            return p;
        }

        private static uint lastFid;
        public static byte[] GetPacket(IBuilderLite build, out Bundle b) {
            var bundle = GetBundle();
            b = bundle;
            var data = build.WeakBuild();
            Log.Net("GetPacket: "+data);
            bundle.newMessage(data.GetType());
            var fid = bundle.writePB(data);
            lastFid = fid;
            return bundle.stream.getbuffer();
        }
        public struct SendPacketInfo {
            public uint fid;
            public byte[] data;
        }
        public static SendPacketInfo GetPacketFull(IBuilderLite build, out  Bundle b) {
            var data = GetPacket(build, out b);
            var t = new SendPacketInfo() {
                fid = lastFid,
                data = data,
            };
            return t;
        }


        /*
		public static IMessageLite sendImmediate(IBuilderLite build) {
			var bundle = GetBundle();
			var data = build.WeakBuild ();
			bundle.newMessage (data.GetType ());
			var fid = bundle.writePB (data);
			bundle.sendImm(KBEngine.KBEngineApp.app.networkInterface(), fid);
			return data;
		}
		
        */

		public void checkStream(int v)
		{
		}
		
		//---------------------------------------------------------------------------------
		public void writeInt8(SByte v)
		{
			checkStream(1);
			stream.writeInt8(v);
		}
	
		public void writeInt16(Int16 v)
		{
			checkStream(2);
			stream.writeInt16(v);
		}
			
		public void writeInt32(Int32 v)
		{
			checkStream(4);
			stream.writeInt32(v);
		}
	
		public void writeInt64(Int64 v)
		{
			checkStream(8);
			stream.writeInt64(v);
		}
		
		public void writeUint8(Byte v)
		{
			checkStream(1);
			stream.writeUint8(v);
		}
	
		public void writeUint16(UInt16 v)
		{
			checkStream(2);
			stream.writeUint16(v);
		}
			
		public void writeUint32(UInt32 v)
		{
			checkStream(4);
			stream.writeUint32(v);
		}
	
		public void writeUint64(UInt64 v)
		{
			checkStream(8);
			stream.writeUint64(v);
		}
		
		public void writeFloat(float v)
		{
			checkStream(4);
			stream.writeFloat(v);
		}
	
		public void writeDouble(double v)
		{
			checkStream(8);
			stream.writeDouble(v);
		}
		
		public void writeString(string v)
		{
			checkStream(v.Length + 1);
			stream.writeString(v);
		}
		
		public void writeBlob(byte[] v)
		{
			checkStream(v.Length + 4);
			stream.writeBlob(v);
		}

		/*
		 * 0xcc   int8
		 * length int32
		 * flowId int32
		 * moduleId int8
		 * messageId int16
		 * protobuffer
		 */ 
		public uint writePB(byte[] v) {
			byte fid = flowId++;
            if(fid == 0){
                fid++;
                flowId++;
            }

			int bodyLength = 1 + 1 + 1 + v.Length;
			int totalLength = 2 + bodyLength;
			stream.writeUint16(Convert.ToUInt16(bodyLength));
			stream.writeUint8(Convert.ToByte(fid));
			stream.writeUint8 (Convert.ToByte(moduleId));
			stream.writeUint8(Convert.ToByte(msgId));
			stream.writePB (v);
			return fid;
		}

		public uint writePB(IMessageLite pbMsg) {
			byte[] bytes;
			using (System.IO.MemoryStream stream = new System.IO.MemoryStream()) {
				pbMsg.WriteTo (stream);
				bytes = stream.ToArray ();
			}
			return writePB (bytes);
		}

    }
} 
