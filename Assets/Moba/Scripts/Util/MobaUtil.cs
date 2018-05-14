using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StatePrinting;

namespace MyLib
{
    public static class MobaUtil
    {
        public static readonly float FixDist = 2f * 2f;

        /// <summary>
        /// 根据摇杆操控或者网络控制得到玩家的朝向
        /// 防止朝向抖动
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetNetTurnTo(Vector2 inputDir, Vector3 netDir, float dist, Vector3 oldForward)
        {
            bool isLocalMoving = Mathf.Abs(inputDir.x) > 0.1f || Mathf.Abs(inputDir.y) > 0.1f;
            if (isLocalMoving)
            {
                var targetDirection = inputDir.x * CameraController.cameraController.camRight + inputDir.y * CameraController.cameraController.camForward;
                var mdir = targetDirection.normalized;
                return mdir;
            }
            if (dist > 0.1f)
            {
                return netDir;
            }
            return oldForward;
        }

        /// <summary>
        /// 根据本地操控位置和网络操控位置的差异 降低移动速度
        /// </summary>
        /// <param name="localPos"></param>
        /// <param name="netPos"></param>
        /// <returns></returns>
        public static float GetSpeedDecCoff(Vector3 localPos, Vector3 netPos)
        {
            var minD = 0.1f;
            var maxD = 1f;

            var deltaPos = netPos - localPos;
            deltaPos.y = 0;
            var len = deltaPos.magnitude;
            var r = Mathf.Clamp01((len - minD) / (maxD - minD));
            //距离越大速度越小
            var rate = Mathf.Lerp(0.8f, 0.0f, r);
            return rate;
        }

        /// <summary>
        /// 根据当前位置和网络位置计算需要加速的速度
        /// </summary>
        /// <param name="curPos"></param>
        /// <param name="tarPos"></param>
        /// <returns></returns>
        public static float GetSpeedCoff(Vector3 curPos, Vector3 tarPos)
        {
            float minDist = 1.0f;
            float maxDist = 2.0f;

            var deltaPos = tarPos - curPos;
            deltaPos.y = 0;
            var len = deltaPos.magnitude;
            var r = Mathf.Clamp01((len - minDist) / (maxDist-minDist));
            //线性插值
            var rate = Mathf.Lerp(1.0f, 2.0f, r);
            return rate;
        }

        public static float DeltaFrameToTime(ulong dif)
        {
            return dif * 0.1f;
        }
        public static bool IsServerMoveBySpeedOrPos(ISyncInterface sync)
        {
            var speed = sync.GetServerVelocity();
            if (IsServerMove(speed))
            {
                return true; 
            }

            var pos = sync.GetServerPos();
            var posDelta = IsNetMove(pos, sync.transform.position);
            return posDelta;
        }
        public static bool IsServerMove(MyVec3 speed)
        {
            return speed.x != 0 || speed.z != 0;
        }
        public static Vector3 DeltaPos(EntityInfo p1, EntityInfo p0)
        {
            var dx = p1.X - p0.X;
            var dz = p1.Z - p0.Z;
            return new Vector3(dx, 0, dz);
        }

        public static Vector3 DeltaPos(AvatarInfo p1, AvatarInfo p0)
        {
            var dx = p1.X - p0.X;
            var dz = p1.Z - p0.Z;
            return new Vector3(dx, 0, dz);
        }

        public static Vector3 Speed(Vector3 dp, float dt)
        {
            return dp / dt;
        }

        public static bool IsLocalMove(VirtualController vcontroller)
        {
            float v = 0;
            float h = 0;
            if (vcontroller != null)
            {
                h = vcontroller.inputVector.x;//CameraRight 
                v = vcontroller.inputVector.y;//CameraForward
            }
            bool isMoving = Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;
            return isMoving;
        }
        /// <summary>
        /// 坐标从整数 转化为浮点数
        /// </summary>
        /// <param name="intPos"></param>
        /// <returns></returns>
        public static Vector3 FloatPos(Vector3 intPos)
        {
            return Util.NetToGameVec(intPos);
        }

        public static Vector3 FloatPos(EntityInfo curInfo)
        {
            return NetworkUtil.FloatPos(curInfo.X, curInfo.Y, curInfo.Z);
        }

        public static Vector3 FloatPos(AvatarInfo curInfo)
        {
            return NetworkUtil.FloatPos(curInfo.X, curInfo.Y, curInfo.Z);
        }

        public static bool IsNetMove(Vector3 netPos, Vector3 curPos)
        {
            var difPos = Util.XZSqrMagnitude(netPos, curPos);
            return (difPos > FixDist);
        }
        public static bool IsNetMove(EntityInfo curInfo, Vector3 curPos)
        {
            var netPos = FloatPos(curInfo);
            var difPos = Util.XZSqrMagnitude(netPos, curPos);
            return (difPos > FixDist);
        }
        public static bool IsNetMove(AvatarInfo curInfo, Vector3 curPos)
        {
            var netPos = FloatPos(curInfo);
            var difPos = Util.XZSqrMagnitude(netPos, curPos);
            return (difPos > FixDist);
        }


        public static void DoNetworkDamage(GameObject me, GCPlayerCmd cmd)
        {
            var eid = cmd.DamageInfo.Enemy;
            //var enemy = ObjectManager.objectManager.GetPlayer(cmd.DamageInfo.Enemy);
            //if (enemy != null)
            {
                var dinfo = cmd.DamageInfo;
                //gameObject.GetComponent<MyAnimationEvent>().OnHit(attacker, cmd.DamageInfo.Damage, cmd.DamageInfo.IsCritical, cmd.DamageInfo.IsStaticShoot);
                me.GetComponent<NpcAttribute>().DoHurt(dinfo.Damage, dinfo.IsCritical, dinfo.IsStaticShoot);
            }
        }

        public static void DoNetworkAttack(GameObject gameObject, GCPlayerCmd proto)
        {
            var sk = proto.SkillAction;
            var cmd = new SkillCMD();
            cmd.skillId = sk.SkillId;
            cmd.skillLevel = sk.SkillLevel;
            cmd.staticShoot = sk.IsStaticShoot;
            cmd.targetPos = NetworkUtil.FloatPos(sk.X, sk.Y, sk.Z);
            cmd.dir = sk.Dir;
            cmd.skillAction = sk;
            cmd.runFrame = sk.RunFrame;
            cmd.proto = proto;

            Log.GUI("Other Player Attack LogicCommand:"+gameObject+":"+proto);
            gameObject.GetComponent<LogicCommand>().PushCommand(cmd);
        }

        public static void SetLevel(GameObject go, AvatarInfo info)
        {
            go.GetComponent<NpcAttribute>().ChangeLevel(info.Level);
        }

        public static void Dead(GameObject go)
        {
            var aibase = go.GetComponent<AIBase>();
            aibase.GetAI().GetAttr().IsDead = true;
            aibase.GetAI().ChangeState(AIStateEnum.DEAD);
        }

        private static Stateprinter printer = new Stateprinter();

        public static string PrintObj(object obj)
        {
            return printer.PrintObject(obj);
        }

        public static void SetPosWithHeight(GameObject go, Vector3 newPos)
        {
            var gm = GridManager.Instance;
            if (gm != null)
            {
                go.transform.position = gm.mapPosFixHeight(newPos);
            }
        }
    }
}
