using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MyLib
{
    public class TankDead : DeadState
    {
        string dieAni;
        private GameObject deadparticle;
        private TowerAutoCheck tac;
        //public Transform tower;
        public override void EnterState()
        {
            base.EnterState();

            dieAni = "death";
            SetAni(dieAni, 1, WrapMode.Once);
            GetAttr().IsDead = true;
            if (GetAttr().IsMine())
            {
                var attackerList = GetEvent().attackerList;
                var last = GetEvent().lastAttacker;
                List<int> finalAttackerList = new List<int>(attackerList.Count);

                var enumerator = attackerList.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Key != last)
                    {
                        if (enumerator.Current.Value >= GameConst.Instance.AssistDamage)
                        {
                            finalAttackerList.Add(enumerator.Current.Key);
                        }
                    }
                }

                NetDateInterface.Dead(last, finalAttackerList);
                attackerList.Clear();
                GetEvent().lastAttacker = 0;
            }
            GetAttr().SetDeadShader();
           
            var tower = GetAttr().GetComponent<TankPhysicComponent>().tower;
            tac = tower.GetComponent<TowerAutoCheck>();
            tac.SetDead(true);
            var rigidbody = GetAttr().GetComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.velocity = Vector3.zero;

            var ds = GameObject.Instantiate(Resources.Load<GameObject>("particles/DeadSmoke")) as GameObject;
            deadparticle = ds;
            ds.transform.position = GetAttr().transform.position;
        }

        public override void ExitState()
        {
            tac.SetDead(false);
            GameObject.Destroy(deadparticle);
            GetAttr().SetLiveShader();
            base.ExitState();

            GetAttr().GetComponent<Rigidbody>().useGravity = true;
        }

        public override IEnumerator RunLogic()
        {
            yield return GetAttr().StartCoroutine(Util.WaitForAnimation(GetAttr().GetComponent<Animation>()));
            var evt = new MyEvent(MyEvent.EventType.PlayerDead);
            evt.localID = aiCharacter.GetLocalId();
            MyEventSystem.myEventSystem.PushEvent(evt);
            while (!quit)
            {
                //ClearEvent();
                yield return null;
            }
        }
    }
}
