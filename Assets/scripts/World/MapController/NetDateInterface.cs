using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MyLib
{
    public static class NetDateInterface
    {
        /// <summary>
        ///相同的技能 Skill Configure来触发Buff 但是不要触发 Buff修改非表现属性
        /// </summary>
        /// <param name="affix">Affix.</param>
        /// <param name="attacker">Attacker.</param>
        /// <param name="target">Target.</param>
        public static void FastAddBuff(Affix affix, GameObject attacker, GameObject target, int skillId, int evtId)
        {
            var cg = CGPlayerCmd.CreateBuilder();
            var binfo = BuffInfo.CreateBuilder();
            binfo.BuffType = (int)affix.effectType;
            binfo.Attacker = attacker.GetComponent<KBEngine.KBNetworkView>().GetServerID();
            binfo.Target = target.GetComponent<KBEngine.KBNetworkView>().GetServerID();
            binfo.SkillId = skillId;
            binfo.EventId = evtId;
            var pos = attacker.transform.position;
            binfo.AddAttackerPos((int)(pos.x * 100));
            binfo.AddAttackerPos((int)(pos.y * 100));
            binfo.AddAttackerPos((int)(pos.z * 100));

            var pos1 = NetworkUtil.ConvertPos(target.transform.position);
            binfo.X = pos1[0];
            binfo.Y = pos1[1];
            binfo.Z = pos1[2];
            binfo.Dir = (int)target.transform.eulerAngles.y;

            var eft = BuffManager.buffManager.GetBuffInstance(affix.effectType);
            var buff = (IEffect)Activator.CreateInstance(eft);
            buff.attacker = attacker;
            buff.target = target;
            var param = buff.GetExtraParams();
            binfo.Params = param;

            cg.BuffInfo = binfo.Build();
            cg.Cmd = "Buff";
            var sc = WorldManager.worldManager.GetActive();
            sc.BroadcastMsg(cg);
        }

        /// <summary>
        /// 技能的朝向
        /// 攻击目标由服务器决定
        /// </summary>
        /// <param name="skillId"></param>
        /// <param name="skillLevel"></param>
        public static void FastUseSkill(int skillId, int skillLevel)
        {
            Log.Sys("FastUseSkill: "+skillId+" lev "+skillLevel);
            var sc = WorldManager.worldManager.GetActive();
            if (sc.IsNet && sc.CanFight)
            {   
                var me = ObjectManager.objectManager.GetMyPlayer();
                var pos = me.transform.position;
                var dir = (int)me.transform.localRotation.eulerAngles.y;
                
                /*
                var tower = me.GetComponent<TankPhysicComponent>().tower;
                if (tower == null)
                {
                    return;
                }
                var dir = (int)tower.transform.eulerAngles.y;
                */


                var cg = CGPlayerCmd.CreateBuilder();
                var skInfo = SkillAction.CreateBuilder();
                skInfo.Who = ObjectManager.objectManager.GetMyServerID(); 
                skInfo.SkillId = skillId;
                skInfo.SkillLevel = skillLevel;
                
                var ip = NetworkUtil.ConvertPos(pos);
                skInfo.X = ip[0];
                skInfo.Y = ip[1];
                skInfo.Z = ip[2];
                skInfo.Dir = dir;

                /*
                if (skillId == 1)
                {
                    var attribute = me.GetComponent<NpcAttribute>();
                    skInfo.IsStaticShoot = attribute.GetStaticShootBuff();
                }
                */

                var target = SkillLogic.FindNearestEnemy(me);
                var targetId = 0;
                if (target != null)
                {
                    targetId = target.GetComponent<NpcAttribute>().GetNetView().GetServerID();
                }
                skInfo.Target = targetId;

                cg.SkillAction = skInfo.Build();
                cg.Cmd = "Skill";
                //sc.BroadcastMsg(cg);
                sc.BroadcastKCP(cg);
            }
        }

        public static void FastDamage(int attackerId, int enemyId, int damage, bool isCritical,bool isStaticShoot)
        {
            var cg = CGPlayerCmd.CreateBuilder();
            var dinfo = DamageInfo.CreateBuilder();
            dinfo.Attacker = attackerId;
            dinfo.Enemy = enemyId;
            dinfo.Damage = damage;
            dinfo.IsCritical = isCritical;
            dinfo.IsStaticShoot = isStaticShoot;
      
            cg.DamageInfo = dinfo.Build();
            cg.Cmd = "Damage";
            WorldManager.worldManager.GetActive().BroadcastMsg(cg);
        }


        public static void Revive()
        {
            var me = ObjectManager.objectManager.GetMyPlayer();
            var sid = me.GetComponent<NpcAttribute>().GetNetView().GetServerID();

            var pos = NetworkUtil.ConvertPos(me.transform.position);
            var dir = (int) me.transform.localRotation.eulerAngles.y;

            var cg = CGPlayerCmd.CreateBuilder();
            cg.Cmd = "Revive";
            var ainfo = AvatarInfo.CreateBuilder();
            ainfo.Id = sid;
            ainfo.X = pos[0];
            ainfo.Y = pos[1];
            ainfo.Z = pos[2];
            ainfo.Dir = dir;
            cg.AvatarInfo = ainfo.Build();
            WorldManager.worldManager.GetActive().BroadcastMsg(cg);
        }

        public static void SyncTime(int leftTime)
        {
            var cg = CGPlayerCmd.CreateBuilder();
            cg.Cmd = "SyncTime";
            cg.LeftTime = leftTime;
            WorldManager.worldManager.GetActive().BroadcastMsg(cg);
        }

        public static void GameOver()
        {
            var cg = CGPlayerCmd.CreateBuilder();
            cg.Cmd = "GameOver";
            WorldManager.worldManager.GetActive().BroadcastMsg(cg);
        }

        public static void Dead(int last,List<int> attackerList)
        {
            var cg = CGPlayerCmd.CreateBuilder();
            cg.Cmd = "Dead";
            var dinfo = DamageInfo.CreateBuilder();
            dinfo.AddRangeAttackerList(attackerList);
            dinfo.Attacker = last;
            dinfo.Enemy = ObjectManager.objectManager.GetMyAttr().GetNetView().GetServerID();
            cg.DamageInfo = dinfo.Build();
            WorldManager.worldManager.GetActive().BroadcastMsg(cg);
        }

        public static void FastMoveAndPos()
        {
            /*
            var s = WorldManager.worldManager.GetActive();
            if (s.IsNet && s.CanFight)
            {
                var me = ObjectManager.objectManager.GetMyPlayer();
                if (me == null)
                {
                    return;
                }
                var pos = me.transform.position;
                var dir = (int)me.transform.localRotation.eulerAngles.y;

                var cg = CGPlayerCmd.CreateBuilder();
                cg.Cmd = "Move";
                var ainfo = AvatarInfo.CreateBuilder();
                ainfo.X = (int)(pos.x * 100);
                ainfo.Z = (int)(pos.z * 100);
                ainfo.Y = (int)(pos.y * 100);
                ainfo.Dir = dir;
                cg.AvatarInfo = ainfo.Build();

                s.BroadcastMsg(cg);
            }
             */ 
        }


        public static void SyncPosDirHP()
        {
            var me = ObjectManager.objectManager.GetMyPlayer();
            if (me == null)
            {
                return;
            }
            var pos = me.transform.position;
            var dir = (int)me.transform.localRotation.eulerAngles.y;
            var meAttr = me.GetComponent<NpcAttribute>();

            var cg = CGPlayerCmd.CreateBuilder();
            cg.Cmd = "UpdateData";
            var ainfo = AvatarInfo.CreateBuilder();
            ainfo.X = (int)(pos.x * 100);
            ainfo.Z = (int)(pos.z * 100);
            ainfo.Y = (int)(pos.y * 100);
            ainfo.Dir = dir;
            ainfo.HP = meAttr.HP;

            cg.AvatarInfo = ainfo.Build();

            var s = WorldManager.worldManager.GetActive();
            s.BroadcastMsg(cg);
        }

        public static void FastRemoveBuff(int effectType, GameObject target)
        {
            var cg = CGPlayerCmd.CreateBuilder();
            var binfo = BuffInfo.CreateBuilder();
            binfo.BuffType = effectType;
            binfo.Target = target.GetComponent<KBEngine.KBNetworkView>().GetServerID();
         
            cg.BuffInfo = binfo.Build();
            cg.Cmd = "RemoveBuff";
            var sc = WorldManager.worldManager.GetActive();
            sc.BroadcastMsg(cg);
        }


        public static void RemoveBuffId(int buffId, GameObject target)
        {
            var cg = CGPlayerCmd.CreateBuilder();
            var binfo = BuffInfo.CreateBuilder();
            binfo.BuffId = buffId;
            binfo.Target = target.GetComponent<KBEngine.KBNetworkView>().GetServerID();
         
            cg.BuffInfo = binfo.Build();
            cg.Cmd = "RemoveBuff";
            var sc = WorldManager.worldManager.GetActive();
            sc.BroadcastMsg(cg);
        }
    }
}
