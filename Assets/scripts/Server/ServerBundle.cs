using UnityEngine;
using System.Collections;
using Google.ProtocolBuffers;
using System.Collections.Generic;
using System;

namespace MyLib
{
	public class ServerBundle 
	{
        //static System.UInt32 serverPushFlowId = 0;

		KBEngine.MemoryStream stream = new KBEngine.MemoryStream();
		public int messageLength = 0;
		public KBEngine.Message msgtype = null;
		public byte moduleId;
		public byte msgId;
		public System.Byte flowId;


		void newMessage(System.Type type) {
			Debug.Log ("ServerBundle:: 开始发送消息 Message is " + type.Name);
			var pa = Util.GetMsgID (type.Name);
            if(pa == null) {
                Debug.LogError("GetMessage Id Error, please Update NameMap.json "+type.Name);
            }
			moduleId = pa.moduleId;
			msgId = pa.messageId;
			
			msgtype = null;
		}

		uint writePB(byte[] v, byte errorCode=0) {

			int bodyLength = 1 + 1 + 1+ 1 + v.Length;
			int totalLength = 2 + bodyLength;
			//checkStream (totalLength);
			Debug.Log ("ServerBundle::writePB pack data is "+bodyLength+" pb length "+v.Length+" totalLength "+totalLength);
			Debug.Log ("ServerBundle::writePB module Id msgId " + moduleId+" "+msgId);
			//stream.writeUint8 (Convert.ToByte(0xcc));
			stream.writeUint16(Convert.ToUInt16(bodyLength));
            stream.writeUint8(Convert.ToByte(flowId));
			stream.writeUint8 (Convert.ToByte(moduleId));
			stream.writeUint8(Convert.ToByte(msgId));
			stream.writeUint8 (Convert.ToByte(errorCode)); // no error reponse flag
			stream.writePB (v);
			
			return flowId;
		}

		uint writePB(IMessageLite pbMsg, byte errorCode=0) {
			byte[] bytes;
			using (System.IO.MemoryStream stream = new System.IO.MemoryStream()) {
				pbMsg.WriteTo (stream);
				bytes = stream.ToArray ();
			}
			return writePB (bytes, errorCode);
		}

        public static byte[] sendImmediateError(IBuilderLite build, byte flowId, byte errorCode) {
            var data = build.WeakBuild ();

            var bundle = new ServerBundle ();
            bundle.newMessage (data.GetType());
            bundle.flowId = flowId;
            bundle.writePB (data, errorCode);

            return bundle.stream.getbuffer();
        }

		public static byte[] MakePacket(IBuilderLite build, byte flowId) {
			var data = build.WeakBuild ();

			var bundle = new ServerBundle ();
			bundle.newMessage (data.GetType());
			bundle.flowId = flowId;
			bundle.writePB (data);

			return bundle.stream.getbuffer();
		}

        /*
        /// <summary>
        /// Send Packet With ErrorCode
        /// </summary>
        /// <param name="build">Build.</param>
        /// <param name="flow">Flow.</param>
        /// <param name="errorCode">Error code.</param>
    
        /// <summary>
        /// Response To Request 
        /// </summary>
        /// <param name="build">Build.</param>
        /// <param name="flow">Flow.</param>
      
     
        */
        public static void SendImmediateError(IBuilderLite build, byte flow, byte errorCode)
        {
            //DemoServer.demoServer.GetThread().SendPacket(build, flow, errorCode);
        }
        public static void SendImmediate(IBuilderLite build, byte flow)
        {
            //DemoServer.demoServer.GetThread().SendPacket(build, flow);
        }
        public static void SendImmediatePush(IBuilderLite build)
        {
            //DemoServer.demoServer.GetThread().SendPacket(build, 0);
        }
    }

}