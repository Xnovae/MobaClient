using UnityEngine;
using System.Collections;

namespace MyLib
{
    public enum AIStateEnum
    {
        INVALID = -1,
        IDLE,
        COMBAT,
        DEAD,
        MOVE,

        FLEE,
        KNOCK_BACK,

        CAST_SKILL,
        Stunned,

        JUMP,

        MOVE_SHOOT,
        STOP,
    }

    public class AIState
    {
        protected AICharacter aiCharacter;
        protected bool quit = false;
        protected int runNum = 0;

        /// <summary>
        /// 添加进入状态机时调用
        /// </summary>
        public virtual void Init()
        {

        }
        public void SetChar(AICharacter a)
        {
            aiCharacter = a;
        }

        public AIStateEnum type = AIStateEnum.INVALID;
        /// <summary>
        ///构成一个Idle组成的树状结构
        /// Idle对应的状态有自己的层 
        /// </summary>
        public int layer = 0;

        public AIState()
        {
		
        }

        public virtual void RefreshState()
        {
            EnterState();
        }

        /// <summary>
        ///重载的时候 子类需要在while中判定是否quit状态
        /// </summary>
        public virtual IEnumerator RunLogic()
        {
            while (!quit)
            {
                if (CheckEvent())
                {
                    break;
                }
                yield return null;
            }
        }

        public static AIState CreateState(AIStateEnum s)
        {
            switch (s)
            {
                case AIStateEnum.IDLE:
                    return new IdleState();
                case AIStateEnum.COMBAT:
                    return new CombatState();
                case AIStateEnum.DEAD:
                    return new DeadState();
                case AIStateEnum.MOVE:
                    return new MoveState();
                default:
                    return null;
            }
        }

        /// <summary>
        /// 重载的时候 子类 需要 执行父类的enterState exitState 函数
        /// </summary>
        public virtual void EnterState()
        {
            runNum++;
            quit = false;
        }

        /// <summary>
        /// 重载的时候 子类 需要 执行父类的enterState exitState 函数
        /// </summary>
        public virtual void ExitState()
        {
            quit = true;
        }

        public virtual bool CheckNextState(AIStateEnum next)
        {
            return true;
        }

        public virtual bool CanChangeState(AIStateEnum next) {
            return CheckNextState(next);
        }

        protected void PlayAni(string name, float speed, WrapMode wm)
        {
            aiCharacter.PlayAni(name, speed, wm);
        }

        protected void SetAni(string name, float speed, WrapMode wm)
        {
            aiCharacter.SetAni(name, speed, wm);
        }

        protected bool CheckAni(string name)
        {
            return aiCharacter.GetAttr().GetComponent<Animation>().GetClip(name) != null;
        }

        protected void SetAttrState(CharacterState s)
        {
            aiCharacter.GetAttr()._characterState = s;
        }

        NpcAttribute attr;

        protected NpcAttribute GetAttr()
        {
            if (attr == null)
            {
                attr = aiCharacter.GetAttr();
            }
            return attr;
        }

        protected MyAnimationEvent GetEvent()
        {
            return GetAttr().GetComponent<MyAnimationEvent>();
        }

        protected MyAnimationEvent.Message lastMsg
        {
            get { return aiCharacter.lastMsg; }
            set { aiCharacter.lastMsg = value; }
        }


        //TODO:检测一些事件 然后进行状态切换
        //获得对应注册哪些事件， 就检测哪些事件？
        //只有状态切换成功才回 CheckEvent 返回true
        protected bool CheckEvent()
        {
            return false;
        }

        protected virtual bool CheckEventOverride(MyAnimationEvent.Message msg)
        {
            return false;
        }

    }

    public class IdleState  : AIState
    {
        public IdleState()
        {
            type = AIStateEnum.IDLE;
        }

    }

    public class CombatState : AIState
    {
        public CombatState()
        {
            type = AIStateEnum.COMBAT;
        }
    }

    public class MoveState : AIState
    {
        public MoveState()
        {
            type = AIStateEnum.MOVE;
        }

    }

    public class DeadState : AIState
    {
        public DeadState()
        {
            type = AIStateEnum.DEAD;
        }

        public override void EnterState()
        {
            base.EnterState();
            MyEventSystem.myEventSystem.PushLocalEvent(GetAttr().GetLocalId(), MyEvent.EventType.MeDead);
        }

        public override bool CheckNextState(AIStateEnum next)
        {
            return false;
        }
    }

    public class FleeState : AIState
    {
        public FleeState()
        {
            type = AIStateEnum.FLEE;
        }
    }

    public class KnockBackState : AIState
    {
        public KnockBackState()
        {
            type = AIStateEnum.KNOCK_BACK;
        }
    }

    public class SkillState : AIState
    {
        public SkillState()
        {
            type = AIStateEnum.CAST_SKILL;
        }

        public override bool CheckNextState(AIStateEnum next)
        {
            if (next == AIStateEnum.CAST_SKILL)
            {
                return false;
            }
            if (next == AIStateEnum.COMBAT)
            {
                return false;
            }
            return base.CheckNextState(next);
        }
    }

    public class StunnedState : AIState
    {
        public StunnedState()
        {
            type = AIStateEnum.Stunned;
        }

        public override bool CheckNextState(AIStateEnum next)
        {
            if (next == AIStateEnum.DEAD)
            {
                return true;
            }
            if (next == AIStateEnum.IDLE)
            {
                if (lastMsg != null)
                {
                    if (lastMsg.type == MyAnimationEvent.MsgType.EXIT_STUNNED)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    /// <summary>
    /// 避免跳跃过程中死亡状态突然出现怎么处理？
    /// 不要切换状态下次再检测CheckEvent 即可
    /// Message 会丢弃 但是 常驻的state不会 
    /// </summary>
    public class JumpState : AIState
    {
        public JumpState()
        {
            type = AIStateEnum.JUMP;
        }

        public override bool CheckNextState(AIStateEnum next)
        {
            if (next == AIStateEnum.IDLE)
            {
                return true;
            }
            return false;
        }
    }

    public class MoveShootState : AIState
    {
        public MoveShootState()
        {
            type = AIStateEnum.MOVE_SHOOT;
        }
    }
    public class StopState : AIState
    {
        public StopState()
        {
            type = AIStateEnum.STOP;
        }
    }
}