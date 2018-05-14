using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MyLib
{
    [RequireComponent(typeof(MonsterSync))]
    [RequireComponent(typeof(MonsterSyncToServer))]
    public class TowerAI : AIBase
    {
        private void Awake()
        {
            attribute = GetComponent<NpcAttribute>();
            ai = new TowerCharacter();
            ai.attribute = attribute;
            ai.AddState(new TowerIdle());
            ai.AddState(new TowerAttack());
            ai.AddState(new TowerDead());
        }
        private void Start()
        {
            ai.ChangeState(AIStateEnum.IDLE);
        }
    }
}