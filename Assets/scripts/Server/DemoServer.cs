using UnityEngine;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using Google.ProtocolBuffers;
using SimpleJSON;
using System;
using KBEngine;

namespace MyLib
{
    /*
	public class ServerThread
	{
		int playerId = 100;
     
		int selectPlayerJob = 4;
		MyLib.ServerMsgReader msgReader = new ServerMsgReader ();
		System.Random random = new System.Random (1000);

		public void CloseServerSocket ()
		{
			
		}

		public ServerThread ()
		{
            msgReader.msgHandle = handleMsg;
        }

		public void SendPacket (IBuilderLite retpb, byte flowId, byte errorCode)
		{
			var bytes = ServerBundle.sendImmediateError (retpb, flowId, errorCode);
            KBEngineApp.app.networkInterface_.AddBuffer(bytes);
		}

		public void SendPacket (IBuilderLite retpb, byte flowId)
		{
			var bytes = ServerBundle.MakePacket (retpb, flowId);
            KBEngineApp.app.networkInterface_.AddBuffer(bytes);
		}

	    public void ReceivePacket(byte[] buffer,uint length)
	    {
            msgReader.process(buffer, length);
        }


		void handleMsg (KBEngine.Packet packet)
		{
			var receivePkg = packet.protoBody.GetType ().FullName;
			Debug.Log ("Server Receive " + receivePkg);
			var className = receivePkg.Split (char.Parse (".")) [1];
			IBuilderLite retPb = null;
			byte flowId = packet.flowId;
			bool findHandler = false;

			if (className == "CGAutoRegisterAccount") {
				var au = GCAutoRegisterAccount.CreateBuilder ();
				au.Username = "liyong";
				retPb = au;
			} else if (className == "CGRegisterAccount") {
				var au = GCRegisterAccount.CreateBuilder ();
				retPb = au;
			} else if (className == "CGLoginAccount") {
				var au = GCLoginAccount.CreateBuilder ();
				var playerInfo = ServerData.Instance.playerInfo;
				if (playerInfo.HasRoles) {
					var role = RolesInfo.CreateBuilder ().MergeFrom (playerInfo.Roles);
					au.AddRolesInfos (role);
				}

				retPb = au;
			} else if (className == "CGSelectCharacter") {
				var inpb = packet.protoBody as CGSelectCharacter;
				if (inpb.PlayerId == 101) {
					selectPlayerJob = 4;
				} else if (inpb.PlayerId == 102) {
					selectPlayerJob = 2;
				} else {
					selectPlayerJob = 1;
				}
				var au = GCSelectCharacter.CreateBuilder ();
				au.TokenId = "12345";
				retPb = au;
			} else if (className == "CGBindingSession") {
				var au = GCBindingSession.CreateBuilder ();
				au.X = 22;
				au.Y = 1;
				au.Z = 17;
				au.Direction = 10;
				au.MapId = 0;
				au.DungeonBaseId = 0;
				au.DungeonId = 0;
				retPb = au;
			} else if (className == "CGEnterScene") {
				var inpb = packet.protoBody as CGEnterScene;
				var au = GCEnterScene.CreateBuilder ();
				au.Id = inpb.Id;
				retPb = au;
			} else if (className == "CGListBranchinges") {
				var au = GCListBranchinges.CreateBuilder ();
				var bran = Branching.CreateBuilder ();
				bran.Line = 1;
				bran.PlayerCount = 2;
				au.AddBranching (bran);
				retPb = au;
			} else if (className == "CGHeartBeat") {
            
			} 
            else if (className == "CGCopyInfo") {
				var pinfo = ServerData.Instance.playerInfo;
				if (pinfo.HasCopyInfos) {
					retPb = GCCopyInfo.CreateBuilder ().MergeFrom (pinfo.CopyInfos);
				} else {
					//First Fetch Login Info
					var au = GCCopyInfo.CreateBuilder ();
					var cin = CopyInfo.CreateBuilder ();
					cin.Id = 101;
					cin.IsPass = false;
					au.AddCopyInfo (cin);
					var msg = au.Build ();
					pinfo.CopyInfos = msg;
					retPb = GCCopyInfo.CreateBuilder ().MergeFrom (msg);
				}
			} 


            else if (className == "CGUserDressEquip") {
				PlayerData.UserDressEquip (packet);
				findHandler = true;
			} else if (className == "CGAutoRegisterAccount") {
				var au = GCAutoRegisterAccount.CreateBuilder ();
				au.Username = "liyong_" + random.Next ();
				retPb = au;
			} else if (className == "CGRegisterAccount") {
				var inpb = packet.protoBody as CGRegisterAccount;
				ServerData.Instance.playerInfo.Username = inpb.Username;

				var au = GCRegisterAccount.CreateBuilder ();
				retPb = au;
			} else if (className == "CGPlayerMove") {
				var au = GCPlayerMove.CreateBuilder ();
				retPb = au;
			} else {
				var fullName = packet.protoBody.GetType ().FullName;
                var handlerName = fullName.Replace ("MyLib", "ServerPacketHandler");

				var tp = Type.GetType (handlerName);
				if (tp == null) {
					if (ServerPacketHandler.HoldCode.staticTypeMap.ContainsKey (handlerName)) {
						tp = ServerPacketHandler.HoldCode.staticTypeMap [handlerName];
					}
				}

				if (tp == null) {
					Debug.LogError ("PushMessage noHandler " + handlerName);
				} else {
					findHandler = true;
					var ph = (ServerPacketHandler.IPacketHandler)Activator.CreateInstance (tp);
					ph.HandlePacket (packet);
				}
			}
        

			if (retPb != null) {
				SendPacket (retPb, flowId);
			} else {
				if (className != "CGHeartBeat" && !findHandler) {
					Debug.LogError ("DemoServer::not Handle Message " + className);
				}
			}
		}

		int maxId = 0;

	}

	public class DemoServer
	{
		public static DemoServer demoServer;
		public bool stop = false;
		ServerData serverData;
		ServerThread sth;

		public ServerThread GetThread ()
		{
			return sth;
		}

		public ServerData GetServerData ()
		{
			return serverData;
		}

		public DemoServer ()
		{
			ServerPacketHandler.HoldCode.Init();

			demoServer = this;
		    if (ServerData.Instance == null)
		    {
		        serverData = new ServerData();
		        serverData.LoadData();
		    }

		    Debug.Log ("Init DemoServer");
			sth = new ServerThread ();
			//var t = new Thread (new ThreadStart (sth.run));
			//t.Start ();
		}

		public void ShutDown ()
		{
			Debug.LogError ("ShutDown Server");
			//serverData.SaveUserData();
			stop = true;

		}
	}
    */
}
