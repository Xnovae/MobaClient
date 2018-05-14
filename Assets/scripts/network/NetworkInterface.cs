
/*
Author: liyonghelpme
Email: 233242872@qq.com
*/

/*
Author: liyonghelpme
Email: 233242872@qq.com
*/
using Google.ProtocolBuffers;
using MyLib;

namespace KBEngine
{
  	using UnityEngine; 
	using System; 
	using System.Net.Sockets; 
	using System.Net; 
	using System.Collections; 
	using System.Collections.Generic;
	using System.Text;
	using System.Threading;


	using MessageModuleID = System.SByte;
	using MessageID = System.UInt16;
	using MessageLength = System.UInt32;

	public delegate void MessageHandler(Packet msg);


    public class ThreadSafeDic
    {
        private Dictionary<uint, MessageHandler> datas = new Dictionary<uint, MessageHandler>();

        public bool Contain(uint k)
        {
            lock (datas)
            {
                return datas.ContainsKey(k);
            }
        }

        public void Add(uint k, MessageHandler msg)
        {
            lock (datas)
            {
                datas[k] = msg;
            }
        }

        public void Remove(uint k)
        {
            lock (datas)
            {
                datas.Remove(k);
            }
        }

        public MessageHandler Get(uint k)
        {
            lock (datas)
            {
                return datas[k];
            }
        }
    }

    /*
    public class NetworkInterface 
    {
        private Socket socket_ = null;
		private MessageReader msgReader = new MessageReader();
		private static ManualResetEvent TimeoutObject = new ManualResetEvent(false);
		private static byte[] _datas = new byte[MemoryStream.BUFFER_MAX];

        private Queue<byte[]> dataBuffer = new Queue<byte[]>(); 

        public ThreadSafeDic flowHandlers = new ThreadSafeDic();

        public NetworkInterface(KBEngineApp app)
        {
            msgReader.mainLoop = KBEngine.KBEngineApp.app;
        }
		
		public void reset()
		{
			if(valid())
				close();
			
			socket_ = null;
			msgReader = new MessageReader();
            msgReader.mainLoop = KBEngineApp.app;
			TimeoutObject.Set();
		}
		
		public Socket sock()
		{
			return socket_;
		}
		
		public bool valid()
		{
		    return true;
			//return ((socket_ != null) && (socket_.Connected == true));
		}

        public void AddBuffer(byte[] data)
        {
            dataBuffer.Enqueue(data);
        }
		
		private static void connectCB(IAsyncResult asyncresult)
		{
			if(KBEngineApp.app.networkInterface().valid()) {
				KBEngineApp.app.networkInterface().sock().EndConnect(asyncresult);
            }else {
                MyLib.MyEventSystem.myEventSystem.PushEvent(MyLib.MyEvent.EventType.ReConnect);
            }
			
			TimeoutObject.Set();
		}
	    
		public bool connect(string ip, int port) 
		{
			int count = 0;
__RETRY:
			reset();
			TimeoutObject.Reset();
			
			socket_ = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); 
			socket_.SetSocketOption (System.Net.Sockets.SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, MemoryStream.BUFFER_MAX);
            try 
            { 
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ip), port); 
                
				socket_.BeginConnect(endpoint, new AsyncCallback(connectCB), socket_);
				
		        if (TimeoutObject.WaitOne(10000))
		        {
                    if(valid()) {
                    }else {
                        MyLib.MyEventSystem.myEventSystem.PushEvent(MyLib.MyEvent.EventType.ReConnect);
                    }
		        }
		        else
		        {
		        	reset();
		        }
        
            } 
            catch (Exception e) 
            {
                Dbg.WARNING_MSG(e.ToString());
                
                if(count < 3)
                {
                	Dbg.WARNING_MSG("connect(" + ip + ":" + port + ") is error, try=" + (count++) + "!");
                	goto __RETRY;
           		 }
            
				return false;
            } 
			
			if(!valid())
			{
				return false;
			}
			
			return true;
		}
        
        public void close()
        {
        }


		void DumpHandler(Packet p) {
		}

		public void send(byte[] datas, MessageHandler handler, uint flowId) {

		    if (handler == null)
		    {
		        flowHandlers.Add(flowId, DumpHandler);
		    }
		    else
		    {
		        flowHandlers.Add(flowId, handler);
		    }

            DemoServer.demoServer.GetThread().ReceivePacket(datas, (uint)datas.Length);
		}
		
		public void recv()
		{
		    while (dataBuffer.Count > 0)
		    {
		        var data = dataBuffer.Dequeue();
                msgReader.process(data, (MessageLength)data.Length, flowHandlers);
            }
            
        }
		
		public void process() 
		{
            recv();
		}
	}
    */
} 
