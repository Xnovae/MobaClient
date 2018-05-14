using UnityEngine;
using System.Collections;

namespace MyLib
{
    /// <summary>
    /// 其它玩家tank 控制器
    /// </summary>
    [RequireComponent(typeof(AnimationController))]
    //[RequireComponent(typeof(PlayerSync))]
    [RequireComponent(typeof(TankPhysicComponent))]
    public class OtherTankAIController : AIBase 
    {
        void Awake()
        {
            attribute = GetComponent<NpcAttribute>();
            var tower = Util.FindChildRecursive(transform, "tower");
            tower.gameObject.AddComponent<TowerAutoCheck>();

            ai = new TankCharacter();
            ai.attribute = attribute;
            ai.AddState(new TankIdle());
            ai.AddState(new TankMoveAndShoot());
            //ai.AddState(new TankSkill());
            ai.AddState(new TankDead());
            //ai.AddState(new MonsterKnockBack());
            ai.AddState(new TankKnockBack());
            ai.AddState(new HumanStunned());
            ai.AddState(new TankStop());
        }

        void Start()
        {
            ai.ChangeState(AIStateEnum.IDLE);
        }

    }

}