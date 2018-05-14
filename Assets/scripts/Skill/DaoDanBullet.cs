using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class DaoDanBullet : MonoBehaviour
    {
        public Vector3 OffsetPos;
        public SkillData skillData;

        public GameObject attacker
        {
            get { return this.runner.stateMachine.attacker; }
        }
        public SkillLayoutRunner runner;
        public MissileData missileData;

        private bool IsDie = false;

        private GameObject target;
        private float velocity;


        private Vector3 initPos;
        private float maxDistance;
        /// <summary>
        /// 不支持反弹 命中即爆炸
        /// </summary>
        private void Start()
        {
            target = runner.stateMachine.target;
            velocity = missileData.Velocity;
            initPos = transform.position;
            maxDistance = missileData.MaxDistance;

            if (missileData.ActiveParticle != null)
            {
                GameObject par = Instantiate(missileData.ActiveParticle) as GameObject;
                par.transform.parent = transform;
                par.transform.localPosition = Vector3.zero;
                par.transform.localRotation = Quaternion.identity;
            }
        }

        private float runTime;
        private void RunForward()
        {
            transform.position += transform.forward*velocity*Time.deltaTime;
            if ((transform.position - initPos).sqrMagnitude >= maxDistance*maxDistance)
            {
                HitSomething();
            }
        }

        private void Update()
        {
            if (IsDie)
            {
                return;
            }

            if (target == null)
            {
                RunForward();
                return;
            }

            var dir = target.transform.position - transform.position;
            dir.y = 0;
            //HitTarget
            if (dir.sqrMagnitude < 1)
            {
                HitSomething();
            }
            else
            {
                dir.Normalize();

                var curDir = transform.forward;
                var diffDir = Quaternion.FromToRotation(curDir, dir);
                var diffY = diffDir.eulerAngles.y;
                diffY = Util.NormalizeDiffDeg(diffY);
                var oldY = this.transform.rotation.eulerAngles.y;

                var dy = Mathf.Clamp(diffY, -15, 15);
                var newDir = Quaternion.Euler(new Vector3(0, oldY+dy, 0));
                transform.rotation = newDir;
                var forward = transform.forward;
                transform.position += forward*velocity*Time.deltaTime;
            }
        }

        private void HitSomething()
        {
            IsDie = true;
            DoDamage();
            CreateHitParticle();
            GameObject.Destroy(gameObject);
        }

        private void DoDamage()
        {
            var oattr = NetworkUtil.GetAttr(target);
            if(oattr != null)
            {
                this.runner.DoDamage(oattr.gameObject);
            }
        }
        
        private void CreateHitParticle()
        {
            if (missileData.HitParticle != null)
            {
                var g = ParticlePool.Instance.GetGameObject(missileData.HitParticle, ParticlePool.InitParticle);
                var removeSelf = NGUITools.AddMissingComponent<DumpMono>(g);
                removeSelf.StartCoroutine(DestoryBullet(g));
                g.transform.position = transform.position;
                g.transform.parent = ObjectManager.objectManager.transform;
            }
            MakeSound();
        }

        private void MakeSound()
        {
            if (!string.IsNullOrEmpty(skillData.HitSound))
            {
                BackgroundSound.Instance.PlayEffect(skillData.HitSound, 0.3f);
            }
        }

        private IEnumerator DestoryBullet(GameObject go)
        {
            yield return new WaitForSeconds(2);
            ParticlePool.Instance.ReturnGameObject(go, ParticlePool.ResetParticle);
        }

    }
}
