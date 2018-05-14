using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyLib
{
    [RequireComponent(typeof(MobaOtherSync))]
    [RequireComponent(typeof(MobaMePhysic))]
    [RequireComponent(typeof(MobaModelLoader))]
    [RequireComponent(typeof(MobaCheckInGrass))]
    public class MobaOtherPlayer : AIBase
    {
        private void Awake()
        {
            attribute = GetComponent<NpcAttribute>();
            ai = new MobaPlayerCharacter();
            ai.attribute = attribute;
            ai.AddState(new MobaOtherIdle());
            ai.AddState(new MobaOtherMove());
            ai.AddState(new MobaMePlayerSkill());
            ai.AddState(new MobaMeDead());
        }

        private void Start()
        {
            ai.ChangeState(AIStateEnum.IDLE);
        }
    }
}
