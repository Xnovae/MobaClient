using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyLib;
public class MeleeMove : MoveState
{
    private MonsterSync networkMove;
    private IPhysicCom physics;
    public override void EnterState()
    {
        base.EnterState();
        aiCharacter.SetRun();
    }
    /// <summary>
    /// 实际移动的逻辑
    /// </summary>
    /// <returns></returns>
    public override IEnumerator RunLogic()
    {
        var playerMove = GetAttr().GetComponent<MoveController>();
        physics = playerMove.GetComponent<IPhysicCom>();
        networkMove = GetAttr().GetComponent<MonsterSync>();
        while (!quit)
        {
            //var netPos = networkMove.GetServerVelocity();
            var isNetMove = MobaUtil.IsServerMoveBySpeedOrPos(networkMove);
            if (isNetMove)
            {
                //MoveByNet();
                yield return GetAttr().StartCoroutine(MoveSmooth());
            }
            else
            {
                aiCharacter.ChangeState(AIStateEnum.IDLE);
                break;
            }
            //yield return null;
            /*
            physics.TurnTo(playerMove.directionTo);

            var runTime = playerMove.runTime;
            var passTime = 0.0f;
            var curPos = playerMove.curPos;
            var tarPos = playerMove.targetPos;
            var curLogicFrameId = playerMove.logicFrameId;

            while (passTime < runTime && !quit)
            {
                var newPos = Vector3.Lerp(curPos, tarPos, Mathf.Clamp01(passTime / runTime));
                physics.MoveTo(newPos);
                passTime += Time.deltaTime;
                yield return null;
            }
            if (!quit)
            {
                //继续执行移动命令
                //否则结束进入Idle状态
                if (curLogicFrameId != playerMove.logicFrameId)
                {
                    aiCharacter.SetRun();
                }
                else
                {
                    aiCharacter.ChangeState(AIStateEnum.IDLE);
                }
            }
            */

        }
    }


    private IEnumerator MoveSmooth()
    {
        var netPos = networkMove.GetServerPos();
        var curPos = GetAttr().transform.position;
        //Log.AI("MoveSmooth:" + netPos + ":" + curPos);
        var tarDir = netPos - curPos;
        tarDir.y = 0;

        var speedCoff = MobaUtil.GetSpeedCoff(curPos, netPos);
        var oriSpeed = aiCharacter.GetAttr().GetSpeed() * speedCoff;
        var mdir = tarDir.normalized;
        physics.TurnTo(mdir);
        var sp = mdir * oriSpeed;

        var passTime = 0.0f;
        var totalTime = tarDir.magnitude / oriSpeed;

        while (!quit && passTime < totalTime)
        {
            passTime += Time.deltaTime;
            var newPos = Vector3.Lerp(curPos, netPos, Mathf.Clamp01(passTime / totalTime));
            physics.MoveToIgnorePhysic(newPos);
            yield return null;
        }
        //var newPos = GetAttr().transform.position + sp * Time.deltaTime;
    }

    /// <summary>
    /// 其它NPC速度加快的移动
    /// </summary>
    private void MoveByNet()
    {
        //var curInfo = networkMove.curInfo;
        //var netPos = MobaUtil.FloatPos(curInfo);
        var netPos = networkMove.GetServerPos();
        var curPos = GetAttr().transform.position;
        var tarDir = netPos - curPos;
        tarDir.y = 0;

        var oriSpeed = aiCharacter.GetAttr().GetSpeed();
        var sp = tarDir / Util.FrameSecTime;
        var mag = sp.magnitude;
        if (mag > oriSpeed * 2)
        {
            mag = oriSpeed * 2;
        }

        var mdir = tarDir.normalized;
        physics.TurnTo(mdir);

        //var speed = aiCharacter.GetAttr().GetSpeed();

        sp = mdir * mag;
        //var sp = mdir * oriSpeed;
        var newPos = GetAttr().transform.position + sp * Time.deltaTime;
        physics.MoveToIgnorePhysic(newPos);
    }

}
