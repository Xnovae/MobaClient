using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MyLib
{
    public class VirtualController
    {
        public Vector2 inputVector = Vector2.zero;
    }

    /// <summary>
    ///接受键盘和屏幕虚拟摇杆移动输入
    /// 以及网络命令移动 
    /// 
    /// </summary>
    public class MoveController : KBEngine.KBMonoBehaviour
    {
        public VirtualController vcontroller = new VirtualController();
        public Vector3 camRight;
        public Vector3 camForward;
        public Vector3 directionTo;
        private Vector2 cr, cf;

        public int logicFrameId = 0;

        // Use this for initialization
        void Start()
        {
            var npc = GetComponent<NpcAttribute>();
            if (npc.GetNetView().IsMe)
            {
                regLocalEvt = new List<MyEvent.EventType>() {
                    MyEvent.EventType.MovePlayer,
                };
            }
            RegEvent();
            Log.Sys("Init Move Player");

            camRight = CameraController.Instance.transform.TransformDirection(Vector3.right);
            camForward = CameraController.Instance.transform.TransformDirection(Vector3.forward);
            camRight.y = 0;
            camForward.y = 0;
            camRight.Normalize();
            camForward.Normalize();

            cr = new Vector2(camRight.x, camRight.z);
            cf = new Vector2(camForward.x, camForward.z);
        }

        /// <summary>
        /// 主镜头旋转了180角度
        /// 输入的左右也要调整 
        /// 
        /// 区分网络移动命令和本地移动命令
        /// 本地移动命令
        /// </summary>
        /// <param name="evt">Evt.</param>
        protected override void OnLocalEvent(MyEvent evt)
        {
            if (evt.type == MyEvent.EventType.MovePlayer)
            {
                vcontroller.inputVector = evt.vec2;
            }
        }
        /// <summary>
        /// 直线移动到目标位置
        /// </summary>
        /// <param name="pos"></param>
        /// <param name=""></param>
        public void MoveTo(ObjectCommand cmd)
        {
            logicFrameId++;
            runTime = Util.FrameToFloat(cmd.runFrame);
            passTime = 0.0f;
            curPos = transform.position;
            targetPos = cmd.targetPos;
            var deltaPos = targetPos - curPos;
            deltaPos.y = 0;
            var dx = deltaPos.x;
            var dz = deltaPos.z;
            var moveDir = new Vector2(dx, dz);
            moveDir.Normalize();
            directionTo = new Vector3(moveDir.x, 0, moveDir.y);
            var hval = Vector2.Dot(moveDir, cr);
            var vval = Vector2.Dot(moveDir, cf);

            vcontroller.inputVector.x = hval;
            vcontroller.inputVector.y = vval;
            state = State.InMove;
        }

        private void Update()
        {
            if(state == State.InMove)
            {
                passTime += Time.deltaTime;
                if(passTime >= runTime)
                {
                    state = State.IDLE;
                    vcontroller.inputVector = Vector2.zero;
                }
            }
        }

        public Vector3 curPos;
        public Vector3 targetPos;
        public float runTime = 0.0f;
        private float passTime = 0.0f;
        private enum State
        {
            IDLE,
            InMove,
        }
        private State state = State.IDLE;
    }

}