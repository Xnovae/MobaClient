using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class TankMoveAndShoot : MoveShootState
    {
        float moveSpeed = 0;
        float walkSpeed = 8.0f;
        float rotateSpeed = 500;
        float speedSmoothing = 10;
        float WindowTime = 0.5f;

        private bool sendRemoveBuff = true;

        public override void EnterState()
        {
            base.EnterState();
            walkSpeed = GameConst.Instance.MoveSpeed;
            inSkill = false;
            sendRemoveBuff = true;
        }

        public override IEnumerator RunLogic()
        {
            GetAttr().StartCoroutine(Move());
            yield return null;
        }

        private IEnumerator Move()
        {
            var playerMove = GetAttr().GetComponent<MoveController>();
            var vcontroller = playerMove.vcontroller;
            var camRight = playerMove.camRight;
            var camForward = playerMove.camForward;
            var physics = playerMove.GetComponent<TankPhysicComponent>();
            var first = true;
            var logicCommand = playerMove.GetComponent<LogicCommand>();

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
                    h = vcontroller.inputVector.x; //CameraRight 
                    v = vcontroller.inputVector.y; //CameraForward
                }
                bool isMoving = Mathf.Abs(h) > 0.1 || Mathf.Abs(v) > 0.1;
                isMoving = Mathf.Abs(h) > 0.1 || Mathf.Abs(v) > 0.1;
                if (!isMoving)
                {
                    physics.MoveSpeed(Vector3.zero);
                    if (!inSkill && !first)
                    {
                        aiCharacter.ChangeState(AIStateEnum.IDLE);
                        break;
                    }
                    else
                    {
                        first = false;
                        yield return null;
                        continue;
                    }
                }

                Vector3 moveDirection = Vector3.zero;
                Vector3 targetDirection = h*camRight + v*camForward;
                if (targetDirection != Vector3.zero)
                {
                    moveDirection = targetDirection;
                }

                var curSmooth = speedSmoothing*Time.deltaTime;
                var targetSpeed = Mathf.Min(targetDirection.magnitude, 1.0f);
                targetSpeed *= walkSpeed;
                moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, curSmooth);
                GetAttr().MoveSpeed = moveSpeed;

                physics.TurnTo(moveDirection);
                var curDir = physics.transform.forward;
                var movement = moveDirection*moveSpeed;

                var cosDir = Vector3.Dot(moveDirection.normalized, curDir.normalized);
                cosDir = Mathf.Max(0, cosDir);
                movement = cosDir*movement;
                //movement = cosDir * movement;
                //当前不动 旋转 
                //在目标方向移动
                //在当前方向移动
                //在当前方向旋转

                if (GetAttr().IsMine() && sendRemoveBuff)
                {
                    if (GetAttr().MoveSpeed > 0.05f)
                    {
                        sendRemoveBuff = false;
                        GameInterface_Skill.RemoveStaticShootBuff(GetAttr().gameObject, 155, Vector3.zero);
                    }
                }

                physics.MoveSpeed(movement);
                //没有使用技能则自动设置方向 有技能则最近设置方向
                yield return null;
            }
        }

        public override void ExitState()
        {
            GetAttr().MoveSpeed = 0;
            var physics = GetAttr().GetComponent<TankPhysicComponent>();
            physics.MoveSpeed(Vector3.zero);
            base.ExitState();
        }


        public override bool CanChangeState(AIStateEnum next)
        {
            if (next == AIStateEnum.COMBAT || next == AIStateEnum.CAST_SKILL)
            {
                if(!inSkill) {
                    return true;
                }else {
                    if (GetAttr().IsMine())
                    {
                        //Util.ShowMsg("正在攻击不能使用技能");
                    }
                    return false;
                }
            }
            return base.CanChangeState(next);
        }
        //检测输入shoot命令
        public override bool CheckNextState(AIStateEnum next)
        {
            if (next == AIStateEnum.COMBAT || next == AIStateEnum.CAST_SKILL)
            {
                GetAttr().StartCoroutine(SkillState());
                return false;
            }
            return base.CheckNextState(next);
        }

        private bool inSkill = false;

        IEnumerator SkillState()
        {
            if (inSkill)
            {
                yield break;
            }
            Log.AI("LastMsgIs: "+lastMsg.type);
            if (lastMsg == null || lastMsg.cmd == null)
            {
                yield break;
            }
            var sp = lastMsg.cmd.targetPos;
            var dir = lastMsg.cmd.dir;
            
            inSkill = true;
            var st = new TankShoot();
            st.skillPos = sp;
            st.skillDir = dir;
            st.IsStatic = lastMsg.cmd.staticShoot;
            st.skillAction = lastMsg.cmd.skillAction;
            aiCharacter.AddTempState(st);
            yield return GetAttr().StartCoroutine(st.RunLogic());
            inSkill = false;
        }
    }
}