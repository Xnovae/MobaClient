using UnityEngine;
using System.Collections;

namespace MyLib
{
    /// <summary>
    /// 激光的延迟Buff要避免和使用技能的延迟Buff重叠出现
    /// </summary>
    public class LaserDamage : MonoBehaviour
    {
        private NpcAttribute me;
        public int PlayerId;

        private void OnDestroy()
        {
            if (me != null)
            {
                GameInterface_Skill.RemoveSkillBuff(me.gameObject, (int) GameBuff.LaserDamageHP);
                GameInterface_Skill.RemoveSkillBuff(me.gameObject, Affix.EffectType.SlowDown2);
                me = null;
            }
        }

        //0.1s 开启关闭
        private void OnTriggerEnter(Collider other)
        {
            var meId = ObjectManager.objectManager.GetMyServerID();
            if (meId == PlayerId)
            {
                return;
            }
            var attr = NetworkUtil.GetAttr(other.gameObject);
            if (attr != null)
            {
                Log.Sys("LaserEnter: " + other.gameObject);
                if (attr.IsMine())
                {
                    me = attr;
                    GameInterface_Skill.AddSkillBuff(me.gameObject, (int) GameBuff.LaserDamageHP, Vector3.zero, PlayerId);
                    GameInterface_Skill.AddSkillBuff(me.gameObject, "SlowDown2", Vector3.zero, PlayerId);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var attr = NetworkUtil.GetAttr(other.gameObject);
            if (attr != null)
            {
                if (attr.IsMine())
                {
                    GameInterface_Skill.RemoveSkillBuff(attr.gameObject, (int) GameBuff.LaserDamageHP);
                    GameInterface_Skill.RemoveSkillBuff(attr.gameObject, Affix.EffectType.SlowDown2);
                    me = null;
                }
            }
        }
    }

    public class LaserBullet : MonoBehaviour
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

        private Vector3 initPos;

        // Use this for initialization
        private void Start()
        {
            if (missileData.ActiveParticle != null)
            {
                GameObject par = Instantiate(missileData.ActiveParticle) as GameObject;
                par.transform.parent = transform;
                par.transform.localPosition = Vector3.zero;
                par.transform.localRotation = Quaternion.identity;

                var ld = par.transform.Find("collider").gameObject.AddComponent<LaserDamage>();
                ld.PlayerId = this.attacker.GetComponent<NpcAttribute>().GetNetView().GetServerID();
            }
            StartCoroutine(WaitDamage());
        }


        private IEnumerator WaitDamage()
        {
            var tower = attacker.GetComponent<TankPhysicComponent>().tower;
            var passTime = 0.0f;
            while (passTime < 5 && attacker != null)
            {
                var dir = tower.transform.eulerAngles.y;
                this.transform.localRotation = Quaternion.Euler(0, dir, 0);

                passTime += Time.deltaTime;
                yield return null;
            }
            GameObject.Destroy(gameObject);
        }

    }
}
