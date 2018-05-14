using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class TankNum : IUserInterface
    {
        private UIInput input;
        public TankNum Instance;
        private void Awake()
        {
            Instance = this;
            input = GetInput("numInput");
            SetCallback("createChar", OnMatch);
            this.regEvt = new List<MyEvent.EventType>()
            {
                MyEvent.EventType.RemoteReConnect,
            };
            SetCallback("Quit", OnQuit);
        }

        void OnQuit()
        {
            WorldManager.ReturnCity();
        }

        protected override void OnEvent(MyEvent evt)
        {
            if (evt.type == MyEvent.EventType.RemoteReConnect)
            {
                pressYet = false;
                if (NetDebug.netDebug.JumpLogin)
                {
                    OnMatch();
                }
            }
        }

        public bool pressYet = false;
        void OnMatch()
        {
            Log.GUI("OnMatchButton");
            if (pressYet)
            {
                Util.ShowMsg("正在匹配耐心等候");
            }
            else
            {
                pressYet = true;
                var iv = Convert.ToInt32(input.value);
                NetMatchScene.Instance.MatchNum = iv;
                NetMatchScene.Instance.SetPress();
            }
        }

        // Use this for initialization
        private void Start()
        {
            //if (NetDebug.netDebug.JumpLogin)
            {
                OnMatch();                
            }
        }
    }
}