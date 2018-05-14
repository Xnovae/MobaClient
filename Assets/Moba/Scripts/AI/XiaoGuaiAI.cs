using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyLib
{
    /// <summary>
    /// 服务器同步小怪
    /// </summary>
    [RequireComponent(typeof(MonsterSync))]
    [RequireComponent(typeof(MonsterSyncToServer))]
    [RequireComponent(typeof(MeleePhysicCom))]
    public class XiaoGuaiAI : AIBase
    {
        private void Awake()
        {
            attribute = GetComponent<NpcAttribute>();
            ai = new XiaoGuaiCharacter();
            ai.attribute = attribute;
            ai.AddState(new MeleeIdle());
            ai.AddState(new MeleeMove());
            ai.AddState(new MeleeAttack());
            ai.AddState(new XiaoGuaiDeath());

            attribute.ShowBloodBar = true;
        }

        void Start()
        {
            ai.ChangeState(AIStateEnum.IDLE);
        }
    }
}
