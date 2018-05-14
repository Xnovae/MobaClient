using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class TankKnockBack : KnockBackState
    {
        float KnockBackTime = 0.6f;
        float KnockBackSpeed = 6;
        float StopKnockTime = 0.4f;

        private bool IsInv = false;
        public override void EnterState()
        {
            IsInv = GetEvent().KnockInv;
            KnockBackSpeed = GameConst.Instance.KnockBackSpeed; 
            base.EnterState();
            aiCharacter.SetIdle();
        }

        public override bool CheckNextState(AIStateEnum next)
        {
            if (next == AIStateEnum.KNOCK_BACK)
            {
                return false;
            }
            return base.CheckNextState(next);
        }

        public override void ExitState()
        {
            var physic = GetAttr().GetComponent<TankPhysicComponent>();
            physic.MoveSpeed(Vector3.zero);
            base.ExitState();
        }

        public override IEnumerator RunLogic()
        {
            var physic = GetAttr().GetComponent<TankPhysicComponent>();
            Vector3 moveDirection = GetAttr().transform.position - GetEvent().KnockWhoPos;
            moveDirection.y = 0;
            if (IsInv)
            {
                moveDirection = -1*moveDirection;
            }

            moveDirection.Normalize();

            float curFlySpeed = KnockBackSpeed;
            float passTime = 0;
            while (passTime < KnockBackTime)
            {
                var movement = moveDirection * curFlySpeed;
                physic.MoveSpeed(movement);
                yield return new WaitForFixedUpdate();
                passTime += Time.fixedDeltaTime;

                if (CheckEvent())
                {
                    yield break;
                }
            }
            
            float stopTime = 0;
            while (stopTime < StopKnockTime)
            {
                curFlySpeed = Mathf.Lerp(curFlySpeed, 0, 5 * Time.deltaTime);
                var movement = moveDirection * curFlySpeed;
                physic.MoveSpeed(movement);

                yield return new WaitForFixedUpdate();
                stopTime += Time.fixedDeltaTime;
                if (CheckEvent())
                {
                    yield break;
                }
            }

            aiCharacter.ChangeState(AIStateEnum.IDLE);
        }
    }

}