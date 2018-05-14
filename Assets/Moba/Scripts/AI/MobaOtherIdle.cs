using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MyLib
{
    public class MobaOtherIdle : IdleState
    {
        public override void EnterState()
        {
            base.EnterState();
            aiCharacter.SetIdle();
        }
        public override IEnumerator RunLogic()
        {
            var playerMove = GetAttr().GetComponent<MoveController>();
            var vcontroller = playerMove.vcontroller;
            var curInfo = GetAttr().GetComponent<ISyncInterface>();
            while (!quit)
            {
                var isNetMove = MobaUtil.IsServerMove(curInfo.GetServerVelocity());
                //只有网络移动
                if (isNetMove)
                {
                    aiCharacter.ChangeState(AIStateEnum.MOVE);
                }
                yield return null;
            }
        }
    }
}
