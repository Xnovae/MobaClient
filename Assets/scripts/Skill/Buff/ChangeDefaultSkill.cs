using System;
using MyLib;
using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class ChangeDefaultSkill : IEffect
    {
        public override void Init(Affix af, GameObject o)
        {
            base.Init(af, o);
            type = Affix.EffectType.ChangeDefaultSkill;
        }

        public override void OnActive()
        {
            base.OnActive();
            obj.GetComponent<MyAnimationEvent>().AddCallBackLocalEvent(MyEvent.EventType.UseSkill, HitTarget);
        }

        private void HitTarget(MyEvent evt)
        {
            IsDie = true;
            var word = this.affix.GetPara(PairEnum.Word);
            if (word != null)
            {
                WindowMng.windowMng.ShowNotifyLog(word, 4, null, true);
            }
        }

        public override void OnDie()
        {
            base.OnDie();
            obj.GetComponent<MyAnimationEvent>().DropCallBackLocalEvent(MyEvent.EventType.UseSkill, HitTarget);
        }

        public override int GetDefaultSkill()
        {
            return Convert.ToInt32(this.affix.GetPara(PairEnum.Abs));
        }

        public override string GetSkillName()
        {
            var skId = this.GetDefaultSkill();
            var skData = Util.GetSkillData(skId, 1);
            return skData.SkillName;
        }
    }

}