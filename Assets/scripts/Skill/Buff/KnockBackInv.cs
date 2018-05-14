using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class KnockBackInv : IEffect
    {
        public override void Init(Affix af, GameObject o)
        {
            base.Init(af, o);
            type = Affix.EffectType.KnockBackInv;
        }

        public override void OnActive()
        {
            Log.AI("KnockBack Buff Active");
            obj.GetComponent<MyAnimationEvent>().KnockBackWho(attackerPos, true);
        }
    }
}
