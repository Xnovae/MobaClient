/*
Author: liyonghelpme
Email: 233242872@qq.com
*/

using System;
using KBEngine;
using UnityEngine;
using System.Collections;
using MonoBehaviour = UnityEngine.MonoBehaviour;
using Random = UnityEngine.Random;

namespace MyLib
{
    //多种类型子弹子类 bulletType
    public class Bullet : MonoBehaviour
    {
        public SkillLayoutRunner runner;
        private bool isDie = false;
        public SkillData skillData;

        public GameObject attacker
        {
            get { return this.runner.stateMachine.attacker; }
            set { }
        }

        public MissileData missileData;
        public int LeftRicochets = 0;
        private float velocity;
        private float maxDistance;
        private Vector3 initPos;
        private Collider lastColobj = null;
        //子弹相对于发射者的位置偏移
        public Vector3 OffsetPos;

        private int fanTanNum = 0;
        /// <summary>
        /// 释放子弹的粒子
        /// 子弹飞行粒子
        /// </summary>
        private void Start()
        {
            LeftRicochets = missileData.NumRicochets;
            velocity = missileData.Velocity;
            velocity = GameConst.Instance.BulletSpeed;
            maxDistance = missileData.MaxDistance;
            initPos = transform.position;

            if (missileData.ReleaseParticle != null)
            {
                GameObject par = Instantiate(missileData.ReleaseParticle) as GameObject;
                NGUITools.AddMissingComponent<RemoveSelf>(par);
                if (runner.stateMachine.forwardSet)
                {
                    var playerForward = Quaternion.Euler(new Vector3(0, 0 + runner.transform.rotation.eulerAngles.y, 0));
                    par.transform.parent = ObjectManager.objectManager.transform;
                    par.transform.localPosition = runner.transform.localPosition + playerForward*OffsetPos;
                    par.transform.localRotation = playerForward;
                }
                else
                {
                    var playerForward =
                        Quaternion.Euler(new Vector3(0, 0 + attacker.transform.rotation.eulerAngles.y, 0));
                    par.transform.parent = ObjectManager.objectManager.transform;
                    par.transform.localPosition = attacker.transform.localPosition + playerForward*OffsetPos;
                    par.transform.localRotation = playerForward;
                }
            }

            if (missileData.ActiveParticle != null)
            {
                GameObject par = Instantiate(missileData.ActiveParticle) as GameObject;
                par.transform.parent = transform;
                par.transform.localPosition = Vector3.zero;
                par.transform.localRotation = Quaternion.identity;
            }
        }

        /*  
        1自己   自己的宠物
        2其它玩家  其它玩家的宠物
        3怪物
        4墙体
5: 事件类型碰撞体不参与技能 例如入口和出口的碰撞体？ （忽略这种 假设所有都会碰撞）


对于子弹的发射者来讲
按照角色分成三种：
1：友方
2：敌方
3：中立（墙体）
        */

        private void FixedUpdate()
        {
            if (isDie)
            {
                return;
            }
            Collider[] col = Physics.OverlapSphere(transform.position, missileData.Radius,
                SkillDamageCaculate.GetDamageLayer());
            foreach (Collider c in col)
            {
                //和多个不同的敌人目标碰撞
                if (c != lastColobj)
                {
                    lastColobj = c;
                    var hitT = NetworkUtil.GetAttr(c.gameObject);
                    if (hitT != null)
                    {
                        if (hitT.gameObject != this.attacker)
                        {
                            HitSomething(c);
                            break;
                        }
                    }
                    else
                    {
                        HitSomething(c);
                        break;
                    }
                }
            }

        }

        /// <summary>
        ///子弹伤害计算也交给skillLayoutRunner执行 
        /// </summary>
        /// <param name="other">Other.</param>
        private void DoDamage(Collider other)
        {
            var oattr = NetworkUtil.GetAttr(other.gameObject);

            if (oattr != null && SkillLogic.IsEnemyForBullet(attacker, oattr.gameObject) && !missileData.DontHurtObject)
            {
                //MakeSound();
                if (runner != null)
                {
                    runner.DoDamage(oattr.gameObject);
                }
            }
        }


        private void AOEDamage()
        {
            Collider[] col = Physics.OverlapSphere(transform.position, missileData.AOERadius,
                SkillDamageCaculate.GetDamageLayer());
            foreach (Collider c in col)
            {
                DoDamage(c);
            }
        }

        // Update is called once per frame
        private void Update()
        {
            if (isDie)
            {
                return;
            }

            if (missileData.maxTurnRate > 0)
            {
                transform.position += transform.forward*velocity*Time.deltaTime;
                transform.localRotation =
                    Quaternion.Euler(new Vector3(0,
                        transform.localRotation.eulerAngles.y +
                        Time.deltaTime*Random.Range(-missileData.maxTurnRate, missileData.maxTurnRate), 0));
                //根据目标位置进行旋转计算
            }
            if (missileData.GotoPosition)
            {
                transform.position += transform.forward*velocity*Time.deltaTime;
            }
            else
            {
                transform.position += transform.forward*velocity*Time.deltaTime;
            }

            if ((transform.position - initPos).sqrMagnitude >= maxDistance*maxDistance)
            {
                if (missileData.DieParticle != null)
                {
                    Log.AI("bullet die " + gameObject.name);
                    //GameObject g = Instantiate(missileData.DieParticle) as GameObject;
                    var g = ParticlePool.Instance.GetGameObject(missileData.DieParticle, ParticlePool.InitParticle);
                    var removeSelf = NGUITools.AddMissingComponent<DumpMono>(g);
                    removeSelf.StartCoroutine(DestoryBullet(g));
                    g.transform.parent = ObjectManager.objectManager.transform;
                    g.transform.position = transform.position;
                }
                MakeSound();
                CreateCameraShake();

                isDie = true;
                //正常死亡也有AOE效果
                if (missileData.IsAOE)
                {
                    AOEDamage();
                }

                if (missileData.DieWaitTime > 0)
                {
                    Log.Sys("Wait for time bullet to die");
                    GameObject.Destroy(gameObject, missileData.DieWaitTime);
                }
                else
                {
                    GameObject.Destroy(gameObject);
                }


                var evt = new MyEvent(MyEvent.EventType.EventMissileDie);
                evt.missile = transform;
                if (attacker != null)
                {
                    Log.Sys("Push Missile Die Event " + attacker.GetComponent<KBEngine.KBNetworkView>().GetLocalId());
                    MyEventSystem.myEventSystem.PushLocalEvent(
                        attacker.GetComponent<KBEngine.KBNetworkView>().GetLocalId(), evt);
                }
            }
        }

        private void MakeSound()
        {
            if (!string.IsNullOrEmpty(skillData.HitSound))
            {
                BackgroundSound.Instance.PlayEffect(skillData.HitSound, 0.3f);
            }
        }

        private void CreateHitParticle()
        {
            if (missileData.HitParticle != null)
            {
                var g = ParticlePool.Instance.GetGameObject(missileData.HitParticle, ParticlePool.InitParticle);
                //GameObject g = Instantiate(missileData.HitParticle) as GameObject;
                var removeSelf = NGUITools.AddMissingComponent<DumpMono>(g);
                removeSelf.StartCoroutine(DestoryBullet(g));
                g.transform.position = transform.position;
                g.transform.parent = ObjectManager.objectManager.transform;
            }
            MakeSound();
            CreateCameraShake();
        }

        private IEnumerator DestoryBullet(GameObject go)
        {
            yield return new WaitForSeconds(2);
            //GameObject.Destroy(go);
            ParticlePool.Instance.ReturnGameObject(go, ParticlePool.ResetParticle);
        }

        //Remove Self
        private void CreateCameraShake()
        {
            if (attacker != null && attacker.GetComponent<KBNetworkView>().IsMe)
            {
                if (missileData.shakeData != null)
                {
                    var shakeObj = new GameObject("CameraShake");
                    shakeObj.transform.parent = ObjectManager.objectManager.transform;
                    var shake = shakeObj.AddComponent<CameraShake>();
                    shake.shakeData = missileData.shakeData;
                    shake.autoRemove = true;
                }
            }
        }

        /*
         *客户端表现粒子和服务器计算伤害的数值分离开来
         */

        private void HitSomething(Collider other)
        {
            Log.AI("Bullet collider enemy " + other.name + " " + other.tag);
            var otherAttr = NetworkUtil.GetAttr(other.gameObject);
            if (otherAttr != null && otherAttr.gameObject == attacker)
            {
                return;
            }

            if (SkillLogic.IsEnemyForBullet(attacker, other.gameObject))
            {
                var oattr = NetworkUtil.GetAttr(other.gameObject);
                if (oattr != null)
                {
                    var buff = oattr.GetComponent<BuffComponent>();
                    var fanTan = buff.CheckHasSuchBuff(Affix.EffectType.FanTanBullet);
                    Log.Sys("CheckHasSuchBuff: " + fanTan);
                    if (fanTan && fanTanNum == 0)
                    {
                        fanTanNum++;
                        var dirY = transform.localRotation.eulerAngles.y;
                        transform.localRotation = Quaternion.Euler(new Vector3(0, dirY + 180, 0));
                        this.runner.stateMachine.attacker = oattr.gameObject;
                        var l2 = Util.FindChildRecursive(this.transform, "blue");
                        if (l2 != null)
                        {
                            l2.gameObject.SetActive(false);
                            var l3 = Util.FindChildRecursive(this.transform, "red");
                            l3.gameObject.SetActive(true);
                        }
                        return;
                    }
                }

                //攻击多个目标只释放一次 DieParticle
                CreateHitParticle();
                //计算随机的弹射 反射方向
                if (LeftRicochets > 0)
                {
                    Log.AI("Generate new bullet " + LeftRicochets);
                    LeftRicochets--;
                    initPos = transform.position;
                    //子弹应该冲入怪物群中
                    transform.localRotation =
                        Quaternion.Euler(new Vector3(0,
                            transform.localRotation.eulerAngles.y +
                            Random.Range(-missileData.RandomAngle, missileData.RandomAngle), 0));
                }
                else
                {
                    //地震是穿透的因此只会等待粒子自然死亡
                    //非穿透性子弹 
                    if (!missileData.piercing)
                    {
                        GameObject.Destroy(gameObject);
                    }
                }

                //伤害多个目标
                if (missileData.IsAOE)
                {
                    AOEDamage();
                }
                else
                {
                    DoDamage(other);
                }

                //非穿透性子弹
                if (!missileData.piercing)
                {
                    var evt = new MyEvent(MyEvent.EventType.EventMissileDie);
                    evt.missile = transform;
                    MyEventSystem.myEventSystem.PushLocalEvent(
                        attacker.GetComponent<KBEngine.KBNetworkView>().GetLocalId(), evt);
                }
                //TODO::撞击其它玩家如何处理
            }
            else
            {
//装到墙体 或者建筑物 等对象身上 则 反射  Not used
                Log.AI("Bullet colldier with Wall " + other.name);
                CreateHitParticle();
                

                if (LeftRicochets > 0)
                {
                    Log.AI("Generate new bullet " + LeftRicochets);
                    LeftRicochets--;
                    initPos = transform.position;
                    transform.localRotation =
                        Quaternion.Euler(new Vector3(0,
                            transform.localRotation.eulerAngles.y + 180 +
                            Random.Range(-missileData.RandomAngle, missileData.RandomAngle), 0));
                    //sleepTime = IgnoreTime;
                }
                else
                {
                    if (!missileData.piercing)
                    {
                        GameObject.Destroy(gameObject);
                    }
                }
                if (!missileData.piercing)
                {
                    var evt = new MyEvent(MyEvent.EventType.EventMissileDie);
                    evt.missile = transform;
                    if (attacker != null)
                    {
                        MyEventSystem.myEventSystem.PushLocalEvent(
                            attacker.GetComponent<KBEngine.KBNetworkView>().GetLocalId(), evt);
                    }
                }
            }

        }

    }

}
