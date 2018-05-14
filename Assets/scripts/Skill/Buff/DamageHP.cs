using MyLib;
using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class DamageHP : IEffect
    {
        public override void Init(Affix af, GameObject o)
        {
            base.Init(af, o);
            type = Affix.EffectType.DamageHP;
        }

        private IEnumerator Add()
        {
            var period = System.Convert.ToSingle(this.affix.GetPara(PairEnum.Abs2));
            var num = -System.Convert.ToInt32(this.affix.GetPara(PairEnum.Abs));
            var goneTime = 0.0f;
            var npcAttr = obj.GetComponent<NpcAttribute>();
            while (!IsDie)
            {
                if (goneTime > period)
                {
                    npcAttr.ChangeHP(num);
                    if (this.attackerId != 0)
                    {
                        npcAttr.GetComponent<MyAnimationEvent>().lastAttacker = this.attackerId;
                    }
                    goneTime -= period;
                }
                goneTime += Time.deltaTime;
                yield return null;
            }
        }

        public override void OnActive()
        {
            base.OnActive();
            var buff = obj.GetComponent<BuffComponent>();
            buff.StartCoroutine(Add());
        }
    }
}