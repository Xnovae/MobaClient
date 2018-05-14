using MyLib;
using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class NuclearZone : MonoBehaviour
    {
        private NpcAttribute me;
        public int PlayerId;
        void Awake()
        {
            Util.ShowMsg("警告：核弹爆炸");
            StartCoroutine(ContinueBuff());
        }

        IEnumerator ContinueBuff()
        {
            while (true)
            {
                if (me != null)
                {
                    GameInterface_Skill.AddSkillBuff(me.gameObject, (int) GameBuff.NuclearDamageHP, Vector3.zero, PlayerId);
                }
                yield return new WaitForSeconds(1);
            }
        }

        private void OnDestroy()
        {
            if (me != null)
            {
                GameInterface_Skill.RemoveSkillBuff(me.gameObject, (int) GameBuff.NuclearDamageHP);
                me = null;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var attr = NetworkUtil.GetAttr(other.gameObject);
            if (attr != null)
            {
                if (attr.IsMine())
                {
                    Util.ShowMsg("警告：进入核弹区域");
                    me = attr;
                    GameInterface_Skill.AddSkillBuff(me.gameObject, (int)GameBuff.NuclearDamageHP, Vector3.zero, PlayerId);
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
                    Util.ShowMsg("退出核弹区域");
                    me = null;
                    GameInterface_Skill.RemoveSkillBuff(attr.gameObject, (int) GameBuff.NuclearDamageHP);
                }
            }
        }
    }

    [RequireComponent(typeof (MonsterSync))]
    [RequireComponent(typeof (MonsterSyncToServer))]
    public class NuclearAI : AIBase
    {
        private void Awake()
        {
 
            attribute = GetComponent<NpcAttribute>();
            attribute.ShowBloodBar = false;
            ai = new ChestCharacter();
            ai.attribute = attribute;
            ai.AddState(new ChestIdle());

        }

        private GameObject light;

        //召唤核弹 绿色辐射区域影响附近的敌人 比DamageZone要暴力很多 每秒 10hp
        //服务器记录Entity的生命剩余时间 瞬间发射的Entity 删除实体
        //总共25s 5s等待 20s作用
        private IEnumerator WaitBomb()
        {
            var ms = GetComponent<MonsterSync>();
            var waitTime = ms.curInfo.LifeLeft - GameConst.Instance.NuclearWorkTime;
            if (waitTime > 0)
            {
                yield return new WaitForSeconds(waitTime);
            }

            var pid = ms.curInfo.PlayerID;
            light = Object.Instantiate(Resources.Load<GameObject>("models/NuclearLight")) as GameObject;
            var nz = light.transform.Find("Collider").gameObject.AddComponent<NuclearZone>();
            nz.PlayerId = pid;
            light.transform.position = this.transform.position;
            var runTime = GameConst.Instance.NuclearWorkTime;
            if (waitTime <= 0)
            {
                runTime = ms.curInfo.LifeLeft;
            }
            yield return new WaitForSeconds(runTime);
            GameObject.Destroy(this.gameObject);
        }

        void OnDestroy()
        {
            Object.Destroy(light);
        }

        private void Start()
        {
            ai.ChangeState(AIStateEnum.IDLE);

            var pos = this.transform.position;
            pos.y = 0.5f;
            this.transform.position = pos;

            StartCoroutine(WaitBomb());
        }
    }
}