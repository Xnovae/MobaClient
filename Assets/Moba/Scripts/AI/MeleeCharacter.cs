using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyLib;

public class MeleeCharacter : AICharacter
{
    private Dictionary<string, float> aniLength;
    private Animator animator;
    private Animation animation;
    protected override void Init()
    {
        base.Init();
        aniLength = new Dictionary<string, float>();
        var ani = GetAttr().GetComponent<Animator>();
        animator = ani;
        if (animator != null)
        {
            var clips = ani.runtimeAnimatorController.animationClips;
            foreach (var c in clips)
            {
                aniLength[c.name] = c.length;
            }
        }else
        {
            animation = GetAttr().GetComponent<Animation>();
            if(animation != null)
            {
                foreach(AnimationClip c in animation)
                {
                    aniLength[c.name] = c.length;
                }
            }

        }
    }

    public override void SetIdle()
    {
        PlayAni("Idle", 1, WrapMode.Loop);
    }
    public override void SetRun()
    {
        PlayAni("Walk", 1, WrapMode.Loop);
    }

    public override void PlayAni(string name, float speed, WrapMode wm)
    {
        if (animator != null)
        {
            var ani = GetAttr().GetComponent<Animator>();
            ani.speed = 1;
            ani.CrossFade(name, 0.1f);
        }
        else
        {

        }

    }
    public override void PlayAniInTime(string name, float time)
    {
        var ani = aniLength[name];
        var rate = ani / time;
        animator.speed = rate;
        animator.CrossFade(name, 0.1f);
    }


}
