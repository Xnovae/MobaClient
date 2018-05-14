using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class LaserSpawn : MonoBehaviour
    {
        public SkillLayoutRunner runner;
        public MissileData missileData;
        public Vector3 Position;

        // Use this for initialization
        private void Start()
        {
            runner = this.transform.parent.GetComponent<SkillLayoutRunner>();
            MakeMissile();
        }

        private void MakeMissile()
        {
            var b = new GameObject("laser_" + missileData.name);
            var bullet = b.AddComponent<LaserBullet>();

            bullet.OffsetPos = Position;
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
