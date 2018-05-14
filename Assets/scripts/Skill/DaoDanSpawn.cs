using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class DaoDanSpawn : MonoBehaviour
    {
        public SkillLayoutRunner runner;
        public MissileData missileData;
        public Vector3 Position;

        void Start()
        {
            runner = this.transform.parent.GetComponent<SkillLayoutRunner>();
            MakeMissile();
        }

        void MakeMissile()
        {
            var b = new GameObject("daodan_" + missileData.name);
            var bullet = b.AddComponent<DaoDanBullet>();
            bullet.OffsetPos = Position;
            Log.Sys("SkillState: "+runner.stateMachine);
            Log.Sys("SkillDatate: "+runner.stateMachine.skillFullData);
            bullet.skillData = runner.stateMachine.skillFullData.skillData;
            bullet.runner = runner;
            bullet.missileData = missileData;
            //bullet.transform.parent = ObjectManager.objectManager.transform;

            var bulletForward = Quaternion.Euler(new Vector3(0, runner.transform.eulerAngles.y, 0));
            bullet.transform.localPosition = runner.transform.localPosition + bulletForward * Position;
            bullet.transform.localRotation = bulletForward;

        }


    }
}