using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MyLib
{
    public class CharCreate : IUserInterface
    {
        //GameObject selChar;
        void Awake()
        {
            regEvt = new List<MyEvent.EventType> () {
                MyEvent.EventType.UpdateCharacterCreateUI,
                MyEvent.EventType.CreateSuccess,
            };
            RegEvent ();
            SetCallback("createChar", OnCreate);
        }

        void OnCreate(GameObject g){
            var name = GetInput ("NameInput").value;
            Log.GUI ("Create Char is "+ name +" ");
            if(string.IsNullOrEmpty(name) || name.Length > 20){
                WindowMng.windowMng.ShowNotifyLog("名字不能为空！且名字长度不能大于20!", 2);
                return;
            }
            CharSelectProgress.charSelectLogic.CreateChar (name, 1);
        }

        protected override void OnEvent (MyEvent evt)
        {
            if (evt.type == MyEvent.EventType.UpdateCharacterCreateUI) {
                UpdateFrame ();
            } else if(evt.type == MyEvent.EventType.CreateSuccess) {
                //Hide(null);
                WindowMng.windowMng.ReplaceView ("UI/CharSelect2", false);
                MyEventSystem.myEventSystem.PushEvent(MyEvent.EventType.UpdateSelectChar);
            }
        }
        void UpdateFrame(){
            FakeObjSystem.fakeObjSystem.OnUIShown (-1, null, 1);
        }
    }
}
