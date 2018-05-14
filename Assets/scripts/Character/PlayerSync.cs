
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

using System;
using KBEngine;

namespace MyLib
{

    /// <summary>
    /// 其它玩家的网络同步
    /// 网络对象的本地代理
    /// Proxy 接受网络同步 
    /// </summary>
    /*
    public class PlayerSync :  ISyncInterface
    {
        AvatarInfo curInfo;
        NpcAttribute attribute;
        private TowerAutoCheck tower;
        private int lastFrameId = -1;

        public override void InitSync(AvatarInfo info)
        {
            Log.Sys("InitSync: "+info);
            if (info != null)
            {
                if (info.HasTeamColor)
                {
                    GetComponent<NpcAttribute>().SetTeamColorNet(info.TeamColor);
                }
            }
        }

        public override void NetworkAttribute(GCPlayerCmd gc)
        {
            var info = gc.AvatarInfo;
            Log.Sys("NetworkMove: " + info);
            if (info.HasFrameID)
            {
                var newFrameId = info.FrameID;
                if (lastFrameId == -1) //未曾初始化过位置
                {

                }
                else if (info.LowChange)
                {
                }
                else if (newFrameId > 127 && lastFrameId <= 127)
                {
                    return;
                }
                else if (newFrameId <= 127 && lastFrameId > 127)
                {
                    return;
                }
                else if (newFrameId <= lastFrameId)
                {
                    return;
                }
                lastFrameId = newFrameId;
            }

            //初始化玩家没有初始化位置坐标 初始化的时候没有FrameID
            //其它玩家的位置根据延迟和速度进行预测
            if (info.HasX)
            {
                var lag = NetworkLatency.LatencyTime;
                var predictLag = Mathf.Max(lag*2, 0.1f);

                curInfo.SpeedX = info.SpeedX;
                curInfo.SpeedY = info.SpeedY;
                curInfo.X = (int)(info.X+predictLag*info.SpeedX);
                curInfo.Y = info.Y;
                curInfo.Z = (int)(info.Z+predictLag*info.SpeedY);
                curInfo.Dir = info.Dir;
                SendMoveCmd();
                if (info.LowChange)
                {
                    var sp = NetworkUtil.FloatPos(curInfo.X, curInfo.Y, curInfo.Z);
                    //this.rigidbody.MovePosition();
                    Util.ForceResetPos(this.GetComponent<Rigidbody>(), sp);
                }
            }

            if (info.HasTowerDir)
            {
                curInfo.TowerDir = info.TowerDir;
                if (tower != null)
                {
                    tower.SyncTowerDir(curInfo.TowerDir);
                }
            }


            var attr = GetComponent<NpcAttribute>();

            if (info.HasJob)
            {
                //attr.job = info.Job;
                attr.SetJob(info.Job);
            }
            if (info.HasHP)
            {
                GetComponent<NpcAttribute>().SetHPNet(info.HP);
            }

            if (info.HasTeamColor)
            {
                GetComponent<NpcAttribute>().SetTeamColorNet(info.TeamColor);
            }
            if (info.HasNetSpeed)
            {
                GetComponent<NpcAttribute>().NetSpeed = info.NetSpeed/100.0f;
            }
            if (info.HasThrowSpeed)
            {
                attr.ThrowSpeed = info.ThrowSpeed/100.0f;
            }
            if (info.HasJumpForwardSpeed)
            {
                attr.JumpForwardSpeed = info.JumpForwardSpeed/100.0f;
                curInfo.JumpForwardSpeed = info.JumpForwardSpeed;
            }
            if (info.HasName)
            {
                attr.userName = info.Name;
                MyEventSystem.myEventSystem.PushLocalEvent(attr.GetLocalId(), MyEvent.EventType.TeamColor);
                MyEventSystem.myEventSystem.PushEvent(MyEvent.EventType.UpdateName);
            }

            var dataChanged = false;
            var skillCount = curInfo.KillCount;
            if (info.HasKillCount)
            {
                skillCount = info.KillCount;
                curInfo.KillCount = info.KillCount;
                dataChanged = true;
            }

            var deadCount = curInfo.DeadCount;
            if (info.HasDeadCount)
            {
                deadCount = info.DeadCount;
                curInfo.DeadCount = info.DeadCount;
                dataChanged = true;
            }

            var secondaryAttackCount = curInfo.SecondaryAttackCount;
            if (info.HasSecondaryAttackCount)
            {
                secondaryAttackCount = info.SecondaryAttackCount;
                curInfo.SecondaryAttackCount = info.SecondaryAttackCount;
                dataChanged = true;
            }

            if (info.HasScore)
            {
                curInfo.Score = info.Score;
                dataChanged = true;
            }

            if(dataChanged)
            {
                var sid = attr.GetNetView().GetServerID();
                ScoreManager.Instance.NetSyncScore(sid, curInfo.Score, sid, skillCount, deadCount, secondaryAttackCount);
            }

            if (info.HasContinueKilled)
            {
                curInfo.ContinueKilled = info.ContinueKilled;
                if (curInfo.ContinueKilled > 0)
                {
                    //ScoreManager.Instance.PlayerKillSound(curInfo.ContinueKilled);
                }
            }

            if (info.HasSkillAction)
            {
                info.SkillAction.X = curInfo.X;
                info.SkillAction.Y = curInfo.Y;
                info.SkillAction.Z = curInfo.Z;
                info.SkillAction.Dir = curInfo.Dir;
                this.NetworkAttack(info.SkillAction);
            }

            //初始化Buff状态
            if (info.BuffInfoCount > 0)
            {
                Log.Net("BuffInit: "+info);
                foreach (var buff in info.BuffInfoList)
                {
                    var pos = NetworkUtil.FloatPos(buff.X, buff.Y, buff.Z);
                    GameInterface_Skill.AddSkillBuff(attr.gameObject, buff.SkillId, pos, buff.Attacker);
                }
            }
        }

        //复活重置FrameID
        public override void Revive(GCPlayerCmd cmd)
        {
            lastFrameId = -1;
            StartCoroutine(attribute.NetworkRevive(cmd)); 
        }

        public override void NetworkAttack(SkillAction sk)
        {
            var cmd = new ObjectCommand(ObjectCommand.ENUM_OBJECT_COMMAND.OC_USE_SKILL);
            cmd.skillId = sk.SkillId;
            cmd.skillLevel = sk.SkillLevel;
            cmd.staticShoot = sk.IsStaticShoot;
            cmd.targetPos = NetworkUtil.FloatPos(sk.X, sk.Y, sk.Z);
            cmd.dir = sk.Dir;
            cmd.skillAction = sk;

            Log.GUI("Other Player Attack LogicCommand");
            gameObject.GetComponent<LogicCommand>().PushCommand(cmd);
        }

        public override void SetLevel(AvatarInfo info)
        {
            GetComponent<NpcAttribute>().ChangeLevel(info.Level);
        }

        public override void SetPositionAndDir(AvatarInfo info)
        {
            Vector3 vxz = new Vector3(info.X / 100.0f, info.Y / 100.0f + 0.2f, info.Z / 100.0f);
            Log.Sys("SetPosition: " + info + " vxz " + vxz + " n " + gameObject.name);
            transform.position = new Vector3(vxz.x, vxz.y, vxz.y);
            transform.rotation = Quaternion.Euler(new Vector3(0, info.Dir, 0));
            StartCoroutine(SetPos(vxz));
        }


        /// <summary>
        /// 本地控制对象接受网络命令
        /// 本地代理接受网络命令
        /// </summary>
        /// <param name="cmd">Cmd.</param>
        public override void DoNetworkDamage(GCPlayerCmd cmd)
        {
            var eid = cmd.DamageInfo.Enemy;
            var attacker = ObjectManager.objectManager.GetPlayer(cmd.DamageInfo.Attacker);
            if (attacker != null)
            {
                gameObject.GetComponent<MyAnimationEvent>().OnHit(attacker, cmd.DamageInfo.Damage, cmd.DamageInfo.IsCritical, cmd.DamageInfo.IsStaticShoot);
            }
        }

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

        public override void NetworkRemoveBuff(GCPlayerCmd cmd)
        {
            if (cmd.BuffInfo.HasBuffType)
            {
                gameObject.GetComponent<BuffComponent>().RemoveBuff((Affix.EffectType) cmd.BuffInfo.BuffType);
            }
            else
            {
                gameObject.GetComponent<BuffComponent>().RemoveBuffId(cmd.BuffInfo.BuffId);
            }
        }


        /// <summary>
        /// 稳定一下初始化位置 
        /// </summary>
        /// <returns>The position.</returns>
        /// <param name="p">P.</param>
        IEnumerator SetPos(Vector3 p)
        {
            var c = 0;
            while (c <= 3)
            {
                transform.position = p;
                c++;
                yield return null;
            }
        }

    
        void Awake()
        {
            curInfo = AvatarInfo.CreateBuilder().Build();
            StartCoroutine(SyncPos());
        }

        void Start()
        {
            attribute = GetComponent<NpcAttribute>();
            tower = gameObject.transform.Find("tower").GetComponent<TowerAutoCheck>();
        }

        IEnumerator SyncPos()
        {
            yield return new WaitForSeconds(2);
            var wt = new WaitForSeconds(0.1f);
            while (true)
            {
                SendMoveCmd();
                yield return wt;
            }
        }


        void SendMoveCmd()
        {
            if (!curInfo.HasX)
            {
                return;
            }
            //防止模型滑动
            this.GetComponent<Rigidbody>().velocity = Vector3.zero;
            //var curPos = NetworkUtil.ConvertPos(transform.position);
            var dir = (int)transform.localRotation.eulerAngles.y;
            var meAttr = gameObject.GetComponent<NpcAttribute>();
            var serverPos = NetworkUtil.FloatPos(curInfo.X, curInfo.Y, curInfo.Z);
            var difPos = Util.XZSqrMagnitude(transform.position, serverPos);
            //if (curInfo.X != curPos [0] || curInfo.Y != curPos [1] || curInfo.Z != curPos [2] || curInfo.Dir != dir)
            if (difPos > 64)
            {
                Util.ForceResetPos(this.GetComponent<Rigidbody>(), serverPos);

                var mvTarget = serverPos;
                var cmd = new ObjectCommand();
                cmd.targetPos = mvTarget;
                cmd.dir = curInfo.Dir;
                cmd.commandID = ObjectCommand.ENUM_OBJECT_COMMAND.OC_MOVE;
                GetComponent<LogicCommand>().PushCommand(cmd);

            }
            else if (difPos > 2 || curInfo.Dir != dir)
            {
                var mvTarget = serverPos;
                var cmd = new ObjectCommand();
                cmd.targetPos = mvTarget;
                cmd.dir = curInfo.Dir;
                cmd.commandID = ObjectCommand.ENUM_OBJECT_COMMAND.OC_MOVE;
                GetComponent<LogicCommand>().PushCommand(cmd);
            }

            if (curInfo.HasJumpForwardSpeed)
            {
                var intJumpSpeed = (int)(meAttr.JumpForwardSpeed * 100);
                if (intJumpSpeed != curInfo.JumpForwardSpeed)
                {
                    meAttr.JumpForwardSpeed = curInfo.JumpForwardSpeed / 100.0f;
                }
            }

            if (curInfo.HasTowerDir)
            {
                if (tower != null)
                {
                    tower.SyncTowerDir(curInfo.TowerDir);
                }
            }

        }


    }
    */
}