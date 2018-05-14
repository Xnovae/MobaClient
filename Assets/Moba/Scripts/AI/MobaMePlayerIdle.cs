using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MyLib
{
    public class MobaMePlayerIdle : IdleState
    {
        ISyncInterface sync;
        public override void Init()
        {
            base.Init();
            sync = aiCharacter.GetAttr().GetComponent<ISyncInterface>();
        }

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
                var isLocalMoving = MobaUtil.IsLocalMove(vcontroller);
                var isNetMove = MobaUtil.IsServerMoveBySpeedOrPos(curInfo);
                //增加假的移动帧 降低启动时间
                if (isLocalMoving)
                {
                    //sync.AddFakeMove();
                }
                if (isNetMove || isLocalMoving)
                {
                    aiCharacter.ChangeState(AIStateEnum.MOVE);
                }
                yield return null;
            }
        }
    }
}
