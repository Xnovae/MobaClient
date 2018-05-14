using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class TankShoot : AIState
    {
        private SkillFullInfo activeSkill;
        private GameObject tower2;

        public Vector3 skillPos;
        public float skillDir;
        public SkillAction skillAction;

        public bool IsStatic { get; set; }

        public override IEnumerator RunLogic()
        {
            tower = GetAttr().GetComponent<TankPhysicComponent>().tower.transform;
            tower2 = Util.FindChildRecursive(tower, "tower2").gameObject;
            activeSkill = GetAttr().GetComponent<SkillInfoComponent>().GetActiveSkill();
            var skInfo = GetAttr().GetComponent<SkillInfoComponent>();

            MyEventSystem.PushLocalEventStatic(GetAttr().GetNetView().GetLocalId(), MyEvent.EventType.UseSkill);
            yield return GetAttr().StartCoroutine(Shoot());
        }

        private string GetAttackAniName()
        {
            var name = string.Format("rslash_{0}", 1);
            return name;
        }

        private string attackAniName;
        private Vector3 forward;

        IEnumerator Shoot()
        {
            var trans = GetAttr().transform;
            //var enemy = SkillLogic.FindNearestEnemy(trans.gameObject);
            GameObject enemy = null;
            var physic = GetAttr().GetComponent<TankPhysicComponent>();
            Log.Sys("FindEnemyIs: " + enemy);
            if (enemy != null)
            {
                var dir = enemy.transform.position - trans.position;
                dir.y = 0;
                Log.Sys("EnemyIs: " + dir);
                //forward = dir;
                forward = tower.forward;

                //physic.TurnToImmediately(dir);
                physic.TurnTower(dir);
            }
            else
            {
                forward = Quaternion.Euler(new Vector3(0, skillDir, 0)) * Vector3.forward;
                //forward = trans.forward;
                physic.TurnTower(forward);
            }

            attackAniName = GetAttackAniName();
            /*
            var realAttackTime = activeSkill.skillData.AttackAniTime / GetAttr().GetSpeedCoff();
            var rate = GetAttr().animation [attackAniName].length / realAttackTime;
            PlayAni(attackAniName, rate, WrapMode.Once);
            */
            yield return GetAttr().StartCoroutine(WaitForAttackAnimation(GetAttr().GetComponent<Animation>()));
            yield return new WaitForSeconds(0.1f);
        }

        IEnumerator ShootAni(float tm)
        {
            var sa = Resources.Load<GameObject>("CameraShake/shootAnim");
            var cs = sa.GetComponent<CameraShakeData>();
            var passTime = 0.0f;
            var initPos = tower2.transform.localPosition;
            while (passTime < tm)
            {
                var rate = passTime / tm;
                var v = cs.shakeCurve.Evaluate(rate);
                var pos = v * cs.MaxOffset;
                var newPos = initPos + new Vector3(0, 0, pos);
                tower2.transform.localPosition = newPos;
                yield return null;
                passTime += Time.deltaTime;
            }
            tower2.transform.localPosition = initPos;
        }
        private Transform tower;

        private IEnumerator WaitForAttackAnimation(Animation animation)
        {
            List<SkillStateMachine> sm = new List<SkillStateMachine>();
            GameObject targetPlayer = null;
            var target = skillAction.Target;
            if (target != 0)
            {
                targetPlayer = ObjectManager.objectManager.GetPlayer(target);
            }
            if (GetAttr().GetComponent<BuffComponent>().HasLianFa())
            {
                Log.Sys("LianFaShoot:");

                var skillStateMachine = SkillLogic.CreateSkillStateMachine(GetAttr().gameObject, activeSkill.skillData,
                    skillPos, targetPlayer);
                skillStateMachine.SetForwardDirection(forward);
                skillStateMachine.SetDelay();
                skillStateMachine.SetStaticShoot(IsStatic);
                sm.Add(skillStateMachine);

                skillStateMachine = SkillLogic.CreateSkillStateMachine(GetAttr().gameObject, activeSkill.skillData,
                    skillPos, targetPlayer);
                skillStateMachine.SetForwardDirection(forward);
                sm.Add(skillStateMachine);
            }
            else
            {
                var skillStateMachine = SkillLogic.CreateSkillStateMachine(GetAttr().gameObject, activeSkill.skillData,
                    skillPos, targetPlayer);
                skillStateMachine.SetForwardDirection(forward);
                skillStateMachine.SetStaticShoot(IsStatic);
                sm.Add(skillStateMachine);
            }

            Log.AI("Wait For Combat Animation");
            float passTime = 0;
            //yield return new WaitForSeconds(0.1f);
            yield return null;
            MyEventSystem.PushLocalEventStatic(GetAttr().GetLocalId(), MyEvent.EventType.EventTrigger);
            var realAttackTime = activeSkill.skillData.AttackAniTime/GetAttr().GetSpeedCoff();
            GetAttr().StartCoroutine(ShootAni(realAttackTime*0.8f));
            //realAttackTime -= 0.3f;
            do
            {
                if (passTime >= realAttackTime*0.8f)
                {
                    break;
                }
                passTime += Time.deltaTime;

                yield return null;
            } while (!quit);

            Log.Ani("Animation is Playing stop ");
            foreach (var skillStateMachine in sm)
            {
                skillStateMachine.Stop();
            }
        }
    }
}