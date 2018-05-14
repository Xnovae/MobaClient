using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KBEngine
{
    public class KBMonoBehaviour : UnityEngine.MonoBehaviour
    {
        protected List<MyLib.MyEvent.EventType> regEvt = null;
        protected List<MyLib.MyEvent.EventType> regLocalEvt = null;
        protected List<EvtCbPair> regLocalEvtCallback = null;

        protected bool regYet = false;

        /// <summary>
        /// BloodBar 继承UIInterface
        /// UIInterface在Awake时添加事件 Enable时注册事件 Disable时取消事件
        ///  
        /// BloodBar需要在Start时动态加入Local事件
        /// </summary>
        /// <param name="force">If set to <c>true</c> force.</param>
        public void RegEvent(bool force = false)
        {
            if (regYet && !force)
            {
                return;
            }
            regYet = true;
            if (regEvt != null)
            {
                foreach (MyLib.MyEvent.EventType t in regEvt)
                {
                    MyLib.MyEventSystem.myEventSystem.RegisterEvent(t, OnEvent);
                }
            }

            if (regLocalEvt != null)
            {
                foreach (MyLib.MyEvent.EventType t in regLocalEvt)
                {
                    Log.Sys("Reglocalevent " + t + " view " + photonView + " myevent " + MyLib.MyEventSystem.myEventSystem);
                    MyLib.MyEventSystem.myEventSystem.RegisterLocalEvent(photonView.GetLocalId(), t, OnLocalEvent);
                }
            }

        }

        protected virtual void OnLocalEvent(MyLib.MyEvent evt)
        {

        }

        public void DropEvent()
        {
            if (!regYet)
            {
                return;
            }
            regYet = false;
            if (regEvt != null)
            {
                foreach (MyLib.MyEvent.EventType t in regEvt)
                {
                    MyLib.MyEventSystem.myEventSystem.dropListener(t, OnEvent);
                }
            }

            if (regLocalEvt != null)
            {
                foreach (MyLib.MyEvent.EventType t in regLocalEvt)
                {
                    MyLib.MyEventSystem.myEventSystem.DropLocalListener(photonView.GetLocalId(), t, OnLocalEvent);
                }
            }
            if (regLocalEvtCallback != null)
            {
                foreach (var t in regLocalEvtCallback)
                {
                    MyLib.MyEventSystem.myEventSystem.DropLocalListener(photonView.GetLocalId(), t.t, t.cb);
                }
            }
        }

        protected virtual void OnEvent(MyLib.MyEvent evt)
        {
        }

        protected virtual void OnDestroy()
        {
            DropEvent();
        }

        public KBNetworkView photonView
        {
            get
            {
                return GetComponent<KBNetworkView>();
            }
        }

        /// <summary>
        /// 注册一个全局事件 
        /// 在RegEvent之后添加事件
        /// </summary>
        /// <param name="t">T.</param>
        protected void AddEvent(MyLib.MyEvent.EventType t)
        {
            regEvt.Add(t);
            MyLib.MyEventSystem.myEventSystem.RegisterEvent(t, OnEvent);
        }

        public class EvtCbPair
        {
            public MyLib.MyEvent.EventType t;
            public MyLib.EventDel cb;
        }

        public void AddCallBackLocalEvent(MyLib.MyEvent.EventType t, MyLib.EventDel cb)
        {
            regYet = true;
            if (regLocalEvtCallback == null)
            {
                regLocalEvtCallback = new List<EvtCbPair>();
            }

            regLocalEvtCallback.Add(new EvtCbPair()
            {
                t = t,
                cb = cb,
            });
            MyLib.MyEventSystem.myEventSystem.RegisterLocalEvent(photonView.GetLocalId(), t, cb);
        }

        public void DropCallBackLocalEvent(MyLib.MyEvent.EventType t, MyLib.EventDel cb)
        {
            MyLib.MyEventSystem.myEventSystem.DropLocalListener(photonView.GetLocalId(), t, cb);
            foreach (var e in regLocalEvtCallback)
            {
                if (e.t == t && e.cb == cb)
                {
                    regLocalEvtCallback.Remove(e);
                    break;
                }
            }
        }
    }
}