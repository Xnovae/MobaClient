using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MyLib
{
    /// <summary>
    /// 客户端通知服务器释放技能
    /// 服务器释放了技能之后广播给客户端
    /// </summary>
    public class MobaMePlayerSkill : SkillState
    {
        SkillStateMachine skillStateMachine;
        SkillFullInfo activeSkill;
        private ObjectCommand cmd;
        private float holdTime;
        public override void EnterState()
        {
            base.EnterState();
            cmd = aiCharacter.lastCmd;
            aiCharacter.blackboard[AIParam.SkillCmd] = new AIEvent() { cmd = cmd.proto};

            var target = cmd.skillAction.Target;
            var targetPlayer = ObjectManager.objectManager.GetPlayer(target);
            activeSkill = GetAttr().GetComponent<SkillInfoComponent>().GetActiveSkill();

            skillStateMachine = SkillLogic.CreateSkillStateMachine(GetAttr().gameObject, activeSkill.skillData, GetAttr().transform.position, targetPlayer);
            skillStateMachine.cmd = cmd.proto;

            var time = Util.FrameToFloat(aiCharacter.lastCmd.skillAction.RunFrame);
            var dir = cmd.skillAction.Dir;
            var physics = aiCharacter.GetAttr().GetComponent<IPhysicCom>();
            physics.TurnToDir(dir);
            holdTime = time;

            var npcConfig = aiCharacter.attribute.npcConfig;
            var actConfig = npcConfig.GetActionBySkillId(cmd.proto.SkillAction.SkillId);
            if (actConfig == null) {
                Debug.LogError("NpcSkill:"+MobaUtil.PrintObj(npcConfig)+":"+cmd.proto);
            }
            var aniName = actConfig.aniName;
            aiCharacter.PlayAniInTime(aniName, time);


        }
        public override IEnumerator RunLogic()
        {
            var tempRunNum = runNum;
            var config = aiCharacter.attribute.npcConfig;
            var atype = config.GetActionBySkillId(cmd.proto.SkillAction.SkillId);
            yield return new WaitForSeconds(atype.hitTime);
            if (!quit && tempRunNum == runNum)
            {
                skillStateMachine.OnEvent(new MyEvent() { skillEvtType = SkillDataConfig.SkillEvent.EventTrigger });
            }
            yield return new WaitForSeconds(atype.totalTime - atype.hitTime);
            if (!quit && tempRunNum == runNum)
            {
                aiCharacter.ChangeState(AIStateEnum.IDLE);
            }
            /*
            var passTime = 0.0f;
            while (!quit && passTime < holdTime)
            {
                passTime += Time.deltaTime;
                yield return null;
            }
            if (!quit)
            {
                aiCharacter.ChangeState(AIStateEnum.IDLE);
            }
            */
        }
    }

}