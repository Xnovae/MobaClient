using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class LianFaBuff : IEffect 
    {
        public override void Init(Affix af, GameObject o)
        {
            base.Init(af, o);
            type = Affix.EffectType.LianFaBuff;
        }
       
    }
}