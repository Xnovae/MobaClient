using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyLib;

public class XiaoGuaiDeath : DeadState
{
    public override void EnterState()
    {
        base.EnterState();
        aiCharacter.PlayAni("die01", 1, WrapMode.Once);
    }
}
