using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyLib
{
    /// <summary>
    /// 动作类型
    /// </summary>
    public enum ActionType
    {
        None,
        Attack,
        Skill1, //技能1
    }

    [System.Serializable]
    public class ActionConfig
    {
        public ActionType type = ActionType.Attack;
        public float totalTime;
        public float hitTime;
        public string aniName = "AbilityR";
        public float skillAttackRange = 8;
        public float skillAttackTargetDist = 8;
        public int skillId;//技能伤害计算的ID
        public bool needEnemy = false; //锁定目标技能必须有目标才可以释放 
    }

    public class NpcConfig : MonoBehaviour
    {
        public bool IsPlayer = false;
        public int npcTemplateId;
        public List<ActionConfig> actionList;
        //普通攻击
        //4个技能的配置
        public string normalAttack = "monsterSingle";
        public float eyeSightDistance = 9.5f;
        public float attackRangeDist = 10;
        public int attackSkill = 1;
        public float moveSpeed = 5;
        public float damageToTower = 1.0f;
        public float maxMoveRange2 = 11;
        public float hpRecover = 0;

        public ActionConfig GetAction(ActionType tp)
        {
            foreach (var a in actionList)
            {
                if (a.type == tp)
                {
                    return a;
                }
            }
            return new ActionConfig() { type = ActionType.None };
        }

        public ActionConfig GetActionBySkillId(int skillId)
        {
            foreach (var a in actionList)
            {
                if (a.skillId == skillId)
                {
                    return a;
                }
            }
            return null;
        }


        public static readonly NpcConfig defaultConfig = new NpcConfig()
        {
            npcTemplateId = -1,
        };

    }


   
}
