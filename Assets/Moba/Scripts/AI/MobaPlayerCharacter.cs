using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MyLib
{
    public class MobaPlayerCharacter : AICharacter
    {
        private Animation ani;
        protected override void Init()
        {
            base.Init();
            //ani = attribute.GetComponent<Animation>();
            GetAttr().gameObject.AddMissingComponent<EvtHandler>().AddLocalEvent(MyEvent.EventType.UpdateModel, OnEvent);
        }
        void OnEvent(MyEvent evt)
        {
            if(evt.type == MyEvent.EventType.UpdateModel)
            {
                ani = GetAttr().GetComponentInChildren<Animation>();
                ChangeStateIgnoreCurState(AIStateEnum.IDLE);
            }
        }

        public override void SetIdle()
        {
            SetAni("Idle", 1, WrapMode.Loop);
        }
        public override void SetRun()
        {
            SetAni("Run", 1, WrapMode.Loop);
        }
        public override void SetAni(string name, float speed, WrapMode wm)
        {
            if (ani != null)
            {
                //Debug.LogError("SetAni:"+name+":"+speed+":"+wm);
                ani[name].speed = speed;
                ani[name].wrapMode = wm;
                ani.CrossFade(name);
            }
        }
        public override void PlayAniInTime(string name, float time)
        {
            var clip = ani[name];
            var speed = clip.length/ (time * 0.8f);
            //Debug.LogError("PlayAniInTime:"+name+":"+speed+":"+time+":"+clip.length);
            SetAni(name, speed, WrapMode.ClampForever);
        }
        public override void PlayAni(string name, float speed, WrapMode wm)
        {
            SetAni(name, speed, wm);
        }
    }
}