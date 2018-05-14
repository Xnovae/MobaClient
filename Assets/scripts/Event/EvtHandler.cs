using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MyLib
{
    public class EvtHandler : MonoBehaviour
    {
        public class LocalReg
        {
            public int localId;
            public EventDel cb;
        }
        private Dictionary<MyEvent.EventType, EventDel> regEvts = new Dictionary<MyEvent.EventType, EventDel>();
        private Dictionary<MyEvent.EventType, LocalReg> regLoalEvtEvts = new Dictionary<MyEvent.EventType, LocalReg>();

        Dictionary<MyEvent.EventType, MyEvent> happenedEvt = new Dictionary<MyEvent.EventType, MyEvent>();

        public void AddEvent(MyEvent.EventType type, EventDel cb)
        {
            if(cb != null) {
                regEvts.Add(type, cb);
                MyEventSystem.myEventSystem.RegisterEvent(type, cb);
            }else {
                var del = new EventDel(delegate(MyEvent evt){
                    if(!happenedEvt.ContainsKey(type)) {
                        happenedEvt.Add(type, evt);
                    }
                });
                MyEventSystem.myEventSystem.RegisterEvent(type, del);
            }
        }

        public void AddLocalEvent(MyEvent.EventType type, EventDel cb)
        {
            var localId = GetComponent<NpcAttribute>().GetLocalId();
            regLoalEvtEvts.Add(type, new LocalReg()
            {
                localId = localId,
                cb = cb,
            });
            MyEventSystem.myEventSystem.RegisterLocalEvent(localId, type, cb);
        }

        public void AddLocalEvent(int localId, MyEvent.EventType type, EventDel cb)
        {
            regLoalEvtEvts.Add(type, new LocalReg()
            {
                localId = localId,
                cb = cb,
            });
            MyEventSystem.myEventSystem.RegisterLocalEvent(localId, type, cb);
        }

        public IEnumerator WaitEvt(MyEvent.EventType type)
        {
            while(!happenedEvt.ContainsKey(type)){
                yield return null;
            }
            happenedEvt.Remove(type);
        }

        void OnDestroy() {
            foreach(var e in regEvts) {
                MyEventSystem.myEventSystem.dropListener(e.Key, e.Value);
            }
            regEvts.Clear();
            foreach (var regLoalEvtEvt in regLoalEvtEvts)
            {
                MyEventSystem.myEventSystem.DropLocalListener(regLoalEvtEvt.Value.localId, regLoalEvtEvt.Key, regLoalEvtEvt.Value.cb);
            }
            regLoalEvtEvts.Clear();
        }
    }
}
