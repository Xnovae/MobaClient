using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyLib
{
    public class MobaMePlayerMove : MoveState 
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
                //var curInfo = networkMove.GetServerVelocity();
                var isLocalMove = MobaUtil.IsLocalMove(vcontroller);
                var isNetMove = MobaUtil.IsServerMoveBySpeedOrPos(networkMove);
                if(isLocalMove || isNetMove)
                {
                    if (isNetMove)//依赖服务器寻路
                    {
                        //MoveByNet();
                        //多个服务器发过来的帧率 有可能导致预测失败 现在的位置超过了预测位置导致玩家不断回头
                        //yield return GetAttr().StartCoroutine(MoveSmooth());
                        MoveByNet();
                        yield return new WaitForFixedUpdate();
                    }
                    else if (isLocalMove) //依赖本地的寻路
                    {
                        MoveByHand();
                        yield return new WaitForFixedUpdate();
                    }
                }else
                {
                    break;
                }
            }
            if (!quit)
            {
                aiCharacter.ChangeState(AIStateEnum.IDLE);
            }
        }

        /*
        private IEnumerator MoveSmooth()
        {
            var netPos = networkMove.GetServerPos();
            var curPos = GetAttr().transform.position;
            Log.AI("MoveSmooth:"+netPos+":"+curPos);
            var tarDir = netPos - curPos;
            tarDir.y = 0;

            var speedCoff = MobaUtil.GetSpeedCoff(curPos, netPos);
            var oriSpeed = aiCharacter.GetAttr().GetSpeed() * speedCoff;
            var mdir = tarDir.normalized;
            physics.TurnTo(mdir);
            var sp = mdir * oriSpeed;

            var passTime = 0.0f;
            var totalTime = tarDir.magnitude / oriSpeed;
            //不要玩家预测移动之后又回头 最终停下来的时候
            if (totalTime > 0.1f)
            {
                while (!quit && passTime < totalTime)
                {
                    passTime += Time.deltaTime;
                    var newPos = Vector3.Lerp(curPos, netPos, Mathf.Clamp01(passTime / totalTime));
                    physics.MoveToIgnorePhysic(newPos);
                    yield return null;
                }
            }else
            {
                yield return null;
            }
            //var newPos = GetAttr().transform.position + sp * Time.deltaTime;
        }
        */

        /// <summary>
        /// 自己玩家正常速度移动
        /// </summary>
        private void MoveByNet()
        {
            var netPos = networkMove.GetServerPos();
            var curPos = GetAttr().transform.position;
            var tarDir = netPos - curPos;
            tarDir.y = 0;
            var deltaDist = tarDir.magnitude;


            var speedCoff = MobaUtil.GetSpeedCoff(curPos, netPos);
            var oriSpeed = aiCharacter.GetAttr().GetSpeed() * speedCoff;

            /*
            var sp = tarDir / Util.FrameSecTime;
            var mag = sp.magnitude;
            if(mag > oriSpeed*2)
            {
                mag = oriSpeed*2;
            }
            */

            Log.AI("MoveMe:"+curPos+":"+netPos+":"+networkMove.GetServerVelocity()+":"+networkMove.GetCurInfoPos()+":"+networkMove.GetCurInfoSpeed());
            /*
            var dist = tarDir.magnitude;
            var serverVel = networkMove.GetServerVelocity();
            if (!MobaUtil.IsServerMove(serverVel) && dist < 1)
            {
                return;
            }
            */

            

            var mdir = tarDir.normalized;
            var netDir = MobaUtil.GetNetTurnTo(vcontroller.inputVector, mdir, deltaDist, physics.transform.forward);
            physics.TurnTo(netDir);

            if (Mathf.Abs(deltaDist) < 0.1f)
            {

            }
            else
            {
                //sp = mdir * mag;
                //var speed2X = speed * 2;
                var sp = mdir * oriSpeed;
                var newPos = GetAttr().transform.position + sp * Time.fixedDeltaTime;
                physics.MoveToIgnorePhysic(newPos);
            }
            //physics.MoveToIgnorePhysic(newPos);
        }

        private void MoveByHand()
        {
            float v = 0;
            float h = 0;
            h = vcontroller.inputVector.x; //CameraRight 
            v = vcontroller.inputVector.y; //CameraForward
            var targetDirection = h * camRight + v * camForward;
            var mdir = targetDirection.normalized;

            var localPos = physics.transform.position;
            var netPos = networkMove.GetServerPos();
            var speedCoff = MobaUtil.GetSpeedDecCoff(localPos, netPos);

            physics.TurnTo(mdir);
            var speed = aiCharacter.GetAttr().GetSpeed() * speedCoff;
            var sp = mdir* speed;
            var newPos = GetAttr().transform.position + sp * Time.fixedDeltaTime;
            physics.MoveToWithPhysic( newPos);
        }
    }
}