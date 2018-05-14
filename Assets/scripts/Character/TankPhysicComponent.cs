using UnityEngine;
using System.Collections;

namespace MyLib
{
    //移动和旋转接口
    public class TankPhysicComponent : MonoBehaviour
    {
        Rigidbody rigid;
        Vector3 moveValue = Vector3.zero;
        private Vector3 mvDir;
        private bool rot = false;

        NpcAttribute attribute;
        public float maxVelocityChange = 3.0f;
        public float maxRotateChange = 3.0f;
        private float gravity = 20;

        public GameObject tower;
        private bool grounded = false;

        public static float Multi = 2;
        void Start()
        {

            //var box = Util.FindChildRecursive(transform, "boxColldier").gameObject;
            //rigid =  box.GetComponent<Rigidbody>();
            //暂时忽略坦克碰撞 允许坦克互相穿透
            Physics.IgnoreLayerCollision((int)GameLayer.Npc, (int)GameLayer.Npc);
            rigid = this.GetComponent<Rigidbody>();

            rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rigid.useGravity = true;
            attribute = GetComponent<NpcAttribute>();
            tower = Util.FindChildRecursive(transform, "tower").gameObject;
            maxRotateChange = attribute.ObjUnitData.jobConfig.rotateSpeed;
        }

        private bool inSkill = false;
        public bool EnterSkillMoveState()
        {
            if (inSkill == false)
            {
                inSkill = true;
                moveValue = Vector3.zero;
                return true;
            }
            return false;
        }

        public void ExitSkillMove()
        {
            moveValue = Vector3.zero;
            inSkill = false;
        }

        public void SkillMove(Vector3 pos, float speed)
        {
			var dir = pos - transform.position;
			dir.y = 0;
			Log.Sys("SkillMove Position "+pos+" speed "+speed+" dir "+dir);
			//冲击技能最大的移动速度不超过这些
			Log.Sys ("newSpeed:" +speed);
            moveValue = speed*dir.normalized;
        }


        public void MoveSpeed(Vector3 moveSpeed)
        {
            if (!inSkill)
            {
                moveValue = moveSpeed*attribute.GetMoveSpeedCoff();
            }
        }

        public void TurnTo(Vector3 moveDirection)
        {
            mvDir = moveDirection;
            rot = true;
        }
        

        //旋转炮台 射击时候 或者安静的时候自动归位
        public void TurnTower(Vector3 moveDirection)
        {
            return;
            var y = Quaternion.LookRotation(moveDirection).eulerAngles.y;
            Log.Sys("TowerRotate: " + y);
            tower.transform.rotation = Quaternion.Euler(new Vector3(0, y, 0));
        }

        void OnCollisionStay()
        {
            grounded = true;
        }

        void FixedUpdate()
        {
            var mv = moveValue*Time.fixedDeltaTime;
            this.rigid.MovePosition(this.rigid.position+mv);

            grounded = false;

            if (rot)
            {
                maxRotateChange = GameConst.Instance.MaxRotateChange;
                var forwardDir = mvDir;
                var curDir = transform.forward;
                curDir.y = 0;
                forwardDir.y = 0;

                var diffDir = Quaternion.FromToRotation(curDir, forwardDir);
                var diffY = diffDir.eulerAngles.y;
                Log.Sys("diffYIs: " + diffY);
                if (diffY > 180)
                {
                    diffY = diffY - 360;
                }
                if (diffY < -180)
                {
                    diffY = 360 + diffY;
                }

                var oldY = this.rigid.rotation.eulerAngles.y;

                var dy = Mathf.Clamp(diffY, -maxRotateChange, maxRotateChange);
                Log.Sys("DirY: " + dy + " diffY: " + diffY);
                var delta = Quaternion.Euler(new Vector3(0, dy*attribute.GetMoveSpeedCoff(), 0));
                this.rigid.MoveRotation(this.rigid.rotation*delta);
                rot = false;
                
                var newY = this.rigid.rotation.eulerAngles.y;
                var changeY = newY - oldY;
                var ta = tower.GetComponent<TowerAutoCheck>();
                ta.AdjustY(changeY);
            }
        }
        #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (ClientApp.Instance.testAI)
            {
                Gizmos.color = Color.red;
                var st = transform.position + new Vector3(0, 2, 0);
                var ed = st + moveValue * 4;
                Gizmos.DrawLine(st, ed);
                //Gizmos.DrawSphere(st, 4);
            }
        }
        #endif
    }


}