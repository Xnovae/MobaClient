using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class TankIdle : IdleState
    {
        bool first = true;
        //private TimerNode staticShootTimer;
        public override void EnterState()
        {
            base.EnterState();
            aiCharacter.SetIdle();

            if (GetAttr().IsMine())
            {
                if (first)
                {
                    first = false;
                }
                //GameInterface_Skill.AddStaticShootBuff(GetAttr().gameObject, (int)SkillData.SkillConstId.StaticShoot, Vector3.zero);
            }
        }

      


        //先进入MOVE_SHOOT 层状态机，再将状态注入
        public override bool CheckNextState(AIStateEnum next)
        {
            if (next == AIStateEnum.COMBAT || next == AIStateEnum.CAST_SKILL)
            {
                //进入MoveShoot状态机 再压入Combat命令
                GetAttr().StartCoroutine(MoveShoot());
                return false;
            }
            return base.CheckNextState(next);
        }
        public override bool CanChangeState(AIStateEnum next)
        {
            if (next == AIStateEnum.COMBAT || next == AIStateEnum.CAST_SKILL)
            {
                return true;
            }
            return base.CanChangeState(next);
        }

        IEnumerator MoveShoot()
        {
            Log.AI("MoveShoot: "+lastMsg.type);
            var tempMsg = lastMsg;
            yield return null;
            aiCharacter.ChangeState(AIStateEnum.MOVE_SHOOT);
            lastMsg = tempMsg;
            aiCharacter.ChangeState(AIStateEnum.COMBAT);
        }

        public override IEnumerator RunLogic()
        {
            var playerMove = GetAttr().GetComponent<MoveController>();
            var vcontroller = playerMove.vcontroller;

            while (!quit)
            {
                if (CheckEvent())
                {
                    yield break;
                }

                float v = 0;
                float h = 0;
                if (vcontroller != null)
                {
                    h = vcontroller.inputVector.x;//CameraRight 
                    v = vcontroller.inputVector.y;//CameraForward
                }

                bool isMoving = Mathf.Abs(h) > 0.1 || Mathf.Abs(v) > 0.1;
                if (isMoving)
                {
                    aiCharacter.ChangeState(AIStateEnum.MOVE_SHOOT);
                }
                yield return null;
            }
        }
    }
}