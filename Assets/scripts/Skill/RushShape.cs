using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class RushShape : MonoBehaviour
    {
        public float Angle = 60;
        public float radius = 4;
        public float speed = 48;
        public float Distance = 12;

        private SkillLayoutRunner runner;
        private Vector3 InitPosition;
        private Vector3 targetPos;
        private HashSet<GameObject> hurtEnemy = new HashSet<GameObject>();
        private float cosAngle;

        private void Start()
        {
            runner = transform.parent.GetComponent<SkillLayoutRunner>();
            InitPosition = runner.transform.position;
            targetPos = InitPosition + transform.forward*Distance;
            cosAngle = Mathf.Cos(Mathf.Deg2Rad*Angle);

            StartCoroutine(MoveOwner());
        }

        private void Update()
        {
            if (!runner || !runner.stateMachine || !runner.stateMachine.attacker || runner.stateMachine.isStop)
            {
                return;
            }

            var me = runner.stateMachine.attacker;
            Collider[] hitColliders;
            hitColliders = Physics.OverlapSphere(me.transform.position, radius,
                SkillDamageCaculate.GetDamageLayer());

            for (int i = 0; i < hitColliders.Length; i++)
            {
                var enemy = NetworkUtil.GetAttr(hitColliders[i].gameObject);
                if (enemy != null && SkillLogic.IsEnemy(me, enemy.gameObject))
                {
                    if (!hurtEnemy.Contains(enemy.gameObject))
                    {
                        var dir = enemy.transform.position - me.transform.position;
                        dir.y = 0;
                        var cos = Vector3.Dot(dir.normalized, transform.forward);
                        Log.AI("DamageHit " + runner.stateMachine.name + " " + enemy.name+" cos "+cos+" a "+cosAngle);
                        if (cos > cosAngle)
                        {
                            DoDamage(enemy.gameObject);
                            hurtEnemy.Add(enemy.gameObject);
                        }
                    }
                }
            }
            Log.AI("Check Damage Shape " + runner.stateMachine.name);
        }

        private IEnumerator MoveOwner()
        {
            var ret = runner.stateMachine.attacker.GetComponent<TankPhysicComponent>().EnterSkillMoveState();
            if (!ret)
            {
                yield break;
            }

            float diff = 0;
            float halfDist = Distance/2.0f;

            //小于一半距离 则加速流程
            //需要移动到目标位置 targetPos 控制移动速度
            do
            {
                if (!runner || !runner.stateMachine || !runner.stateMachine.attacker)
                {
                    break;
                }

                diff = Mathf.Sqrt(Util.XZSqrMagnitude(targetPos, runner.stateMachine.attacker.transform.position));
                var newSpeed = speed;
                if (diff < halfDist)
                {
                    newSpeed = diff/halfDist*speed;
                }

                runner.MoveOwner(targetPos, newSpeed);
                yield return null;
            } while (diff > 0.2f && !runner.stateMachine.isStop);

            Log.Sys("ExitSkillMove :" + diff);
            var tank = runner.stateMachine.attacker.GetComponent<TankPhysicComponent>();
            if (tank != null)
            {
                tank.ExitSkillMove();
            }
            else
            {
                runner.stateMachine.attacker.GetComponent<PhysicComponent>().ExitSkillMove();
            }
        }

        private void DoDamage(GameObject g)
        {
            Log.Sys("DoDamage: " + g + " runner " + runner);
            runner.DoDamage(g);
        }
    }
}