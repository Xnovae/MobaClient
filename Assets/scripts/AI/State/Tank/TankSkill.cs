using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class TankSkill : SkillState
    {
        private SkillStateMachine skillStateMachine;
        private SkillFullInfo activeSkill;

        private float rotateSpeed = 10;
        private float walkSpeed = 3.0f;

        public override void EnterState()
        {
            base.EnterState();
            Log.AI("Enter Skill State ");
            //启动技能状态机 启动动画
            activeSkill = GetAttr().GetComponent<SkillInfoComponent>().GetActiveSkill();
            skillStateMachine = SkillLogic.CreateSkillStateMachine(GetAttr().gameObject, activeSkill.skillData,
                GetAttr().transform.position);
        }

        public override void ExitState()
        {
            base.ExitState();
        }

        //向当前所面朝方向进行攻击
        public override IEnumerator RunLogic()
        {
            Log.AI("Check Animation " + GetAttr().GetComponent<Animation>().IsPlaying(activeSkill.skillData.AnimationName));
            float passTime = 0;
            var realAttackTime = activeSkill.skillData.AttackAniTime/GetAttr().GetSpeedCoff();
            //var animation = GetAttr().animation;
            //var attackAniName = activeSkill.skillData.AnimationName;
            //var rate = GetAttr().animation[attackAniName].length/realAttackTime;
            Log.AI("AttackAniSpeed " + " realAttackTime " + realAttackTime);
            //PlayAni(activeSkill.skillData.AnimationName, rate, WrapMode.Once);
            var isEvt = false;
            while (!quit)
            {
                if (CheckEvent())
                {
                    isEvt = true;
                    break;
                }
                if (passTime >= realAttackTime*0.8f)
                {
                    break;
                }
                passTime += Time.deltaTime;
                yield return null;
            }
            Log.AI("Stop SkillState ");
            skillStateMachine.Stop();
            if (!isEvt)
            {
                aiCharacter.ChangeState(AIStateEnum.IDLE);
            }
        }

    }
}