using System.Collections.Generic;
using MyLib;
using UnityEngine;
using System.Collections;


namespace MyLib
{
    /// <summary>
    /// 先进入了一个区域然后才退出了旧的区域
    /// 需要调整处理顺序
    /// 复活问题
    /// </summary>
    public class WarnZone : MonoBehaviour
    {
        private NpcAttribute me;
        public GameObject light;

        private void Start()
        {
            light.transform.parent = this.transform;
            light.gameObject.SetActive(true);
            Util.ShowMsg("警告：辐射区域出现");
            StartCoroutine(ContinueBuff());
        }

        IEnumerator ContinueBuff()
        {
            while (true)
            {
                if (me != null)
                {
                    GameInterface_Skill.AddSkillBuff(me.gameObject, (int) GameBuff.DamageHP, Vector3.zero);
                }
                yield return new WaitForSeconds(1);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            //Debug.LogError("OnTriggerEnter: "+other);
            var attr = NetworkUtil.GetAttr(other.gameObject);
            if (attr != null)
            {
                if (attr.IsMine())
                {
                    Util.ShowMsg("警告：进入辐射区域");
                    me = attr;
                    GameInterface_Skill.AddSkillBuff(me.gameObject, (int) GameBuff.DamageHP, Vector3.zero);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var attr = NetworkUtil.GetAttr(other.gameObject);
            if (attr != null)
            {
                if (attr == me)
                {
                    Util.ShowMsg("退出辐射区域");
                    me = null;
                    GameInterface_Skill.RemoveSkillBuff(attr.gameObject, (int) GameBuff.DamageHP);
                }
            }
        }
    }

    [RequireComponent(typeof (MonsterSync))]
    [RequireComponent(typeof (MonsterSyncToServer))]
    public class WarnZoneAI : AIBase
    {
        private void Awake()
        {
            attribute = GetComponent<NpcAttribute>();
            attribute.ShowBloodBar = false;
            ai = new ChestCharacter();
            ai.attribute = attribute;
            ai.AddState(new ChestIdle());

            Util.SetLayer(gameObject, GameLayer.IgnoreCollision2);


            var st = attribute.spawnTrigger;
            var sz = st.GetComponent<SpawnZone>();
            var old = sz.GetComponent<BoxCollider>();
            var go = new GameObject();
            go.transform.parent = transform;
            var bc = go.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.center = old.center;
            bc.size = old.size;
            Util.InitGameObject(go);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = sz.transform.localScale;

            var wz = go.AddComponent<WarnZone>();
            wz.light = sz.transform.Find("light").gameObject;

        }

        void Start()
        {
            ai.ChangeState(AIStateEnum.IDLE);
        }
    }
}
