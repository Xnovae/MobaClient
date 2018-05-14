using System.Net;
using KBEngine;
using SimpleJSON;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MonoBehaviour = UnityEngine.MonoBehaviour;

namespace MyLib
{
    public enum WorldState
    {
        Idle,
        Connecting,
        Connected,
        Closed,
    }
    public class NetMatchScene : MonoBehaviour
    {
        public enum RoomState {
            InMatch,
            InGame,
            AllReady, //所有人加入成功准备开始
            GameOver,

            SelectHero, //选择英雄
            TryEnter,
        }
        public RoomState roomState{
            get;
            private set;
        }
        public void SetAllReady() {
            Log.Net("SetAllReady");
            roomState = RoomState.AllReady;
        }

        public int myId;
        MainThreadLoop ml;
        public RemoteClient rc{
            get;
            private set;
        }

        private WorldState _s = WorldState.Idle;
        //private string ServerIP = "127.0.0.1";

        public RoomInfo roomInfo
        {
            get;
            set;
        }

        public WorldState state
        {
            get
            {
                return _s;
            }
            set
            {
                if (_s != WorldState.Closed)
                {
                    _s = value;
                } else
                {
                    Debug.LogError("WorldHasQuit Not: " + value);
                }
            }
        }

        public MatchRoom matchRoom;
        public static NetMatchScene Instance;

        void Awake()
        {
            roomState = RoomState.InMatch;
            if(Instance != null) {
                GameObject.Destroy(Instance.gameObject);
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            matchRoom = gameObject.AddComponent<MatchRoom>();

            StartCoroutine(InitGameData());

            ml = gameObject.AddComponent<MainThreadLoop>();

            TextAsset bindata = Resources.Load("Config") as TextAsset;
            Debug.Log("nameMap " + bindata);
          
            //ServerIP = ClientApp.Instance.remoteServerIP;
            Debug.LogError("ServerIP: " + ClientApp.Instance.QueryServerIP);

            state = WorldState.Connecting;
            StartCoroutine(FindIdleServerThenConnect());

            var eh = gameObject.AddComponent<EvtHandler>();
            eh.AddEvent(MyEvent.EventType.ExitScene, OnExit);

            gameObject.AddComponent<NetworkLatency>();
        }

        private IEnumerator FindIdleServerThenConnect()
        {
            var url = "http://" + ClientApp.Instance.QueryServerIP + ":" + ClientApp.Instance.ServerHttpPort + "/idleServer";
            var www = new WWW(url);
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                //WorldManager.ReturnCity();
                Util.ShowMsg("连接服务器失败");
                WorldManager.ReturnCity();
                yield break;
            }
            var txt = www.text;
            Log.Sys("FindIdleServer: "+txt);
            var returnServer = txt.Split(':');
            var lb = IPAddress.Parse(returnServer[0]);
            var ind = returnServer[0].IndexOf("127.0.0.1");
            Log.Sys("Remote ServerIP: "+returnServer[0]+" ind "+ind);
            if(ind != -1)
            {
                ClientApp.Instance.remoteServerIP = ClientApp.Instance.QueryServerIP;
            }
            else
            {
                ClientApp.Instance.remoteServerIP = returnServer[0];
            }
            ClientApp.Instance.remotePort = System.Convert.ToInt32(returnServer[1]);
            Log.Sys("ConnectRemoteServer: "+ClientApp.Instance.remoteServerIP+" port "+ClientApp.Instance.remotePort);
            StartCoroutine(InitConnect());
        }


        IEnumerator  InitGameData()
        {
            yield return null;
            /*
            GameInterface_Backpack.ClearDrug();
            //等待玩家模型初始化
            yield return StartCoroutine(NetworkUtil.WaitForWorldInit());
            var me = ObjectManager.objectManager.GetMyPlayer();
            var sync = CGSetProp.CreateBuilder();
            sync.Key = (int)CharAttribute.CharAttributeEnum.LEVEL;
            sync.Value = 1;
            KBEngine.Bundle.sendImmediate(sync);
            PlayerData.ResetSkillLevel();
            */
        }

        IEnumerator InitConnect()
        {
            if (rc != null)
            {
                rc = null;
                yield return new WaitForSeconds(2);
            }

            //玩家自己模型尚未初始化准备完毕则不要连接服务器放置Logic之后玩家的ID没有设置
            while (ObjectManager.objectManager.GetMyPlayer() == null)
            {
                yield return null;
            }
            //重新构建新的连接
            rc = new RemoteClient(ml);
            rc.evtHandler = EvtHandler;
            rc.msgHandler = MsgHandler;

            rc.Connect(ClientApp.Instance.remoteServerIP, ClientApp.Instance.remotePort);
            while (lastEvt == RemoteClientEvent.None && state == WorldState.Connecting)
            {
                yield return null;
            }
            Debug.LogError("StartInitData: " + lastEvt);
            MyEventSystem.PushEventStatic(MyEvent.EventType.RemoteReConnect);
            if (lastEvt == RemoteClientEvent.Connected)
            {
                state = WorldState.Connected;
                //初始化数据
                yield return StartCoroutine(InitData());
                //开始匹配
                yield return StartCoroutine(StartMatch());
            }
            else
            {
                Debug.LogError("Connect Lost To Server"); 
            }
        }


        private RemoteClientEvent lastEvt = RemoteClientEvent.None;

        void EvtHandler(RemoteClientEvent evt)
        {
            Debug.LogError("RemoteClientEvent: " + evt);
            NetDebug.netDebug.AddMsg("ReceiveEvt: "+evt);
            lastEvt = evt;
            if (lastEvt == RemoteClientEvent.Close)
            {
                WindowMng.windowMng.ShowNotifyLog("和服务器断开连接：" + state);
                if (state != WorldState.Closed)
                {
                    Debug.LogError("ConnectionClosed But WorldNotClosed");
                    state = WorldState.Closed;
                    //StartCoroutine(RetryConnect());
                    //StartCoroutine(QuitScene());
                    WorldManager.ReturnCity();
                    Util.ShowMsg("断开连接");
                }
            } else if (lastEvt == RemoteClientEvent.Connected)
            {
                WindowMng.windowMng.ShowNotifyLog("连接服务器成功：" + state);
            }
        }

        void MsgHandler(KBEngine.Packet packet)
        {
            var proto = packet.protoBody as GCPlayerCmd;
            Log.Net("ReceiveMsg: " + proto+" pid "+packet.flowId);
            var cmds = proto.Result.Split(' ');
            var c0 = cmds [0];
            if (c0 == "Add")
            {
                roomInfo.PlayersList.Add(proto.AvatarInfo);
                Util.ShowMsg("玩家:"+proto.AvatarInfo.Name+" 加入游戏，当前人数:"+matchRoom.GetPlayerNum());
            } else if (c0 == "Update")
            {
                foreach (var p in roomInfo.PlayersList)
                {
                    if (p.Id == proto.AvatarInfo.Id)
                    {
                        matchRoom.SyncAvatarInfo(proto.AvatarInfo, p);
                        break;
                    }
                }

            } else if (c0 == "Remove")
            {
                foreach (var p in roomInfo.PlayersList)
                {
                    if (p.Id == proto.AvatarInfo.Id)
                    {
                        roomInfo.PlayersList.Remove(p);
                        break;
                    }
                }
                Util.ShowMsg("玩家:"+proto.AvatarInfo.Name+" 离开游戏，当前人数:"+matchRoom.GetPlayerNum());
            } else if (c0 == "StartGame")
            {
                //进入Map5场景开始游戏
                //将网络状态数据保留 
                //等待所有玩家进入场景成功
                //EnterSuc 
                //然后将所有玩家状态重新刷新一遍
                Util.ShowMsg("玩家足够开始游戏："+matchRoom.GetPlayerNum());
                Log.Net("StartGame");
                if (roomState != RoomState.InGame)
                {
                    roomState = RoomState.InGame;
                    WorldManager.worldManager.WorldChangeScene((int)LevelDefine.Battle, false);
                    GameObject.Destroy(GetComponent<NetworkLatency>());
                    var js = new JSONClass();
                    js.Add("total", new JSONData(1));
                    RecordData.UpdateRecord(js);
                }
            }else if(c0 == "SelectHero")
            {
                Util.ShowMsg("开始选择英雄");
                if(roomState == RoomState.InMatch)
                {
                    roomState = RoomState.SelectHero;
                    WorldManager.worldManager.WorldChangeScene((int)LevelDefine.SelectHero, false);
                    GameObject.Destroy(GetComponent<NetworkLatency>());
                }
            }
        }


        /// <summary>
        /// 通知服务器选择的英雄
        /// </summary>
        /// <returns></returns>
        private IEnumerator ChooseHeroCor(int hid)
        {
            if(roomState == RoomState.SelectHero)
            {
                roomState = RoomState.TryEnter;
                var cg = CGPlayerCmd.CreateBuilder();
                var lev = UserInfo.UserLevel;
                cg.Cmd = "ChooseHero";
                var avtar = AvatarInfo.CreateBuilder();
                avtar.PlayerModelInGame = hid;
                cg.AvatarInfo = avtar.Build();

                Bundle bundle;
                var data = KBEngine.Bundle.GetPacketFull(cg, out bundle);
                yield return StartCoroutine(rc.SendWaitResponse(data.data, data.fid, (packet) =>
                {
                    //var cmd = packet.protoBody as GCPlayerCmd;
                    //roomInfo = cmd.RoomInfo;
                    Util.ShowMsg("选择英雄:");
                }, bundle));

            }
        }

        public void ChooseHero(int hid)
        {
            StartCoroutine(ChooseHeroCor(hid));
        }

        void SendUserData()
        {
            Debug.LogError("SendUserData");
            if (state != WorldState.Connected)
            {
                return;
            }
            if (rc == null)
            {
                return;
            }

            var me = ObjectManager.objectManager.GetMyPlayer();
            var pos = me.transform.position;
            var dir = (int)me.transform.localRotation.eulerAngles.y;
            var cg = CGPlayerCmd.CreateBuilder();
            cg.Cmd = "InitData";
            var ainfo = AvatarInfo.CreateBuilder();
            if (NetDebug.netDebug.IsTest)
            {
                ainfo.IsRobot = true;
            }
            else
            {
                ainfo.IsRobot = false;
            }
            
            ainfo.Dir = dir;
            ainfo.Name = ServerData.Instance.playerInfo.Roles.Name;

            var pinfo = ServerData.Instance.playerInfo;
            foreach (var d in pinfo.DressInfoList)
            {
                ainfo.DressInfoList.Add(d);
            }
            ainfo.Level = ObjectManager.objectManager.GetMyProp(CharAttribute.CharAttributeEnum.LEVEL);
            ainfo.HP = ObjectManager.objectManager.GetMyProp(CharAttribute.CharAttributeEnum.HP);
            ainfo.Job = ServerData.Instance.playerInfo.Roles.Job;

            cg.AvatarInfo = ainfo.Build();
            var sync = me.GetComponent<MeSyncToServer>();
            sync.InitData(cg.AvatarInfo);

            Bundle bundle;
            var data = KBEngine.Bundle.GetPacket(cg, out bundle);
            rc.Send(data, bundle);
        }

        private IEnumerator InitData()
        {
            Debug.LogError("InitData");
            if (rc == null)
            {
                yield break;
            }

            var cg = CGPlayerCmd.CreateBuilder();
            cg.Cmd = "Login";
            var dinfo = DeviceInfo.CreateBuilder();
            dinfo.Did = StatisticsManager.Instance.DeviceID;
            dinfo.AccountName = PlatformSdkManager.Instance.Uid;
            dinfo.DeviceName = SystemInfo.deviceModel;
            dinfo.Pid = PlatformSdkManager.Instance.PlatformID;
            dinfo.Uid = PlatformSdkManager.Instance.Uid;
            cg.DeviceInfo = dinfo.Build();
            Bundle bundle;
            var data = KBEngine.Bundle.GetPacketFull(cg, out bundle);
            var fail = false;
            yield return StartCoroutine(rc.SendWaitResponse(data.data, data.fid, (packet) =>
            {
                if (packet.responseFlag == 0)
                {
                    var proto = packet.protoBody as GCPlayerCmd;
                    var cmds = proto.Result.Split(' ');
                    myId = System.Convert.ToInt32(cmds[1]);
                    ObjectManager.objectManager.RefreshMyServerId(myId);
                }
                else
                {
                    fail = true;
                }
            }, bundle));

            //发送报文超时失败 重新发送给服务器
            if (fail)
            {
                Debug.LogError("InitData Fail");
                yield break;
            }
            Debug.LogError("SendLogin: " + myId);
            SendUserData();
        }

        public int MatchNum = 2;
        private bool PressUI = false;

        public void SetPress()
        {
            PressUI = true;
        }

        IEnumerator StartMatch()
        {
            Debug.LogError("StartMatch: "+PressUI);
            NetDebug.netDebug.AddMsg("StartMatch");
            while (!PressUI)
            {
                yield return null;
            }
            Util.ShowMsg("开始匹配");
            Debug.LogError("Matching");
            var cg = CGPlayerCmd.CreateBuilder();
            var lev = UserInfo.UserLevel;
            

            cg.Cmd = "Match";

            //Moba场景客户端和服务器共享的逻辑配置
            //103 是Moba场景配置
            var levelids = new List<int>()
            {
                103,
            };

            var rd = Random.Range(0, levelids.Count);
            var rinfo = RoomInfo.CreateBuilder();
            rinfo.MaxPlayerNum = MatchNum;
            if (GameConst.Instance.TestMap != 0)
            {
                rinfo.LevelId = GameConst.Instance.TestMap;
            }
            else
            {
                rinfo.LevelId = levelids[rd];
            }
            var rin = cg.RoomInfo = rinfo.Build();

            Bundle bundle;
            var data = KBEngine.Bundle.GetPacketFull(cg, out bundle);
            yield return StartCoroutine(rc.SendWaitResponse(data.data, data.fid, (packet) =>
            {
                var cmd = packet.protoBody as GCPlayerCmd;
                roomInfo = cmd.RoomInfo;
                Util.ShowMsg("加载关卡:"+roomInfo.LevelId);
            }, bundle));
        }

     
        public void BroadcastMsg(CGPlayerCmd.Builder cg)
        {
            Log.Net("BroadcastMsg: " + cg);
            if (rc != null)
            {
                Bundle bundle;
                var data = KBEngine.Bundle.GetPacket(cg, out bundle);
                rc.Send(data, bundle);
            }
        }

        void OnExit(MyEvent ev)
        {
            if (WorldManager.NextScene.isCity && this != null)
            {
                GameObject.Destroy(gameObject);
            }
        }

        void OnDestroy()
        {
            if (rc != null)
            {
                rc.evtHandler = null;
                rc.msgHandler = null;
                rc.Disconnect();
                rc = null;
            }
        }
    }
}
