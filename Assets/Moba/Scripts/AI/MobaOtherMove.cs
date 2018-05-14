using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyLib
{
    public class MobaOtherMove : MoveState
    {
        VirtualController vcontroller;
        Vector3 camRight;
        Vector3 camForward;
        IPhysicCom physics;
        ISyncInterface networkMove;

        public override void Init()
        {
            base.Init();
            var playerMove = GetAttr().GetComponent<MoveController>();
            vcontroller = playerMove.vcontroller;
            physics = GetAttr().GetComponent<IPhysicCom>();
            camRight = CameraController.Instance.camRight;
            camForward = CameraController.Instance.camForward;
        }

        public override void EnterState()
        {
            base.EnterState();
            aiCharacter.SetRun();
        }

        /// <summary>
        /// MoveController 摇杆输入
        /// MobaMeSync 网络输入
        /// 两个综合考虑
        /// </summary>
        /// <returns></returns>
        public override IEnumerator RunLogic()
        {
            networkMove = GetAttr().GetComponent<ISyncInterface>();
            var physics = GetAttr().GetComponent<CreepMeleeAI>();

            while (!quit)
            {
                var curInfo = networkMove.GetServerVelocity();
                var isNetMove = MobaUtil.IsServerMove(curInfo);
                if (isNetMove)
                {
                    if (isNetMove)//依赖服务器寻路
                    {
                        MoveByNet();
                    }
                    yield return null;
                }
                else
                {
                    break;
                }
            }
            if (!quit)
            {
                aiCharacter.ChangeState(AIStateEnum.IDLE);
            }
        }

        private void MoveByNet()
        {
            var netPos = networkMove.GetServerPos();
            var curPos = GetAttr().transform.position;
            var tarDir = netPos - curPos;
            tarDir.y = 0;
            var mdir = tarDir.normalized;
            physics.TurnTo(mdir);

            var speed = aiCharacter.GetAttr().GetSpeed();
            var sp = mdir * speed;
            var newPos = GetAttr().transform.position + sp * Time.deltaTime;
            physics.MoveToIgnorePhysic(newPos);
        }
      
    }
}