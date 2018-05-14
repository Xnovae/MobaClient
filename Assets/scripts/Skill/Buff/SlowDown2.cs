using System;
using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class SlowDown2 : IEffect
    {
        public override void Init(Affix af, GameObject o)
        {
            base.Init(af, o);
            type = Affix.EffectType.SlowDown2;
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
