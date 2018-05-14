
/*
Author: liyonghelpme
Email: 233242872@qq.com
*/
using UnityEngine;
using System.Collections;

namespace MyLib
{
	public class GameInterface 
	{
		public static GameInterface gameInterface = new GameInterface();
	
        /// <summary>
        /// 和使用普通技能一样 
        /// </summary>
		public void PlayerAttack() {
            //连击3招
            var meId = ObjectManager.objectManager.GetMyAttr().GetNetView().GetLocalId();
            var skillInfo = ObjectManager.objectManager.GetMyPlayer().GetComponent<SkillInfoComponent>();
            skillInfo.SetDefaultActive();

            var skillData = skillInfo.GetActiveSkill().skillData;
            var curState = ObjectManager.objectManager.GetMyPlayer().GetComponent<AIBase>().GetAI().state;
            //无动作技能释放 不用检测
            if (string.IsNullOrEmpty(skillData.AnimationName))
            {
            } else
            {
                //当前状态不能使用技能 或者已经在技能状态了不能连续点击
                if (curState != null)
                {
                    var ret = curState.CanChangeState(AIStateEnum.CAST_SKILL);
                    if (!ret)
                    {
                        return;
                    }
                }
            }

            if (curState != null && curState.type == AIStateEnum.DEAD)
            {
                Util.ShowMsg("死亡不能使用技能");
                return;
            }
            //默认技能上次使用频率控制

            MyEventSystem.PushLocalEventStatic(meId, MyEvent.EventType.UseSkill);
            var skillId = skillInfo.GetActiveSkill().skillId;
            /*
            //如果身上有Buff则调整默认技能的ID
            if (!NetworkUtil.IsNet())
            {
                ObjectManager.objectManager.GetMyPlayer().GetComponent<MyAnimationEvent>().OnSkill(skillData);
            }

            if (skillInfo.GetActiveSkill().skillData.skillConfig.stopMove)
            {
                //短时间限制自己移动
                var me = ObjectManager.objectManager.GetMyPlayer();
                GameInterface_Skill.AddSkillBuff(me.gameObject, "StopMoveBuff", Vector3.zero);
            }

            NetDateInterface.FastMoveAndPos();
            */

            NetDateInterface.FastUseSkill(skillId, skillData.Level);

		}


		//将背包物品装备起来
		public void PacketItemUserEquip(BackpackData item) {
			//摆摊
			//验证使用规则
			//判断等级
			var myself = ObjectManager.objectManager.GetMyPlayer ();
			if (myself != null) {
				if( item.GetNeedLevel () != -1 && myself.GetComponent<NpcAttribute>().Level < item.GetNeedLevel()) {
					var evt = new MyEvent(MyEvent.EventType.DebugMessage);
					evt.strArg = "等级不够";
					MyEventSystem.myEventSystem.PushEvent(evt);
					return;
				}
			}

			BackPack.backpack.SetSlotItem (item);
			BackPack.backpack.StartCoroutine(BackPack.backpack.UseEquipForNetwork ());
		}

		

	}
}
