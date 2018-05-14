using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class DropBlockDead : DeadState
    {
        
    }
    [RequireComponent(typeof(MonsterSync))]
    [RequireComponent(typeof(MonsterSyncToServer))]
    public class DropAIController : AIBase 
    {
        void Awake()
        {
            attribute = GetComponent<NpcAttribute>();
            attribute.ShowBloodBar = false;

            ai = new ChestCharacter();
            ai.attribute = attribute;
            ai.AddState(new ChestIdle());
            ai.AddState(new DropBlockDead());

            Util.SetLayer(gameObject, GameLayer.IgnoreCollision2);
            if (NetworkUtil.IsMaster())
            {
                StartCoroutine(Drop());
            }
        }

        void Start()
        {
            ai.ChangeState(AIStateEnum.IDLE);
        }

        public GameObject dropGoods;
        IEnumerator Drop()
        {
            while (true)
            {
                yield return new WaitForSeconds(10);
                if (dropGoods == null)
                {
                    DropGoods.DropStaticGoods(attribute);
                }
            }
        }
       
    }
}