using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MyLib {

    /// <summary>
    /// 锁定目标子弹
    /// 飞行到目标位置就停止飞行移除掉
    /// </summary>
    public class BulletTargetFly : MonoBehaviour
    {
        public Vector3 OffsetPos;
        public SkillLayoutRunner runner;
        public MissileData missileData;
        private GameObject attacker;
        private float passTime = 0;
        private Vector3 initPos;
        private SkillData skillData;
        private GameObject target;

        private float flyTime;

        private Vector3 dieTargetPos;

        private void Start()
        {
            attacker = runner.stateMachine.attacker;
            target = runner.stateMachine.target;
            skillData = runner.stateMachine.skillFullData.skillData;

            //释放时候的粒子效果
            if (missileData.ReleaseParticle != null)
            {
                GameObject par = Instantiate(missileData.ReleaseParticle) as GameObject;
                NGUITools.AddMissingComponent<RemoveSelf>(par);
                if (runner.stateMachine.forwardSet)
                {
                    var playerForward = Quaternion.Euler(new Vector3(0, 0 + runner.transform.rotation.eulerAngles.y, 0));
                    par.transform.parent = ObjectManager.objectManager.transform;
                    par.transform.localPosition = runner.transform.localPosition + playerForward * OffsetPos;
                    par.transform.localRotation = playerForward;
                }
                else
                {
                    var playerForward =
                        Quaternion.Euler(new Vector3(0, 0 + attacker.transform.rotation.eulerAngles.y, 0));
                    par.transform.parent = ObjectManager.objectManager.transform;
                    par.transform.localPosition = attacker.transform.localPosition + playerForward * OffsetPos;
                    par.transform.localRotation = playerForward;
                }
            }

            //飞行粒子效果
            if (missileData.ActiveParticle != null)
            {
                GameObject par = Instantiate(missileData.ActiveParticle) as GameObject;
                par.transform.parent = transform;
                par.transform.localPosition = Vector3.zero;
                par.transform.localRotation = Quaternion.identity;
            }

            initPos = transform.position;
            if (target != null)
            {
                var tarPos = target.transform.position;
                var dist = tarPos - initPos;
                dist.y = 0;
                flyTime = dist.magnitude / missileData.Velocity;
                flyTime = Mathf.Max(flyTime, 0.5f);
            }else
            {
                var tarPos = initPos + transform.forward * missileData.Velocity * missileData.lifeTime;
                flyTime = missileData.lifeTime;
                dieTargetPos = tarPos;
                flyTime = Mathf.Max(flyTime, 0.5f);
            }
        }

        private void FixedUpdate()
        {
            passTime += Time.fixedDeltaTime;
            var rate = Mathf.Clamp01(passTime / flyTime);
            if (target != null)
            {
                var tarPos = target.transform.position;
                var newPos = Vector3.Lerp(initPos, tarPos, rate);
                newPos.y = initPos.y;
                transform.position = newPos;
            }
            else
            {
                var newPos = Vector3.Lerp(initPos, dieTargetPos, rate);
                newPos.y = initPos.y;
                transform.position = newPos;
            }

            if(passTime >= flyTime)
            {
                HitSomething();
            }
        }

        private void HitSomething()
        {
            CreateHitParticle();
            GameObject.Destroy(gameObject);
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
            CreateCameraShake();
        }

        private IEnumerator DestoryBullet(GameObject go)
        {
            yield return new WaitForSeconds(2);
            ParticlePool.Instance.ReturnGameObject(go, ParticlePool.ResetParticle);
        }

        private void MakeSound()
        {
            if (!string.IsNullOrEmpty(skillData.HitSound))
            {
                BackgroundSound.Instance.PlayEffect(skillData.HitSound, 0.3f);
            }
        }

        private void CreateCameraShake()
        {
            if (attacker != null && attacker.GetComponent<KBEngine.KBNetworkView>().IsMe)
            {
                if (missileData.shakeData != null)
                {
                    var shakeObj = new GameObject("CameraShake");
                    shakeObj.transform.parent = ObjectManager.objectManager.transform;
                    var shake = shakeObj.AddComponent<CameraShake>();
                    shake.shakeData = missileData.shakeData;
                    shake.autoRemove = true;
                }
            }
        }
    }
}
