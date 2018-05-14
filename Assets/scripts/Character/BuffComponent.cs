using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MyLib
{
    public class BuffComponent : MonoBehaviour
    {
        public List<IEffect> effectList = new List<IEffect>();

        void Update()
        {
            foreach (IEffect ef in effectList)
            {
                if (!ef.IsDie)
                {
                    ef.OnUpdate();
                }
            }
            for (int i = 0; i < effectList.Count;)
            {
                var ef = effectList [i];
                if (ef.IsDie)
                {
                    ef.OnDie();
                    effectList.RemoveAt(i);
                    MyEventSystem.myEventSystem.PushLocalEvent(
                    this.gameObject.GetComponent<NpcAttribute>().GetNetView().GetLocalId(), MyEvent.EventType.BuffChange);
                } else
                {
                    i++;
                }
            }
        }

        public bool CheckHasSuchBuff(Affix.EffectType effectType)
        {
            for (int i = 0; i < effectList.Count; i++)
            {
                if (effectList [i].affix.effectType == effectType)
                {
                    return true;
                }
            } 
            return false;
        }

        public void RemoveBuff(Affix.EffectType type)
        {
            foreach (var a in effectList)
            {
                if (a.affix.effectType == type)
                {
                    a.IsDie = true;
                    break;
                }
            }
        }

        public void RemoveBuffId(int id)
        {
            foreach (var effect in effectList)
            {
                if (effect.EffectId == id)
                {
                    effect.IsDie = true;
                    break;
                }
            }
        }

        public bool AddBuff(Affix affix, Vector3 attackerPos = default(Vector3), int attackerId = 0, int buffId = 0)
        {
            if (affix != null)
            {
                Log.Sys("AddBuff is " + gameObject.name + " " + affix.effectType+" buffId "+affix.keepOld+" para "+affix.GetPara(PairEnum.Abs));

                //只保留最旧的Buff
                if (affix.keepOld)
                {
                    for (int i = 0; i < effectList.Count; i++)
                    {
                        if (effectList[i].affix.effectType == affix.effectType)
                        {
                            return false;
                        }
                    }
                }

                //同组不同Buff互斥 超能子弹 和 核弹 同时拾取防御护盾就不能这样处理了不同类型的
                //changDefaultSkill需要立即替换 但是无敌不需要
                if (affix.Group != 0)
                {
                    for (var i = 0; i < effectList.Count; i++)
                    {
                        var ef = effectList[i];
                        if (ef.affix.Group == affix.Group)
                        {
                            ef.IsDie = true;
                        }
                    }
                }



                var eft = BuffManager.buffManager.GetBuffInstance(affix.effectType);
                var buff = (IEffect) Activator.CreateInstance(eft);
                buff.EffectId = buffId;
                buff.Init(affix, gameObject);
                buff.attackerPos = attackerPos;
                buff.attackerId = attackerId;

                if (affix.IsOnlyOne)
                {
                    for (int i = 0; i < effectList.Count; i++)
                    {
                        if (effectList[i].affix.effectType == affix.effectType)
                        {
                            effectList[i].IsDie = true;
                        }
                    }
                }

                effectList.Add(buff);
                buff.OnActive();
                MyEventSystem.myEventSystem.PushLocalEvent(
                    this.gameObject.GetComponent<NpcAttribute>().GetNetView().GetLocalId(), MyEvent.EventType.BuffChange);
                return true;
            }
            return false;
        }

        public void RemoveAllBuff()
        {
            foreach (var effect in effectList)
            {
                effect.IsDie = true;
            }
        }

        public int GetArmor()
        {
            int addArmor = 0;
            foreach (IEffect ef in effectList)
            {
                addArmor += ef.GetArmor();
            }
            return addArmor;
        }

        public float GetSpeedCoff()
        {
            float speedCoff = 1;
            foreach (IEffect ef in effectList)
            {
                speedCoff *= ef.GetSpeedCoff();
            }
            return speedCoff;
        }

        public float GetMoveSpeedCoff()
        {
            float speedCoff = 1;
            foreach (IEffect ef in effectList)
            {
                speedCoff *= ef.GetMoveCoff();
            }
            return speedCoff;
        }

        public float GetTurnSpeed()
        {
            float speedCoff = 1;
            foreach (IEffect ef in effectList)
            {
                speedCoff *= ef.GetTurnSpeed();
            }
            return speedCoff;
        }

        public int GetCriticalRate()
        {
            int rate = 0;
            foreach (IEffect ef in effectList)
            {
                rate += ef.GetCriticalRate();
            }
            return rate;
        }

        public bool CanUseSkill()
        {
            foreach (var ef in effectList)
            {
                if (!ef.CanUseSkill())
                {
                    return false;
                }
            }
            return true;
        }

        public bool GetBuff(Affix.EffectType effectType)
        { 
            foreach (var ef in effectList)
            {
                if (ef.affix.effectType == effectType)
                {
                    return true;
                }
            }
            return false;
        }

        void OnDisable()
        {
            foreach (IEffect ef in effectList)
            {
                ef.OnDie();
            }
        }

        public bool HasLianFa()
        {
            foreach (var effect in effectList)
            {
                if (effect.affix.effectType == Affix.EffectType.LianFaBuff)
                {
                    return true;
                }
            }
            return false;
        }

        public int GetBuffSkill()
        {
            foreach (var effect in effectList)
            {
                var s = effect.GetDefaultSkill();
                if (s != 0)
                {
                    return s;
                }
            }
            return 0;
        }

        public string GetSkillName()
        {
            foreach (var effect in effectList)
            {
                if (!string.IsNullOrEmpty(effect.GetSkillName()))
                {
                    return effect.GetSkillName();
                }
            }
            return null;
        }
    }

}