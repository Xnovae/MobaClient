using System;
using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class SlowDown :IEffect 
    {
        public override void Init (Affix af, GameObject o)
        {
            base.Init (af, o);
            type = Affix.EffectType.SlowDown;
        }

        public override float GetMoveCoff()
        {
            return Convert.ToSingle(this.affix.GetPara(PairEnum.Abs));
        }

        public override float GetTurnSpeed()
        {
            return Convert.ToSingle(this.affix.GetPara(PairEnum.Abs2));
        }
    }
}
