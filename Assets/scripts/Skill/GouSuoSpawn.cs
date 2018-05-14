using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class GouSuoSpawn : MonoBehaviour
    {
        public SkillLayoutRunner runner;
        public MissileData missileData;
        public Vector3 Position;

        private void Start()
        {
            runner = this.transform.parent.GetComponent<SkillLayoutRunner>();
            MakeMissile();
        }

        private void MakeMissile()
        {
            var b = new GameObject("laser_" + missileData.name);
            var bullet = b.AddComponent<GouSuoBullet>();

            bullet.OffsetPos = Position;
            bullet.skillData = runner.stateMachine.skillFullData.skillData;
            bullet.runner = runner;
            bullet.missileData = missileData;

            var bulletForward = Quaternion.Euler(new Vector3(0, runner.transform.eulerAngles.y, 0));
            bullet.transform.localPosition = runner.transform.localPosition + bulletForward*Position;
            bullet.transform.localRotation = bulletForward;
        }
    }
}