
/*
Author: liyonghelpme
Email: 233242872@qq.com
*/

/*
Author: liyonghelpme
Email: 233242872@qq.com
*/
using MyLib;
using System.Reflection;
using System.Collections.Generic;

namespace KBEngine
{
  	using UnityEngine; 
	using System; 
	using System.Collections; 
	using System.Collections.Generic;
	using System.Text;
    using System.Threading;
	using System.Text.RegularExpressions;
	
	using MessageID = System.UInt16;
	using MessageLength = System.UInt32;

	public delegate void Callback();
    public class KBEThread
    {

        KBEngineApp app_;
		public bool over = false;
		
        public KBEThread(KBEngineApp app)
        {
            this.app_ = app;
        }

    }

	public class KBEngineApp : IMainLoop
	{
		public static KBEngineApp app = null;
		public float KBE_FLT_MAX = float.MaxValue;
		//public NetworkInterface networkInterface_ = null;
		
        //private Thread t_ = null;
        public KBEThread kbethread = null;
        
        public string username = "kbengine";
        public string password = "123456";
		public enum LoginType {
			UUID,
			ACCOUNT,
		}
		public LoginType loginType = LoginType.UUID;
        
		
		public string ip = "127.0.0.1";
		public UInt16 port = 20013;
		
		public static string url = "http://127.0.0.1";
		
		public string baseappIP = "";
		public UInt16 baseappPort = 0;
		
		// Allow synchronization role position information to the server
		public bool syncPlayer = true;
		
		public UInt64 entity_uuid = 0;
		public Int32 entity_id = 0;
		public string entity_type = "";
		public Vector3 entityLastLocalPos = new Vector3(0f, 0f, 0f);
		public Vector3 entityLastLocalDir = new Vector3(0f, 0f, 0f);
		public Vector3 entityServerPos = new Vector3(0f, 0f, 0f);
		

        public  void queueInUpdate(System.Action cb) {
        }
        public void removeUpdate(System.Action cb) {
        }

		public struct ServerErr
		{
			public string name;
			public string descr;
			public UInt16 id;
		}
		
		public static Dictionary<UInt16, ServerErr> serverErrs = new Dictionary<UInt16, ServerErr>(); 
		
		private System.DateTime lastticktime_ = System.DateTime.Now;
		private System.DateTime lastUpdateToServerTime_ = System.DateTime.Now;
		
		public UInt32 spaceID = 0;
		public string spaceResPath = "";
		public bool isLoadedGeometry = false;
		
		
		public bool isbreak = false;
		public Queue<System.Action> pendingCallbacks = new Queue<Action>();

		ClientApp client;
        public KBEngineApp(ClientApp c)
        {
			client = c;
			app = this;
        	//networkInterface_ = new NetworkInterface(this);
            kbethread = new KBEThread(this);
        }

	    /*
        public void destroy()
        {
        	Dbg.WARNING_MSG("KBEngine::destroy()");
        	isbreak = true;
        	
        	int i = 0;
        	while(!kbethread.over && i < 50)
        	{
        		Thread.Sleep(1);
        		i += 1;
        	}
        	
			if(t_ != null)
        		t_.Abort();

        	t_ = null;
        	
        	reset();
        }
        
        public Thread t(){
        	return t_;
        }
        */

        /*
        public NetworkInterface networkInterface(){
        	return networkInterface_;
        }
        */
        

        /*
		public void reset()
		{
			entity_uuid = 0;
			entity_id = 0;
			entity_type = "";
			lastticktime_ = System.DateTime.Now;
			lastUpdateToServerTime_ = System.DateTime.Now;
			spaceID = 0;
			spaceResPath = "";
			isLoadedGeometry = false;
			
			networkInterface_.reset();
		}
		public void process()
		{
			while(!isbreak)
			{
				networkInterface_.process();
			}
			
			Dbg.WARNING_MSG("KBEngine::process(): break!");
		}

        */

		/*
		 * Connect to login Server 
		 */ 
		public bool login_loginapp()
		{
			//reset();
			return true;
		}
	
		public void queueInLoop(System.Action cb) {
			lock (this) {
				pendingCallbacks.Enqueue(cb);
			}
		}

		/* Muduo Framework
		 * 
		 * Main Thread Update 
		 * 
		 */
		public void UpdateMain() {
			lock (this) {
			    while (pendingCallbacks.Count > 0)
			    {
			        var cb = pendingCallbacks.Dequeue();
			        try
			        {
			            cb();
			        }
			        catch (Exception ex)
			        {
			            Debug.LogError(ex.ToString());
			        }
			    }
			}
            //networkInterface_.process();
		}

	}
} 
