using UnityEngine;
using System.Collections;

namespace MyLib
{
    /// <summary>
    /// 自己玩家向服务器同步状态
    /// </summary>
    /*
    public class PlayerSyncToServer : MeSyncToServer 
    {
        AvatarInfo lastInfo;
        AvatarInfo curInfo;
        //frameId 服务器发送的每个玩家对应的frameId 每个玩家有自己的一套 否则flowId 不怎么够用
        private byte frameId = 0;
        private bool low = true;

        private MoveController mv;

        private NpcAttribute attr;
        void Start()
        {
            mv = GetComponent<MoveController>();
            attr = GetComponent<NpcAttribute>();
        }

        public override void InitData(AvatarInfo info)
        {
            lastInfo = AvatarInfo.CreateBuilder(info).Build();
            curInfo = AvatarInfo.CreateBuilder(info).Build();
        }

        private float lastSyncTime = 0;
        public override void SyncPos()
        {
            var me = gameObject;
            var pos = me.transform.position;
            var dir = (int) me.transform.localRotation.eulerAngles.y;
            var intPos = NetworkUtil.ConvertPos(pos);
            var towerDir = (int) me.GetComponent<TankPhysicComponent>().tower.transform.eulerAngles.y;

            var ainfo = AvatarInfo.CreateBuilder();
            ainfo.Id = GetComponent<NpcAttribute>().GetNetView().GetServerID();
            var change = false;
            //第一个报文用于建立UDP连接 在UDP连接没有建立起来一直走TCP
            if (frameId == 1)
            {
                change = true;
            }
            var diffTime = Time.time - lastSyncTime;
            //防止UDP连接断开
            if (diffTime > 1 || (intPos[0] != lastInfo.X || intPos[1] != lastInfo.Y || intPos[2] != lastInfo.Z || dir != lastInfo.Dir))
            {
                lastSyncTime = Time.time;
                ainfo.X = intPos[0];
                ainfo.Y = intPos[1];
                ainfo.Z = intPos[2];
                ainfo.Dir = dir;

                curInfo.X = ainfo.X;
                curInfo.Y = ainfo.Y;
                curInfo.Z = ainfo.Z;
                curInfo.Dir = dir;
                change = true;

                lastInfo.X = ainfo.X;
                lastInfo.Y = ainfo.Y;
                lastInfo.Z = ainfo.Z;
                lastInfo.Dir = ainfo.Dir;
            }

            if (dir != lastInfo.Dir)
            {
                ainfo.Dir = dir;
                curInfo.Dir = dir;
                lastInfo.Dir = ainfo.Dir;

                change = true;
            }


            if (lastInfo.TowerDir != towerDir)
            {
                ainfo.TowerDir = towerDir;
                curInfo.TowerDir = towerDir;
                change = true;
                lastInfo.TowerDir = ainfo.TowerDir;
            }

            if (mv != null)
            {
                var min = CursorManager.cursorManager.MoveInput;
                var iv = min;
                var ms = GameConst.Instance.MoveSpeed*attr.GetMoveSpeedCoff();
                var nx = (int)(iv.x*ms*110);
                var ny = (int) (iv.y*ms*110);
                var curState = attr.GetComponent<AIBase>().GetAI().state;
                if (curState.type == AIStateEnum.DEAD)
                {
                    nx = 0;
                    ny = 0;
                }

                if (nx != lastInfo.SpeedX || ny != lastInfo.SpeedY)
                {
                    ainfo.SpeedX = nx;
                    ainfo.SpeedY = ny;

                    lastInfo.SpeedX = nx;
                    lastInfo.SpeedY = ny;

                    change = true;
                }
            }

            if (change)
            {
                var cg = CGPlayerCmd.CreateBuilder();
                cg.Cmd = "UpdateData";
                var nf = frameId++;
                var find = false;
                if (low && nf > 127)
                {
                    find = true;
                    low = false;
                }
                else if (nf == 0)
                {
                    find = true;
                    low = true;
                }
                ainfo.FrameID = nf;
                cg.AvatarInfo = ainfo.Build();
                //报文区间变化 TCP稳定通知
                if (find)
                {
                    NetworkUtil.Broadcast(cg);
                }
                else
                {
                    NetworkUtil.BroadcastUDP(cg);
                }
            }
        }

        public override void SyncAttribute()
        {
            var me = gameObject;
            var pos = me.transform.position;
            var dir = (int)me.transform.localRotation.eulerAngles.y;
            var meAttr = me.GetComponent<NpcAttribute>();
            var intPos = NetworkUtil.ConvertPos(pos);
            var towerDir = (int)me.GetComponent<TankPhysicComponent>().tower.transform.eulerAngles.y;

            var ainfo = AvatarInfo.CreateBuilder();
            var change = false;

            if (meAttr.HP != lastInfo.HP)
            {
                ainfo.HP = meAttr.HP;
                curInfo.HP = meAttr.HP;
                change = true;

                lastInfo.HP = ainfo.HP;
            }
            var intNetSpeed = (int)(meAttr.NetSpeed * 100);
            if (intNetSpeed != lastInfo.NetSpeed)
            {
                ainfo.NetSpeed = intNetSpeed;
                curInfo.NetSpeed = intNetSpeed;
                change = true;

                lastInfo.NetSpeed = ainfo.NetSpeed;
            }
            var intThrowSpeed = (int)(meAttr.ThrowSpeed * 100);
            if (intThrowSpeed != lastInfo.ThrowSpeed)
            {
                ainfo.ThrowSpeed = intThrowSpeed;
                curInfo.ThrowSpeed = intThrowSpeed;
                change = true;

                lastInfo.ThrowSpeed = ainfo.ThrowSpeed;
            }
            var intJumpSpeed = (int)(meAttr.JumpForwardSpeed * 100);
            if (intJumpSpeed != lastInfo.JumpForwardSpeed)
            {
                ainfo.JumpForwardSpeed = intJumpSpeed;
                curInfo.JumpForwardSpeed = intJumpSpeed;
                change = true;

                lastInfo.JumpForwardSpeed = ainfo.JumpForwardSpeed;
            }

            if(!string.Equals(meAttr.userName, lastInfo.Name)) {
                ainfo.Name = meAttr.userName;
                curInfo.Name = meAttr.userName;
                change = true;
                lastInfo.Name = ainfo.Name;
            }

            if(lastInfo.Job != ServerData.Instance.playerInfo.Roles.Job) {
                ainfo.Job = ServerData.Instance.playerInfo.Roles.Job;
                curInfo.Job = ainfo.Job;
                change = true;
                lastInfo.Job = ainfo.Job;
            }


            if (change)
            {
                //lastInfo = AvatarInfo.CreateBuilder(curInfo).Build();

                var cg = CGPlayerCmd.CreateBuilder();
                cg.Cmd = "UpdateData";
                cg.AvatarInfo = ainfo.Build();
                NetworkUtil.Broadcast(cg);
            }
        }

    }
    */

}