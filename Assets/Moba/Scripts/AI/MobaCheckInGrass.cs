using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyLib {
    public class MobaCheckInGrass : MonoBehaviour {

        private MobaModelLoader modelLoader;
        void Start()
        {
            modelLoader = GetComponent<MobaModelLoader>();
        }

        void Update() {
            if(GridManager.Instance != null)
            {
                var inst = GridManager.Instance;
                var inGrass = inst.CheckInGrass(transform.position);
                modelLoader.SetInGrass(inGrass);
            }
        }
    }
}
