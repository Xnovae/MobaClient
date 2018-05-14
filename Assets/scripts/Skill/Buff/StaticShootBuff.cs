using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class StaticShootBuff : IEffect
    {

        public override void Init(Affix af, GameObject o)
        {
            base.Init(af, o);
            type = Affix.EffectType.StaticShootBuff;
        }
    }
}