using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyLib
{
    public class MobaMeStop : StopState 
    {
        public override bool CheckNextState(AIStateEnum next)
        {
            return false;
        }

    }
}
