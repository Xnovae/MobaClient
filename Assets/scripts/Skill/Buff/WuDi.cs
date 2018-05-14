using System;
using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class WuDi : IEffect
    {
        public override void Init(Affix af, GameObject o)
        {
            base.Init(af, o);
            this.type = Affix.EffectType.WuDi;
        }

        public override void OnActive()
        {
            base.OnActive();
            obj.GetComponent<MyAnimationEvent>().AddCallBackLocalEvent(MyEvent.EventType.OnHit, BeHit);
        }

        //无敌护盾效果以及记得删除Buff服务器 自己控制
        private int count = 0;
        void BeHit(MyEvent evt)
        {
            count++;
            var num = Convert.ToInt32(this.affix.GetPara(PairEnum.Abs));
            if (count >= num)
            {
                this.GetBuffCom().StartCoroutine(WaitDie());
            }
        }

        private IEnumerator WaitDie()
        {
            yield return null;
            this.IsDie = true;
        }

        public override int GetArmor ()
		{
			return 99999;
		}

        public override void OnDie()
        {
            base.OnDie();
            obj.GetComponent<MyAnimationEvent>().DropCallBackLocalEvent(MyEvent.EventType.OnHit, BeHit);
        }
    }
}
