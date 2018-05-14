using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;

namespace MyLib
{
    /// <summary>
    /// 玩家自己的属性 从服务器上同步 
    /// </summary>
    /*
    public class MySelfAttributeSync : ISyncInterface 
    {
        private AvatarInfo curInfo;
        private NpcAttribute attr;
        private LogicCommand logicCommand;//操作MoveController 类似于服务器控制位置移动命令而不是客户端控制移动
        private AICharacter ai;

        public override void InitSync(AvatarInfo info)
        {
            if (info != null)
            {
                if (info.HasTeamColor)
                {
                    GetComponent<NpcAttribute>().SetTeamColorNet(info.TeamColor);
                }
            }
        }

        public override void NetworkAttribute(GCPlayerCmd cmd) {
            var info = cmd.AvatarInfo;
            var attr = GetComponent<NpcAttribute>();
            Log.Net("MySelfSync: "+info);
            if (info == null)
            {
                Debug.LogError("NetMatchInfo is Null ");
                return;
            }

            if (info.HasX)
            {
                var lag = NetworkLatency.LatencyTime;
                var predictLag = Mathf.Max(lag*2, 0.1f);

                curInfo.SpeedX = info.SpeedX;
                curInfo.SpeedY = info.SpeedY;
                curInfo.X = (int)(info.X+info.SpeedX*predictLag);
                curInfo.Y = info.Y;
                curInfo.Z = (int)(info.Z+info.SpeedY*predictLag);
                curInfo.Dir = info.Dir;
            }

            if(info.HasTeamColor) {
                attr.SetTeamColorNet(info.TeamColor);
            }
            if(info.HasIsMaster) {
                attr.SetIsMasterNet(info.IsMaster);
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

            if (dataChanged)
            {
                var sid = attr.GetNetView().GetServerID();
                ScoreManager.Instance.NetSyncScore(sid, curInfo.Score, sid, skillCount, deadCount, secondaryAttackCount);
            }

            if (info.HasContinueKilled)
            {
                curInfo.ContinueKilled = info.ContinueKilled;
                if (curInfo.ContinueKilled > 0)
                {
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

            Log.GUI("My Player Attack LogicCommand");
            gameObject.GetComponent<LogicCommand>().PushCommand(cmd);
            
        }

        public override void NetworkBuff(GCPlayerCmd cmd)
        {
            var sk = Util.GetSkillData(cmd.BuffInfo.SkillId, 1);
            var skConfig = SkillLogic.GetSkillInfo(sk);
            var evt = skConfig.GetEvent(cmd.BuffInfo.EventId);
            var binfo = cmd.BuffInfo;
            if (evt != null)
            {
                var pos = cmd.BuffInfo.AttackerPosList;
                var px = pos[0]/100.0f;
                var py = pos[1]/100.0f;
                var pz = pos[2]/100.0f;
                gameObject.GetComponent<BuffComponent>()
                    .AddBuff(evt.affix, new Vector3(px, py, pz), 0, cmd.BuffInfo.BuffId);
            }
        }

        public override void NetworkRemoveBuff(GCPlayerCmd cmd)
        {         
            var binfo = cmd.BuffInfo;
            if (binfo.HasBuffType)
            {
                gameObject.GetComponent<BuffComponent>().RemoveBuff((Affix.EffectType) binfo.BuffType);
            }
            else
            {
                gameObject.GetComponent<BuffComponent>().RemoveBuffId( binfo.BuffId);
            }

        }

        //TODO: 自己貌似没有走这里
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
        public override void DoNetworkDamage(GCPlayerCmd cmd)
        {
            throw new NotImplementedException();
        }

        void Start()
        {
            logicCommand = GetComponent<LogicCommand>();
            curInfo = AvatarInfo.CreateBuilder().Build();
            attr = GetComponent<NpcAttribute>();
            ai = GetComponent<AIBase>().GetAI();
        }

    }
    */
}