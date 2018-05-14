using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class NuclearBuff :IEffect 
    {
        public override void Init(Affix af, GameObject o)
        {
            base.Init(af, o);
            type = Affix.EffectType.NuclearBuff;
        }

        public override void OnActive()
        {
            base.OnActive();
            obj.GetComponent<MyAnimationEvent>().AddCallBackLocalEvent(MyEvent.EventType.UseSkill, HitTarget);
        }

        void HitTarget(MyEvent evt) {
            IsDie = true;
            WindowMng.windowMng.ShowNotifyLog("核弹已经释放，请立即逃离该区域", 4, null, true);
        }

        public override void OnDie()
        {
            base.OnDie();
            obj.GetComponent<MyAnimationEvent>().DropCallBackLocalEvent(MyEvent.EventType.UseSkill, HitTarget);
        }

        public override int GetDefaultSkill()
        {
            return (int)SkillData.SkillConstId.Nuclear;
        }

        public override string GetSkillName()
        {
            return "核弹拥有者";
        }
    }
}