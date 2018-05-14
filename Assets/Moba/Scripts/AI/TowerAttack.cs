using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyLib
{
    public class TowerAttack : SkillState
    {
        private ObjectCommand cmd;
        private SkillFullInfo activeSkill;
        private SkillStateMachine skillStateMachine;
        private float holdTime;

        public override void EnterState()
        {
            base.EnterState();
            cmd = aiCharacter.lastCmd;
            activeSkill = GetAttr().GetComponent<SkillInfoComponent>().GetActiveSkill();
            var target = cmd.skillAction.Target;
            var targetPlayer = ObjectManager.objectManager.GetPlayer(target);

            skillStateMachine = SkillLogic.CreateSkillStateMachine(GetAttr().gameObject, activeSkill.skillData, GetAttr().transform.position, targetPlayer );
            Log.AI("TowerAttack:"+skillStateMachine);

            var time = Util.FrameToFloat(aiCharacter.lastCmd.skillAction.RunFrame);
            var dir = cmd.skillAction.Dir;
            holdTime = time;
        }

        /// <summary>
        /// 创建子弹服务器通知客户端 Trigger了子弹创建
        /// 播放动作帧上面的特效和事件
        /// </summary>
        /// <returns></returns>
        public override IEnumerator RunLogic()
        {
            var config = aiCharacter.GetAttr().npcConfig;
            var atype = config.GetAction(ActionType.Attack);
            var tempRunNum = runNum;
            yield return new WaitForSeconds(atype.hitTime);
            if (!quit && tempRunNum == runNum)
            {
                skillStateMachine.OnEvent(new MyEvent() { skillEvtType= SkillDataConfig.SkillEvent.EventTrigger });
            }

            yield return new WaitForSeconds(atype.totalTime - atype.hitTime);
            if (!quit && tempRunNum == runNum)
            {
                aiCharacter.ChangeState(AIStateEnum.IDLE);
            }
        }
    }
}