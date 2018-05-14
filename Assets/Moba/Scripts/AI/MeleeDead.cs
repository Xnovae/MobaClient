using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyLib;

public class MeleeDead : DeadState {
    public override void EnterState()
    {
        base.EnterState();
        aiCharacter.PlayAni("CreepDeath", 1, WrapMode.Once);
    }

}
