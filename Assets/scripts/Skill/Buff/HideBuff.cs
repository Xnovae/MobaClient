using UnityEngine;
using System.Collections;

namespace MyLib
{
    /// <summary>
    /// 给自己加粒子效果给别人就是直接隐藏不显示Render即可
    /// 或者自己也隐藏？
    /// 但是换Job就麻烦了
    /// </summary>
    public class HideBuff : IEffect
    {
        private NpcAttribute attr;
        public override void Init(Affix af, GameObject o)
        {
            base.Init(af, o);
            type = Affix.EffectType.HideBuff;
            attr = obj.GetComponent<NpcAttribute>();
            attr.HideMe();
            obj.GetComponent<MyAnimationEvent>().AddCallBackLocalEvent(MyEvent.EventType.UseSkill, Over);
            obj.GetComponent<MyAnimationEvent>().AddCallBackLocalEvent(MyEvent.EventType.BeHit, Over);
        }

        void Over(MyEvent evt)
        {
            this.IsDie = true;
            /*
            if (attr.GetNetView().IsMe)
            {
                //NetDateInterface.FastRemoveBuff((int)type, attr.gameObject);
            }
            */
        }

        public override void OnDie()
        {
            obj.GetComponent<MyAnimationEvent>().DropCallBackLocalEvent(MyEvent.EventType.UseSkill, Over);
            obj.GetComponent<MyAnimationEvent>().DropCallBackLocalEvent(MyEvent.EventType.BeHit, Over);
            base.OnDie();
            var attr = obj.GetComponent<NpcAttribute>();
            attr.ShowMe();
        }
    }

}


