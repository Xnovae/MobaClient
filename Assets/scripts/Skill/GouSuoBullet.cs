using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class GouSuoBullet : MonoBehaviour
    {
        public Vector3 OffsetPos;
        public SkillData skillData;

        public GameObject attacker
        {
            get { return this.runner.stateMachine.attacker; }
        }

        public SkillLayoutRunner runner;
        public MissileData missileData;
        private Vector3 initPos;

        private GameObject gousuoLine;
        private GameObject line;
        private GameObject head;
        private void Start()
        {

            GameObject par = Instantiate(missileData.ActiveParticle) as GameObject;
            par.transform.parent = transform;
            par.transform.localPosition = Vector3.zero;
            par.transform.localRotation = Quaternion.identity;
            gousuoLine = par;


            line = par.transform.Find("GouSuoLine").gameObject;
            head = par.transform.Find("GouSuoHead").gameObject;

            initPos = head.transform.position;

            Log.Sys("GouSuoCreate");
            StartCoroutine(MoveHead());
        }


        private bool CheckHit()
        {
            Collider[] col = Physics.OverlapSphere(head.transform.position, missileData.Radius,
                SkillDamageCaculate.GetDamageLayer());
            foreach (Collider c in col)
            {
                //和多个不同的敌人目标碰撞
                var hitT = NetworkUtil.GetAttr(c.gameObject);
                if (hitT != null)
                {
                    if (hitT.gameObject != this.attacker)
                    {
                        HitSomething(hitT);
                        return true;
                    }
                }
            }
            return false;
        }

        private void HitSomething(NpcAttribute hitTarget)
        {
            DoDamage(hitTarget.gameObject);
            CreateHitParticle();
            GameObject.Destroy(gameObject);
        }

        private void DestroyMe()
        {
            CreateHitParticle();
            GameObject.Destroy(gameObject);
        }
        private void DoDamage(GameObject target)
        {
            var oattr = NetworkUtil.GetAttr(target);
            if (oattr != null)
            {
                this.runner.DoDamage(oattr.gameObject);
            }
        }

        private void CreatePar(Vector3 pos)
        {
            var g = ParticlePool.Instance.GetGameObject(missileData.HitParticle, ParticlePool.InitParticle);
            var removeSelf = NGUITools.AddMissingComponent<DumpMono>(g);
            removeSelf.StartCoroutine(DestoryBullet(g));
            g.transform.position = pos;
            g.transform.parent = ObjectManager.objectManager.transform;
        }
        private void CreateHitParticle()
        {
            if (missileData.HitParticle != null)
            {
                CreatePar(transform.position);
                CreatePar(head.transform.position);
            }
            MakeSound();
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


        private IEnumerator MoveHead()
        {
            var totalTime = skillData.skillConfig.animationTime / 1000.0f;
            var passTime = 0.0f;

            Log.Sys("MoveHead: "+passTime+" tot "+totalTime);
            while (passTime < totalTime)
            {
                head.transform.position += head.transform.forward * missileData.Velocity * Time.deltaTime;
                var scale = head.transform.localPosition.z;
                line.transform.localScale = new Vector3(1, 1, scale);
                var diffDist = (head.transform.position - initPos).sqrMagnitude;
                if (diffDist > missileData.MaxDistance*missileData.MaxDistance)
                {
                    DestroyMe();
                    break;
                }
                passTime += Time.deltaTime;
                var ret = CheckHit();
                if (ret)
                {
                    break;
                }
                yield return null;
            }
        }


    }
}