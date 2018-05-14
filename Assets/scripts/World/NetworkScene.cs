using KBEngine;
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using MonoBehaviour = UnityEngine.MonoBehaviour;

namespace MyLib
{
    /// <summary>
    /// 只进行玩家的初始化 
    /// </summary>
    public class NetworkScene : MonoBehaviour
    {
        private ulong ServerFrameId = 0;
        private float lastSyncTime = 0;

           
        /// <summary>
        /// 获取服务器推送的一组命令的服务器FrameID
        /// </summary>
        /// <returns></returns>
        public ulong GetSyncCmdFrame()
        {
            return ServerFrameId;
        }

        /// <summary>
        /// 用于通知服务器客户端输入 的网络报文的 预测的服务器FrameID 将浮点数转化为整数 * 100
        /// 可能frameID 退化到int 范围
        /// </summary>
        /// <returns></returns>
        public ulong GetPredictServerTimeForNet()
        {
            var st = GetPredictServerFrame();
            return (uint)Util.GameToNetNum(st);
        }
        /// <summary>
        /// 计算客户端当前时间对应的服务器帧ID
        /// 预测值
        /// 服务器100ms一帧
        /// 支持小数部分帧  10.2 FrameID  * 100ms = 实际的服务器时间
        /// </summary>
        /// <returns></returns>
        public float GetPredictServerFrame()
        {
            if (lastSyncTime > 0)
            {
                //var passTime = Time.time - lastSyncTime + NetworkLatency.LatencyTime / 2;
                var passTime = Time.time - lastSyncTime + 0.05f;
                var passFrame = passTime / 0.1f;
                Log.Net("GetPredictServerFrame:"+Time.time+":"+ServerFrameId+":"+lastSyncTime+":"+NetworkLatency.LatencyTime);
                return (ServerFrameId + passFrame);
                //固定偏移
                //return ServerFrameId + 0.5f;
            }else
            {
                return ServerFrameId;
            }
        }

        /// <summary>
        /// 获取客户端预测当前相对于服务器命令的时间
        /// </summary>
        /// <param name="cmdFrameId"></param>
        /// <returns></returns>
        public float GetPredictPassServerTime(ulong cmdFrameId)
        {
            var now = GetPredictServerFrame();
            var passTime = now - cmdFrameId;
            return (float)(passTime * 0.1f);
        }


        public int myId;
        MainThreadLoop ml;

        public RemoteClient rc
        {
            get; private set; }
        private WorldState _s = WorldState.Idle;
        private string ServerIP = "127.0.0.1";

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

        public static NetworkScene Instance;

        void Awake()
        {
            Instance = this;
            InitDataMgr();
            StartCoroutine(InitGameData());

            ml = gameObject.AddComponent<MainThreadLoop>();

            TextAsset bindata = Resources.Load("Config") as TextAsset;
            Debug.Log("nameMap " + bindata);
          
            ServerIP = ClientApp.Instance.remoteServerIP;
            Debug.LogError("ServerIP: " + ServerIP);

            state = WorldState.Connecting;
            //StartCoroutine(InitConnect());
        }
        private void InitDataMgr()
        {
            MobaDataMgr.Init();
        }

        /// <summary>
        /// 初始化网络对战场景里面的属性状态：
        /// 设置等级
        /// 重置技能 
        /// </summary>
        /// <returns></returns>
        IEnumerator InitGameData()
        {
            yield return null;
            /*
            GameInterface_Backpack.ClearDrug();
            yield return StartCoroutine(NetworkUtil.WaitForWorldInit());
            var me = ObjectManager.objectManager.GetMyPlayer();
            var sync = CGSetProp.CreateBuilder();
            sync.Key = (int)CharAttribute.CharAttributeEnum.LEVEL;
            sync.Value = 1;
            KBEngine.Bundle.sendImmediate(sync);
            PlayerData.ResetSkillLevel();
            */
        }


        private RemoteClientEvent lastEvt = RemoteClientEvent.None;

        void EvtHandler(RemoteClientEvent evt)
        {
            Debug.LogError("RemoteClientEvent: " + evt);
            lastEvt = evt;
            if (lastEvt == RemoteClientEvent.Close)
            {
                WindowMng.windowMng.ShowNotifyLog("和服务器断开连接：" + state);
                if (state != WorldState.Closed)
                {
                    //Debug.LogError("ConnectionClosed But WorldNotClosed");
                    state = WorldState.Idle;
                    //StartCoroutine(RetryConnect());
                    //WindowMng.windowMng.ShowNotifyLog("重练尚未实现");
                    //StartCoroutine(ReEnter());
                    StartCoroutine(QuitScene());
                }
            } else if (lastEvt == RemoteClientEvent.Connected)
            {
                WindowMng.windowMng.ShowNotifyLog("连接服务器成功：" + state);
            }
        }

        private IEnumerator QuitScene()
        {
            Log.Sys("QuitScene");
            WorldManager.worldManager.WorldChangeScene(2, false);
            yield break;
        }

        public void MsgHandler(KBEngine.Packet packet)
        {
            var proto = packet.protoBody as GCPlayerCmd;
            Log.Net("Map4Receive: " + proto);
            var cmds = proto.Result.Split(' ');
            var cmd0 = cmds[0];
            if (cmds [0] == "Add")
            {
                ObjectManager.objectManager.CreateOtherPlayer(proto.AvatarInfo);
                //PlayerDataInterface.DressEquip(proto.AvatarInfo);
                var player = ObjectManager.objectManager.GetPlayer(proto.AvatarInfo.Id);
                if (player != null)
                {
                    var sync = player.GetComponent<ISyncInterface>();

                    if (!proto.AvatarInfo.HasScore)
                    {
                        proto.AvatarInfo.Score = 0;
                    }
                    if (sync != null)
                    {
                        var ainfo = NetMatchScene.Instance.matchRoom.GetPlayerInfo(proto.AvatarInfo.Id);
                        sync.InitSync(ainfo);
                        sync.NetworkAttribute(proto);
                    }
                }

            } else if (cmds [0] == "Remove")
            {
                ObjectManager.objectManager.DestroyPlayer(proto.AvatarInfo.Id); 
            } else if (cmds [0] == "Update")
            {
                var player = ObjectManager.objectManager.GetPlayer(proto.AvatarInfo.Id);
                if (player != null)
                {
                    var sync = player.GetComponent<ISyncInterface>();
                    if(sync != null)
                    {
                        sync.NetworkAttribute(proto);
                    }
                } 

            } else if (cmds [0] == "Damage")
            {
                var dinfo = proto.DamageInfo;
                var enemy = ObjectManager.objectManager.GetPlayer(dinfo.Enemy);
                if (enemy != null)
                {
                    var sync = enemy.GetComponent<ISyncInterface>();
                    if (sync != null)
                    {
                        sync.DoNetworkDamage(proto);
                    }
                }

            } else if (cmds [0] == "Skill")
            {
                var sk = proto.SkillAction;
                var player = ObjectManager.objectManager.GetPlayer(sk.Who);
                if (player != null)
                {
                    var sync = player.GetComponent<ISyncInterface>();
                    if (sync != null)
                    {
                        sync.NetworkAttack(proto);
                    }
                }
            } else if (cmds [0] == "Buff")
            {
                var target = proto.BuffInfo.Target;
                var player = ObjectManager.objectManager.GetPlayer(target);
                if (player != null)
                {
                    var sync = player.GetComponent<ISyncInterface>();
                    if(sync != null)
                    {
                        sync.NetworkBuff(proto);
                    }
                }
                
                /*
                var target = proto.BuffInfo.Target;
                var sync = NetDateInterface.GetPlayer(target);
                var player = ObjectManager.objectManager.GetPlayer(target);
                if (sync != null)
                {
                    sync.NetworkBuff(proto);
                }
                else if(player != null)
                {
                    var sync2 = player.GetComponent<MySelfAttributeSync>();
                    if (sync2 != null)
                    {
                        sync2.NetworkBuff(proto);
                    }
                }
                if (player != null && !NetworkUtil.IsNetMaster())
                {
                    var monSync = player.GetComponent<MonsterSync>();
                    if (monSync != null)
                    {
                        monSync.NetworkBuff(proto);
                    }
                }
                */
            }
            else if (cmds[0] == "RemoveBuff")
            {
                var target = proto.BuffInfo.Target;
                var player = ObjectManager.objectManager.GetPlayer(target);
                if(player != null)
                {
                    var sync = player.GetComponent<ISyncInterface>();
                    if(sync != null)
                    {
                        sync.NetworkRemoveBuff(proto);
                    }
                }
             
            }
            else if (cmds [0] == "AddEntity")
            {
                var ety = proto.EntityInfo;
                if (ety.EType == EntityType.CHEST)
                {
                    WaitZoneInit(ety);
                } else if (ety.EType == EntityType.DROP)
                {
                    var itemData = Util.GetItemData((int)ItemData.GoodsType.Props, (int)ety.ItemId);
                    var itemNum = ety.ItemNum;
                    var pos = NetworkUtil.FloatPos(ety.X, ety.Y, ety.Z); 
                    DropItemStatic.MakeDropItemFromNet(itemData, pos, itemNum, ety);
                }

            } else if (cmds [0] == "UpdateEntity")
            {
                var ety = proto.EntityInfo;
                var mon = ObjectManager.objectManager.GetPlayer(ety.Id);
                Log.Net("UpdateEntityHP: " + ety.Id + " hp " + ety.HasHP + " h " + ety.HP+":"+ety+":"+mon);
                //if (!NetworkUtil.IsMaster() && mon != null)
                if(mon != null)
                {
                    var sync = mon.GetComponent<MonsterSync>();
                    if (sync != null)
                    {
                        sync.NetworkAttribute(proto);
                    }
                }
            } else if (cmds [0] == "RemoveEntity")
            {
                var ety = proto.EntityInfo;
                var mon = ObjectManager.objectManager.GetPlayer(ety.Id);
                //if (!NetworkUtil.IsMaster() && mon != null)
                if(mon != null)
                {
                    var netView = mon.GetComponent<KBEngine.KBNetworkView>();
                    if (netView != null)
                    {
                        ObjectManager.objectManager.DestroyByLocalId(netView.GetLocalId());
                    }
                }
            } else if (cmds [0] == "Pick")
            {
                //if (!NetworkUtil.IsMaster())
                {
                    var action = proto.PickAction;
                    var ety = ObjectManager.objectManager.GetPlayer(action.Id);
                    var who = ObjectManager.objectManager.GetPlayer(action.Who);
                    if (ety != null)
                    {
                        var item = ety.GetComponent<DropItemStatic>();
                        if (item != null)
                        {
                            item.PickItemFromNetwork(who);
                        }
                    }
                }
            } else if (cmds [0] == "Revive")
            {
                var player = ObjectManager.objectManager.GetPlayer(proto.AvatarInfo.Id);
                if (player != null)
                {
                    var sync = player.GetComponent<ISyncInterface>();
                    if (sync != null)
                    {
                        sync.Revive(proto);
                    }
                }
            }else if(cmd0 == "DeadActor")
            {
                var player = ObjectManager.objectManager.GetPlayer(proto.ActorId);
                if(player != null)
                {
                    var sync = player.GetComponent<ISyncInterface>();
                    if(sync != null)
                    {
                        sync.Dead(proto);
                    }
                }
            }
            else if (cmds [0] == "Dead")
            {
                //ScoreManager.Instance.NetAddScore(dinfo.Attacker, dinfo.Enemy);
                ScoreManager.Instance.Dead(proto);
            } else if (cmds [0] == "SyncTime")
            {
                //if (!NetworkUtil.IsNetMaster())
                ScoreManager.Instance.NetSyncTime(proto.LeftTime);
            } else if (cmds [0] == "GameOver")
            {
                //if (!NetworkUtil.IsNetMaster())
                {
                    ScoreManager.Instance.NetworkGameOver();
                }
            } else if (cmds [0] == "AllReady")
            {
                Util.ShowMsg("所有客户端准备完成");
                //当所有客户端准备好之后 服务器推送Entity给所有客户端
                NetMatchScene.Instance.SetAllReady();
                
            }else if (cmds[0] == "News")
            {
                var con = proto.News;
                Util.ShowMsg(con);
            }else if (cmds[0] == "UDPLost")
            {
                Util.ShowMsg("Server Side UDPLost!");
                UDPLost();
            }else if(cmd0 == "SyncFrame")
            {
                SyncFrame(proto);
            }
        }

        private void SyncFrame(GCPlayerCmd proto)
        {
            ServerFrameId = proto.FrameId;
            lastSyncTime = Time.time;
        }

        private void UDPLost()
        {
            if (udpClient != null)
            {
                udpClient.Quit();
                udpClient = null;
            }
        }

        void WaitZoneInit(EntityInfo ety)
        {
            var unitData = Util.GetUnitData(false, ety.UnitId, 0);
            ObjectManager.objectManager.CreateSpawnZoneEntity(unitData, ety);
        }

        private RemoteUDPClient udpClient = null; 
        private KCPClient kcpClient = null;
        //设置发送Ready
        //当所有的Ready之后Master 会发送一个Go状态
        public void InitMap()
        {
            rc = NetMatchScene.Instance.rc;

            /*
            udpClient = new RemoteUDPClient(rc.GetMainLoop(), this);
            udpClient.Connect(ClientApp.Instance.remoteServerIP, ClientApp.Instance.remoteUDPPort, ClientApp.Instance.UDPListenPort);

            kcpClient = new KCPClient(rc.GetMainLoop(), this);
            kcpClient.closeEventHandler = this.CloseEventHandler;
            kcpClient.Connect(ClientApp.Instance.remoteServerIP, ClientApp.Instance.remoteKCPPort);
            */

            StartCoroutine(SendReady());
            //StartCoroutine(CheckUDPState());
        }

        /// <summary>
        /// 下层自上层的断开连接 
        /// 是否重连
        /// </summary>
        private void CloseEventHandler() {
            if(state != WorldState.Closed) { 
                kcpClient = new KCPClient(rc.GetMainLoop(), this);
                kcpClient.closeEventHandler = this.CloseEventHandler;
                kcpClient.Connect(ClientApp.Instance.remoteServerIP, ClientApp.Instance.remoteKCPPort);
                //重新初始化 食物的状态请求
            }
        }


        private IEnumerator CheckUDPState()
        {
            var wt = new WaitForSeconds(5);
            while (true)
            {
                if (udpClient != null && udpClient.Connected)
                {
                    var now = Time.time;
                    var last = udpClient.lastReceiveTime;
                    if (udpClient.receiveYet)
                    {
                        if (now - last > 5)
                        {
                            //UDP 连接断开之后 走TCP进行数据广播
                            Util.ShowMsg("Client UDP Connect Lost");
                            UDPLost();
                            //通知服务器切断UDP连接
                            var cg = CGPlayerCmd.CreateBuilder();
                            cg.Cmd = "UDPLost";
                            this.BroadcastMsg(cg);
                            break;
                        }
                    }
                }
                yield return wt;
            }
        }


        private IEnumerator SendReady()
        {
            while (ObjectManager.objectManager.GetMyPlayer() == null)
            {
                yield return null;
            }
            //等待NetworkLoadZone 初始化完成
            yield return new WaitForSeconds(0.2f);
            rc.evtHandler = EvtHandler;
            rc.msgHandler = MsgHandler;
            state = WorldState.Connected;
            myId = NetMatchScene.Instance.myId;
            ObjectManager.objectManager.RefreshMyServerId(myId);

            var cg = CGPlayerCmd.CreateBuilder();
            cg.Cmd = "Ready";
            Bundle bundle;
            var data = KBEngine.Bundle.GetPacket(cg, out bundle);
            rc.Send(data, bundle);
            yield return StartCoroutine(SendUserData());
            yield return StartCoroutine(SendCommandToServer());
        }


        private void SyncUDPPos()
        {
            var me = ObjectManager.objectManager.GetMyPlayer();
            if (me != null)
            {
                var sync = me.GetComponent<MeSyncToServer>();
                sync.SyncPos();
            }
        }

        void SyncMyAttribute()
        {
            var me = ObjectManager.objectManager.GetMyPlayer();
            if (me != null)
            {
                var sync = me.GetComponent<MeSyncToServer>();
                sync.SyncAttribute();
            }
        }


        /// <summary>
        /// 周期性的同步属性状态到服务器上面 Diff属性
        /// lastAvatarInfo 比较后的属性 
        /// 操作命令在彻底和服务器同步之后开始发送
        /// </summary>
        /// <returns>The command to server.</returns>
        IEnumerator SendCommandToServer()
        {
            Debug.LogError("SendCommandToServer");
            var waitTime = new WaitForSeconds(ClientApp.Instance.syncFreq);

            //客户端每隔100ms向服务器同步一次操控位置信息
            //等待服务器通知AllReady才开始设置属性
            while(NetMatchScene.Instance.roomState != NetMatchScene.RoomState.AllReady)
            {
                yield return null;
            }

            var me = ObjectManager.objectManager.GetMyPlayer();
            if (me != null)
            {
                var sync = me.GetComponent<ISyncInterface>();
                while (!sync.CheckSyncState())
                {
                    yield return null;
                }
            }
            while (state == WorldState.Connected)
            {
                SyncMyAttribute();
                SyncUDPPos();
                yield return waitTime;
            }
        }

        IEnumerator SendUserData()
        {
            Debug.LogError("SendUserData: "+state+" rc "+rc);
            if (state != WorldState.Connected)
            {
                yield break;
            }
            if (rc == null)
            {
                yield break;
            }

            var me = ObjectManager.objectManager.GetMyPlayer();
            var pos = me.transform.position;
            var dir = (int)me.transform.localRotation.eulerAngles.y;

            var cg = CGPlayerCmd.CreateBuilder();
            cg.Cmd = "InitData";
            var ainfo = AvatarInfo.CreateBuilder();
            ainfo.X = (int)(pos.x * 100);
            ainfo.Z = (int)(pos.z * 100);
            ainfo.Y = (int)(pos.y * 100);
            ainfo.Dir = dir;
            ainfo.Name = ServerData.Instance.playerInfo.Roles.Name;

            ainfo.Level = ObjectManager.objectManager.GetMyProp(CharAttribute.CharAttributeEnum.LEVEL);
            ainfo.HP = ObjectManager.objectManager.GetMyProp(CharAttribute.CharAttributeEnum.HP);
            ainfo.Job = ServerData.Instance.playerInfo.Roles.Job;

            cg.AvatarInfo = ainfo.Build();
            var sync = me.GetComponent<MeSyncToServer>();
            sync.InitData(cg.AvatarInfo);

            Bundle bundle;
            var data = KBEngine.Bundle.GetPacketFull(cg, out bundle);
            yield return StartCoroutine(rc.SendWaitResponse(data.data, data.fid, (packet) =>
            {
            }, bundle));
            WindowMng.windowMng.ShowNotifyLog("玩家数据初始化成功");
        }


        void QuitWorld()
        {
            Debug.LogError("QuitWorld");
            state = WorldState.Closed;
            if (rc != null)
            {
                rc.evtHandler = null;
                rc.msgHandler = null;
                rc.Disconnect();
                rc = null;
            }
            if (udpClient != null)
            {
                udpClient.Quit();
                udpClient = null;
            }
            if(kcpClient != null) {
                kcpClient.Close();
                kcpClient = null;
            }
        }

        void  OnDestroy()
        {
            if (NetMatchScene.Instance != null)
            {
                UnityEngine.Object.Destroy(NetMatchScene.Instance.gameObject);
            }
            QuitWorld();
        }

        public void BroadcastMsg(CGPlayerCmd.Builder cg)
        {
            //客户端时间戳
            cg.FrameId = NetworkScene.Instance.GetPredictServerTimeForNet();
            Log.Net("BroadcastMsg: " + cg.Build()+":"+GetSyncCmdFrame());
            if (rc != null)
            {
                Bundle bundle;
                var data = KBEngine.Bundle.GetPacket(cg, out  bundle);
                rc.Send(data, bundle);
            }
        }

        /// <summary>
        /// UDP 通道失效的时候使用TCP通道
        /// </summary>
        /// <param name="cg"></param>
        public void BroadcastUDPMsg(CGPlayerCmd.Builder cg)
        {
            if (udpClient != null && !udpClient.IsClose && udpClient.Connected)
            {
                udpClient.SendPacket(cg);
            }
            else
            {
                BroadcastMsg(cg);
            } 
        }

        public void BroadcastKCPMsg(CGPlayerCmd.Builder cg) {
            if(kcpClient != null && !kcpClient.IsClose && kcpClient.Connected) {
                kcpClient.SendPacket(cg);
            }else {
                BroadcastMsg(cg);
            }
        }
    }
}
