using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MyLib
{
    public class MobaMeDead : DeadState
    {
        public override void EnterState()
        {
            base.EnterState();
            aiCharacter.PlayAni("Dead", 1, WrapMode.Once);
        }
        public override bool CheckNextState(AIStateEnum next)
        {
            return false;
        }
    }
}