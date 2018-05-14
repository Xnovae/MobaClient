using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyLib;

/// <summary>
/// 定时播放一个技能动作
/// 特效和子弹 实例 自己的配置和执行逻辑
/// </summary>
public class MeleeAttack : SkillState 
{
    SkillStateMachine skillStateMachine;
    SkillFullInfo activeSkill;
    private ObjectCommand cmd;
    private float holdTime;
    public override void EnterState()
    {
        base.EnterState();
        cmd = aiCharacter.lastCmd;
        activeSkill = GetAttr().GetComponent<SkillInfoComponent>().GetActiveSkill();
        var target = cmd.skillAction.Target;
        var targetPlayer = ObjectManager.objectManager.GetPlayer(target);
        skillStateMachine = SkillLogic.CreateSkillStateMachine(GetAttr().gameObject, activeSkill.skillData, GetAttr().transform.position, targetPlayer);
        var time = Util.FrameToFloat(aiCharacter.lastCmd.skillAction.RunFrame);
        var dir =  cmd.skillAction.Dir;
        var physics = aiCharacter.GetAttr().GetComponent<IPhysicCom>();
        physics.TurnToDir(dir);
        holdTime = time;

        var config = aiCharacter.GetAttr().npcConfig;
        var atype = config.GetAction(ActionType.Attack);
        aiCharacter.PlayAniInTime(atype.aniName, time);

    }
    public override IEnumerator RunLogic()
    {
        var config = aiCharacter.GetAttr().npcConfig;
        var atype = config.GetAction(ActionType.Attack);
        var tempRunNum = runNum;
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
    }
}
