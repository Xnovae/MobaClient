using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MyLib
{
    public enum AIParam
    {
        SkillCmd,
    }
    public class AIEvent
    {
        public GCPlayerCmd cmd;
    }

    public class AICharacter
    {
        public Dictionary<AIParam, AIEvent> blackboard = new Dictionary<AIParam, AIEvent>();

        public bool canRelive = false;

        public AIState state
        {
            get;
            set;
        }

        private NpcAttribute _attr;
        public NpcAttribute attribute
        {
            get
            {
                return _attr;
            }
            set
            {
                _attr = value;
                Init();
            }
        }
        /// <summary>
        /// 初始化完模型调用该接口
        /// </summary>
        protected virtual void Init()
        {

        }
        Dictionary<AIStateEnum, AIState> stateMap = new Dictionary<AIStateEnum, AIState>();

       
        public NpcAttribute GetAttr()
        {
            return attribute;
        }

        public AICharacter()
        {
        }

        public int GetLocalId()
        {
            return attribute.GetComponent<KBEngine.KBNetworkView>().GetLocalId();
        }

        public void ChangeStateIgnoreCurState(AIStateEnum s)
        {
            if (state != null)
            {
                state.ExitState();
                if (stateCor.ContainsKey(state.type))
                {
                    attribute.StopCoroutine(stateCor[state.type]);
                }
            }
            state = stateMap[s];
            state.EnterState();
            var cor = attribute.StartCoroutine(state.RunLogic());
            stateCor[s] = cor;
        }

        private Dictionary<AIStateEnum, Coroutine> stateCor = new Dictionary<AIStateEnum, Coroutine>();



        public bool ChangeStateForce(AIStateEnum s)
        {
            Log.AI("Change State Force " + GetAttr().gameObject + " state " + s);

            if (state != null && state.type == s)
            {
                return false;
            }
            if (state != null)
            {
                state.ExitState();
            }
            state = stateMap [s];
            state.EnterState();
            attribute.StartCoroutine(state.RunLogic());
            return true;
        }

        public void Stop()
        {
            if (this.state != null)
            {
                this.state.ExitState();
            }
        }

        //TODO:状态机检测是否可以进入 其它状态
        public bool ChangeState(AIStateEnum s)
        {
            Log.AI("Change State " + GetAttr().gameObject + " state " + s);

            if (state != null && !state.CheckNextState(s))
            {
                return false;
            }

            if (!stateMap.ContainsKey(s))
            {
                //Debug.LogError("Who Not Has Such State "+GetAttr().gameObject+" state "+s);
                Log.Sys("gameObject No State " + GetAttr().gameObject + " state " + s);
                return false;
            }

            if (state != null && state.type == s)
            {
                return false;
            }

            if (state != null)
            {
                state.ExitState();
            }
            state = stateMap [s];
            state.EnterState();
            attribute.StartCoroutine(state.RunLogic());
            return true;
        }

        public MyAnimationEvent.Message lastMsg;

        public void AddState(AIState state)
        {
            if (stateMap.ContainsKey(state.type))
            {
                Log.AI("Error Has SameState In Map " + state.type + " " + stateMap [state.type] + " " + state);
            }
            stateMap [state.type] = state;
            state.SetChar(this);
            state.Init();
        }

        //增加临时状态
        public void AddTempState(AIState state)
        {
            state.SetChar(this);
        }

        private SkillInfoComponent GetSkill()
        {
            return GetAttr().GetComponent<SkillInfoComponent>();
        }

        public ObjectCommand lastCmd;
        public void OnCommand(ObjectCommand cmd)
        {
            lastCmd = cmd;
            if (cmd.commandID == ObjectCommand.ENUM_OBJECT_COMMAND.OC_USE_SKILL)
            {
                var skillData = Util.GetSkillData(cmd.skillId, cmd.skillLevel);
                var skillPart = GetSkill();
                skillPart.SetActiveSkill(skillData);
                ChangeState(AIStateEnum.CAST_SKILL);
            }else if(cmd.commandID == ObjectCommand.ENUM_OBJECT_COMMAND.OC_MOVE)
            {
                ChangeState(AIStateEnum.MOVE);
            }
        }

        public virtual IEnumerator ShowDead()
        {
            yield return null;
        }

        public virtual void SetRun()
        {
            throw new System.Exception("AI Characet Not Set Run " + GetAttr().gameObject.name);
        }

        public virtual void SetIdle()
        {
            throw new System.Exception("AI Characet Not Set Idle " + GetAttr().gameObject.name);
        }

        protected bool CheckAni(string name)
        {
            return GetAttr().GetComponent<Animation>().GetClip(name) != null;
        }

        public virtual void PlayAni(string name, float speed, WrapMode wm)
        {
			
            var ani = GetAttr().GetComponent<Animation>();
			
            ani [name].speed = speed;
            ani [name].wrapMode = wm;
            ani [name].time = 0;
            ani.Play(name);
        }
        public virtual void PlayAniInTime(string name, float time)
        {
            throw new System.NotImplementedException();
        }

        public virtual void SetAni(string name, float speed, WrapMode wm)
        {
            var ani = GetAttr().GetComponent<Animation>();

            ani [name].speed = speed;
            ani [name].wrapMode = wm;
            ani.CrossFade(name);
        }

        GameObject enemy;

        public void SetEnemy(GameObject e)
        {
            enemy = e;
        }

        public GameObject GetEnemy()
        {
            return enemy;
        }

        public virtual void RefreshState()
        {
            if(state != null)
            {
                state.RefreshState();
            }
        }

    }

}