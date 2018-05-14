using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


namespace MyLib
{
    /// <summary>
    /// 同步服务器上的怪物属性和操作 
    /// </summary>
    public class MonsterSync : ISyncInterface 
    {
        AIBase aibase;
        public EntityInfo curInfo;
        private NpcAttribute attr;
        private List<EntityInfo> positions = new List<EntityInfo>();

        public override void AddFakeMove()
        {
            throw new NotImplementedException();
        }

        public override Vector3 GetCurInfoPos()
        {
            return MobaUtil.FloatPos(curInfo);
        }
        public override Vector3 GetCurInfoSpeed()
        {
            return MobaUtil.FloatPos(new Vector3(curInfo.SpeedX, 0, curInfo.SpeedY));
        }

        public override MyVec3 GetServerVelocity()
        {
            if (positions.Count > 0)
            {
                var p1 = positions[positions.Count - 1];
                return new MyVec3(p1.SpeedX, 0, p1.SpeedY);
            }
            return MyVec3.zero;
        }
        /// <summary>
        /// 服务器还需要同步Speed用于表示Entity是否移动的一个状态
        /// </summary>
        /// <returns></returns>
        public override Vector3 GetServerPos()
        {
            if (positions.Count > 0)
            {
                var p1 = positions[positions.Count - 1];
                var passTime = NetworkScene.Instance.GetPredictPassServerTime(p1.FrameID);
                var speed = NetworkUtil.FloatPos(p1.SpeedX, 0, p1.SpeedY);
                if (passTime > 0)
                {
                    return MobaUtil.FloatPos(p1) + speed * (passTime + Util.FrameSecTime);
                }
                else
                {
                    return MobaUtil.FloatPos(p1) + speed * Util.FrameSecTime;
                }
            }
            else
            {
                return MobaUtil.FloatPos(curInfo);
            }

            /*
            var passTime = NetworkScene.Instance.GetPredictPassServerTime(curInfo.FrameID);
            //客户端跑的比服务器快 外插值
            if (passTime > 0)
            {
                var c = positions.Count;
                if (c >= 2)
                {
                    var p1 = positions[c - 1];
                    var p0 = positions[c - 2];
                    var deltaFrame = p1.FrameID - p0.FrameID;
                    //速度没有归0
                    if (deltaFrame > 0)
                    {
                        var tm = MobaUtil.DeltaFrameToTime(deltaFrame);
                        var sp = MobaUtil.DeltaPos(p1, p0);
                        var speed = MobaUtil.Speed(sp, tm);

                        //var speed = new Vector3(curInfo.SpeedX, 0, curInfo.SpeedY);
                        speed = Util.NetToGameVec(speed);
                        var extraPos = passTime * speed;
                        return MobaUtil.FloatPos(curInfo) + extraPos;
                    }else
                    {
                        return MobaUtil.FloatPos(curInfo);
                    }
                }else
                {
                    return MobaUtil.FloatPos(curInfo);
                }
            }else
            {
                return MobaUtil.FloatPos(curInfo);
            }
            */
        }
        public override void InitSetPos(Vector3 pos)
        {
            //MobaUtil.SetPosWithHeight(gameObject, pos);
            transform.position = pos;
        }

        private void Start()
        {
            attr = GetComponent<NpcAttribute>();
            gameObject.AddComponent<DebugServerPos>();
        }

        public override void DoNetworkDamage(GCPlayerCmd cmd)
        {
            MobaUtil.DoNetworkDamage(gameObject, cmd);
        }

        /// <summary>
        /// 击退效果 直接添加buff
        /// </summary>
        /// <param name="cmd"></param>
        public override void NetworkBuff(GCPlayerCmd cmd)
        {
            var sk = Util.GetSkillData(cmd.BuffInfo.SkillId, 1);
            var skConfig = SkillLogic.GetSkillInfo(sk);
            var evt = skConfig.GetEvent(cmd.BuffInfo.EventId);
            if (evt != null)
            {
                var pos = cmd.BuffInfo.AttackerPosList;
                var px = pos [0] / 100.0f;
                var py = pos [1] / 100.0f;
                var pz = pos [2] / 100.0f;
                gameObject.GetComponent<BuffComponent>().AddBuff(evt.affix, new Vector3(px, py, pz), 0, cmd.BuffInfo.BuffId);
            }
        }

        public void SyncAttribute(EntityInfo info) {
            //Log.Sys("SyncMonAttr: "+info);
            curInfo.FrameID = NetworkScene.Instance.GetSyncCmdFrame();
            var attr = gameObject.GetComponent<NpcAttribute>();
            if (info.HasTeamColor)
            {
                curInfo.TeamColor = info.TeamColor;
            }
            NetworkMove(info);
            if (info.HasHP)
            {
                attr.SetHPNet(info.HP);
            }
            if (info.HasLifeLeft)
            {
                curInfo.LifeLeft = info.LifeLeft;
            }
            if (info.HasPlayerID)
            {
                curInfo.PlayerID = info.PlayerID;
            }
        }
        public override void NetworkAttribute(GCPlayerCmd cmd)
        {
            var info = cmd.EntityInfo;
            SyncAttribute(info);
        }

        public override void InitSync(AvatarInfo info)
        {
            throw new NotImplementedException();
        }

        public override void NetworkAttack(GCPlayerCmd cmd)
        {
            MobaUtil.DoNetworkAttack(gameObject, cmd);
        }

        public override void Revive(GCPlayerCmd gc)
        {
            throw new NotImplementedException();
        }
        public override void SetLevel(AvatarInfo info)
        {
            throw new NotImplementedException();
        }
        public override void SetPositionAndDir(AvatarInfo info)
        {
            throw new NotImplementedException();
        }
        public override void NetworkRemoveBuff(GCPlayerCmd cmd)
        {
            throw new NotImplementedException();
        }
        public override void Dead(GCPlayerCmd cmd)
        {
            MobaUtil.Dead(gameObject);
        }

        /// <summary>
        /// 相当于每一阵率都去检查
        /// 表现层如何保证自己忠实的执行完了命令呢
        /// 表现层没有寻路逻辑，导致可能被卡住
        /// 表现层并不能完美重现服务器上的移动过程
        /// 
        /// 如何解决命令没有完全执行的问题 客户端命令队列
        /// </summary>
        private void Awake()
        {
            curInfo = EntityInfo.CreateBuilder().Build();
        }

        /// <summary>
        /// 网络移动
        /// </summary>
        /// <param name="info"></param>
        private void NetworkMove(EntityInfo info)
        {
            if (info.HasX)
            {
                curInfo.X = info.X;
                curInfo.Y = info.Y;
                curInfo.Z = info.Z;

                //curInfo.Dir = info.Dir;
                info.FrameID = NetworkScene.Instance.GetSyncCmdFrame();

                positions.Add(info);
                //不是ResetPos
                if(positions.Count > 5)
                {
                    positions.RemoveAt(0);
                }
            }
            /*
            else
            {
                var fakeInfo = EntityInfo.CreateBuilder();
                fakeInfo.X = curInfo.X;
                fakeInfo.Y = curInfo.Y;
                fakeInfo.Z = curInfo.Z;
                fakeInfo.FrameID = NetworkScene.Instance.GetSyncCmdFrame();
                positions.Add(fakeInfo.Build());
                if(positions.Count > 5)
                {
                    positions.RemoveAt(0);
                }
            }
            */

            if(info.HasSpeedX)
            {
                curInfo.SpeedX = info.SpeedX;
                curInfo.SpeedY = info.SpeedY;
                //curInfo.FrameID = NetworkScene.Instance.GetSyncCmdFrame();
                //info.FrameID = NetworkScene.Instance.GetSyncCmdFrame();
                //服务器尝试停止移动
                if(info.SpeedX == 0 && info.SpeedY == 0)
                {

                }
            }

            if (info.HasDir)
            {
                curInfo.Dir = info.Dir;
            }
        }

        /// <summary>
        /// 怪物统一使用 Rigidbody 控制
        /// 子弹型刚体
        /// </summary>
        private void SendMoveCmd()
        {
            if (!curInfo.HasX)
            {
                return;
            }
            var dir = (int)transform.localRotation.eulerAngles.y;
            var meAttr = gameObject.GetComponent<NpcAttribute>();
            var serverPos = NetworkUtil.FloatPos(curInfo.X, curInfo.Y, curInfo.Z);
            var cmd = new MoveCMD();
            cmd.targetPos = serverPos;
            cmd.dir = curInfo.Dir;
            //cmd.speed = Util.NetToGameNum(curInfo.Speed);
            GetComponent<LogicCommand>().PushCommand(cmd);
        }

        public override bool CheckSyncState()
        {
            return true;
        }
    }

}