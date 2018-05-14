using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyLib
{
    /// <summary>
    /// 具体AI可以NPC配置类型再分化
    /// </summary>
    [RequireComponent(typeof(MobaMeSync))]
    [RequireComponent(typeof(MobaMeSyncToServer))]
    [RequireComponent(typeof(MobaMePhysic))]
    [RequireComponent(typeof(MobaModelLoader))]
    [RequireComponent(typeof(MobaCheckInGrass))]
    public class MobaMePlayerAI : AIBase 
    {
        private void Awake()
        {
            attribute = GetComponent<NpcAttribute>();
            ai = new MobaPlayerCharacter();
            ai.attribute = attribute;
            ai.AddState(new MobaMePlayerIdle());
            ai.AddState(new MobaMePlayerMove());
            ai.AddState(new MobaMePlayerSkill());
            ai.AddState(new MobaMeDead());
            ai.AddState(new MobaMeStop());
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

    
        private void Start()
        {
            ai.ChangeState(AIStateEnum.IDLE);
        }
    }
}
