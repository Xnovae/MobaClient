using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class SuperShootBuff : IEffect
    {
        public override void Init(Affix af, GameObject o)
        {
            base.Init(af, o);
            type = Affix.EffectType.SuperShootBuff;
        }

        public override void OnActive()
        {
            base.OnActive();
            obj.GetComponent<MyAnimationEvent>().AddCallBackLocalEvent(MyEvent.EventType.UseSkill, HitTarget);
        }

        void HitTarget(MyEvent evt) {
            IsDie = true;
        }

        public override void OnDie()
        {
            base.OnDie();
            obj.GetComponent<MyAnimationEvent>().DropCallBackLocalEvent(MyEvent.EventType.UseSkill, HitTarget);
        }

        public override int GetDefaultSkill()
        {
            return (int)SkillData.SkillConstId.SuperShoot;
        }

        public override string GetSkillName()
        {
            return "超能子弹";
        }
    }

}