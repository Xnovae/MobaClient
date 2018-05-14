
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
using System.Collections.Generic;
using System;
using SimpleJSON;

/// <summary>
/// 初始化技能列表 CActionItem_Skill 管理这些技能
/// </summary>
namespace MyLib
{
    public class SkillDataController : MonoBehaviour
    {
        public static SkillDataController skillDataController;

        //右下角快捷栏 里面的 技能  还包括 使用药品的技能
        //TODO: 初始化结束之后 玩家 SkillinfoComponent 从这里获取快捷栏里面的技能  不包括普通攻击技能  普通技能的ID 根据单位的BaseSkill 确定
        //防御和闪避 也是固定的
        List<SkillFullInfo> skillSlots = new List<SkillFullInfo>();
        int totalSkillPoint;
        public int TotalSp
        {
            get
            {
                return totalSkillPoint;
            }
            set
            {
                totalSkillPoint = value;
            }
        }

        public int DistriSp
        {
            get
            {
                return 0;
            }
        }

        List<SkillFullInfo> activeSkill = new List<SkillFullInfo>();
        List<SkillFullInfo> passiveSkill = new List<SkillFullInfo>();

        public List<SkillFullInfo> activeSkillData
        {
            get
            {
                return activeSkill;
            }
        }

        void Update()
        {
            foreach (SkillFullInfo s in skillSlots)
            {
                s.Update();
            }
        }

        public bool CheckCoolDown(int index)
        {
            foreach (SkillFullInfo s in skillSlots)
            {
                if (s.shortSlotId == index)
                {
                    return s.CheckCoolDown();
                }
            }
            return false;
        }
        public void SetCoolDown(int index) {
            foreach (SkillFullInfo s in skillSlots)
            {
                if (s.shortSlotId == index)
                {
                    s.SetCoolDown();
                    break;
                }
            }
        }

        public float GetCoolTime(int index) {
            foreach (SkillFullInfo s in skillSlots)
            {
                if (s.shortSlotId == index)
                {
                    return s.CoolDownTime;
                }
            }
            return 0;
        }

        public List<SkillFullInfo> passive
        {
            get
            {
                return passiveSkill;
            }
        }

	
        /// <summary>
        /// 返回实际技能的等级  快捷技能里面只有技能ID 
        /// </summary>
        /// <returns>The short skill data.</returns>
        /// <param name="index">Index.</param>
        public SkillData GetShortSkillData(int index)
        {
            //Log.Sys("GetShortSkillData "+index);
            foreach (SkillFullInfo s in skillSlots)
            {
                if (s.shortSlotId == index)
                {
                    //Log.Sys("SkillLevel: "+s.skillData.Level);
                    foreach (var sk in activeSkill)
                    {
                        if (sk.skillId == s.skillId)
                        {
                            return sk.skillData;
                        }
                    }

                    return s.skillData;
                    //return null
                }
            }
            return null;
        }

      

        public void InitSkillShotAfterSelectSkill( int PlayerModelInGame)
        {
            var npcConfig = NpcDataManager.Instance.GetConfig(PlayerModelInGame);
            var sk1 = npcConfig.GetAction(ActionType.Skill1);
            skillSlots = new List<SkillFullInfo>();
            var full = new SkillFullInfo(sk1.skillId, 0);
            skillSlots.Add(full);
            MyEventSystem.myEventSystem.PushEvent(MyEvent.EventType.UpdateShortCut);
        }

      

        public void UpdateShortcutsInfo(IList<ShortCutInfo> shortCutInfo)
        {
            skillSlots = new List<SkillFullInfo>();
            foreach (ShortCutInfo s in shortCutInfo)
            {
                var full = new SkillFullInfo(s);
                skillSlots.Add(full);
            }
            MyEventSystem.myEventSystem.PushEvent(MyEvent.EventType.UpdateShortCut);
        }


        void Awake()
        {
            skillDataController = this;
            DontDestroyOnLoad(gameObject);
        }

        /*
		 * 需要道具升级技能
		 * TODO:Push SP点数更新
		 */
        public void SkillLevelUpWithSp(int skillId)
        {
            /*
            var levelUp = CGSkillLevelUp.CreateBuilder();
            levelUp.SkillId = skillId;
            KBEngine.Bundle.sendImmediate(levelUp);
            */
        }


        public void ActivateSkill(GCPushActivateSkill skill)
        {
            foreach (var a in activeSkill)
            {
                if (a.skillId == skill.SkillId)
                {
                    a.SetLevel(skill.Level);
                    break;
                }
            }
            MyEventSystem.PushEventStatic(MyEvent.EventType.UpdateSkill);
        }

    }

}