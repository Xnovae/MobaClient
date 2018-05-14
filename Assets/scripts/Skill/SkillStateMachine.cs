using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MyLib
{
    /// <summary>
    /// 限制所有的事件只处理一次
    /// OnHit之后不再处理OnHit 或者状态一直维持
    /// </summary>
    public class SkillStateMachine : MonoBehaviour
    {
        /// <summary>
        /// 用于计算技能结果的命令
        /// </summary>
        public GCPlayerCmd cmd;
        /// <summary>
        /// 默认技能状态机 存在时间 不超过5s 
        /// </summary>
        public float lifeTime = 5;
        /// <summary>
        /// 所有技能运行层 
        /// </summary>
        List<GameObject> allRunners = new List<GameObject>();
        /// <summary>
        /// 技能状态机是否已经停止了 
        /// </summary>
        public bool isStop = false;

        public bool isStaticShoot = false;
        /// <summary>
        /// 技能状态机初始位置 
        /// </summary>
        public Vector3 InitPos = Vector3.zero;

        //需要前摇时间的技能标记目标位置 记录当前攻击目标的位置
        public Vector3 MarkPos;
        //攻击者 攻击目标
        public GameObject attacker;
        public GameObject target;
        //技能相关数据
        public SkillFullInfo skillFullData;
        public SkillDataConfig skillDataConfig;
   
        //子弹设置了朝向
        public bool forwardSet = false;
        private Vector3 forwardDir;
        public void SetForwardDirection(Vector3 f) {
            forwardSet = true;
            forwardDir = f;
        }
        private void Awake()
        {
            
        }

        private bool delay = false;
        public void SetDelay()
        {
            delay = true;
        }

        public void SetStaticShoot(bool isStatic)
        {
            isStaticShoot = isStatic;
        }

        public int ownerLocalId = -1;
        //注册监听技能相关事件  攻击命中事件 子弹命中或者死亡 攻击动画结束
        static List<MyEvent.EventType> regEvt = new List<MyEvent.EventType>(){
            MyEvent.EventType.EventTrigger,
            MyEvent.EventType.EventMissileDie,
            MyEvent.EventType.AnimationOver,
        };

        //计算技能伤害
        public void DoDamage(GameObject g)
        {
            SkillDamageCaculate.DoDamage(attacker, skillFullData, g, isStaticShoot);
        }
        //注册事件处理
        void RegEvent()
        {
            Log.AI("regevent is " + regEvt.Count);
            foreach (MyEvent.EventType e in regEvt)
            {
                MyEventSystem.myEventSystem.RegisterLocalEvent(ownerLocalId, e, OnEvent);
            }
        }
        //取消事件处理
        void UnRegEvent()
        {
            foreach (MyEvent.EventType e in regEvt)
            {
                MyEventSystem.myEventSystem.DropLocalListener(ownerLocalId, e, OnEvent);
            }
        }
        //取消特定事件 防止当前状态机接收到 玩家新的动作事件
        void UnRegEvent(MyEvent.EventType evt)
        {
            MyEventSystem.myEventSystem.DropLocalListener(ownerLocalId, evt, OnEvent);
        }

        /// <summary>
        /// 初始化特定事件发生时候的技能层
        /// 或者 创建孩子技能 
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="evt">Evt.</param>
        void InitLayout(SkillDataConfig.EventItem item, MyEvent evt)
        {
            if (item.layout != null)
            {
                var g = Instantiate(item.layout) as GameObject;
                g.transform.parent = transform;

                //陷阱粒子效果 位置是 当前missile爆炸的位置
                //瞬间调整SkillLayout的方向为 攻击者的正方向
                g.transform.localPosition = InitPos;
                float y;
                if(forwardSet) {
                    y = Quaternion.LookRotation(forwardDir).eulerAngles.y;
                }else {
                    y = attacker.transform.localRotation.eulerAngles.y;
                }
                g.transform.localRotation = Quaternion.Euler(new Vector3(0, y, 0));
                g.transform.localScale = Vector3.one;
            
            
                var runner = g.AddComponent<SkillLayoutRunner>();
                runner.stateMachine = this;
                runner.Event = item;
                runner.triggerEvent = evt;
                allRunners.Add(g);
                Log.AI("SkillLayout " + item.layout.name);
            } else if (item.childSkillId != 0 && item.childSkillId != -1)
            {
                Log.AI("Create Child Skill " + item.childSkillId);
                SkillLogic.CreateSkillStateMachine(attacker, Util.GetSkillData(item.childSkillId, 1), evt.missile.position);
            }
            
        }

        /// <summary>
        /// 动画结束 取消对命中事件处理
        /// </summary>
        void OnAnimationOver()
        {
            UnRegEvent(MyEvent.EventType.EventTrigger); 
        }

        /// <summary>
        /// 有可能存在多次命中事件 
        /// 格斗游戏 每一帧的 伤害判定窗口
        /// </summary>
        void OnHit()
        {
            Log.AI("Show Skill Hit Event " + gameObject.name);
            if (!isStop)
            {
                if (skillDataConfig != null)
                {
                    foreach (SkillDataConfig.EventItem item in skillDataConfig.eventList)
                    {
                        if (item.evt == SkillDataConfig.SkillEvent.EventTrigger)
                        {
                            InitLayout(item, null);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 子弹命中目标 取消命中判定 单个子弹命中
        /// </summary>
        /// <param name="evt">Evt.</param>
        void OnMissileDie(MyEvent evt)
        {
            Log.AI("Missile Die Receive");
            foreach (SkillDataConfig.EventItem item in skillDataConfig.eventList)
            {
                if (item.evt == SkillDataConfig.SkillEvent.EventMissileDie)
                {

                    InitLayout(item, evt);
                }
            }
            UnRegEvent(MyEvent.EventType.EventMissileDie);
        }

        IEnumerator DelayShowBullet(SkillDataConfig.EventItem item)
        {
            yield return new WaitForSeconds(0.4f);
            InitLayout(item, null);
        }
        /// <summary>
        /// 开始事件处理 
        /// </summary>
        void OnStart()
        {
            if (skillDataConfig != null)
            {
                foreach (SkillDataConfig.EventItem item in skillDataConfig.eventList)
                {
                    if (item.evt == SkillDataConfig.SkillEvent.EventStart)
                    {
                        if (delay)
                        {
                            StartCoroutine(DelayShowBullet(item));
                        }
                        else
                        {
                            InitLayout(item, null);
                        }
                    }
                }
                Log.Sys("SkillFullData.skillData: "+skillFullData.skillData.Sound+" ");
                if (!string.IsNullOrEmpty(skillFullData.skillData.Sound))
                {
                    BackgroundSound.Instance.PlayEffect("skill/" + skillFullData.skillData.Sound, 0.2f);
                }
            }
        }

        public void OnEvent(MyEvent evt)
        {
            switch (evt.skillEvtType)
            {
                case SkillDataConfig.SkillEvent.EventTrigger:
                    OnHit();
                    break;
                case SkillDataConfig.SkillEvent.EventMissileDie:
                    OnMissileDie(evt);
                    break;
                case SkillDataConfig.SkillEvent.AnimationOver:
                    OnAnimationOver();
                    break;
            }
        }

        void OnDestroy()
        {
            UnRegEvent();
        }

        //Start
        void Start()
        {
            lifeTime = skillFullData.skillData.skillConfig.lifeTime/1000.0f;
            RegEvent();
            StartCoroutine(FinishSkill());
            OnStart();
        }

        /// <summary>
        /// 一定时间之后销毁所有的技能对象清理 
        /// </summary>
        /// <returns>The skill.</returns>
        IEnumerator FinishSkill()
        {
            Log.AI("Finish Skill Here " + gameObject.name);
            yield return new WaitForSeconds(lifeTime);
            foreach (GameObject r in allRunners)
            {
                GameObject.Destroy(r);
            }
            GameObject.Destroy(gameObject);
        }

        //玩家连击伤害的时候，一个动作结束则 终止这个动作的技能状态机
        public void Stop()
        {
            isStop = true;
            MyEventSystem.myEventSystem.PushLocalEvent(attacker.GetComponent<NpcAttribute>().GetLocalId(), MyEvent.EventType.HideWeaponTrail);
        }
    }
}