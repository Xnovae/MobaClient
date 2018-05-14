using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class FanTanBullet : IEffect 
    {
        private int localId;
        public override void Init(Affix af, GameObject o)
        {
            base.Init(af, o);
            type = Affix.EffectType.FanTanBullet;
            localId = obj.GetComponent<NpcAttribute>().GetLocalId();
            MyEventSystem.myEventSystem.RegisterLocalEvent(localId, MyEvent.EventType.OnBullet, OnEvent);
        }
        
        void OnEvent(MyEvent evt)
        {
                
        }

        public override void OnDie()
        {
            MyEventSystem.myEventSystem.DropLocalListener(localId, MyEvent.EventType.OnBullet, OnEvent);
            base.OnDie();
        }
    }
}