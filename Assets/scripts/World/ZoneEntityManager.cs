using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MyLib
{
    public class ZoneEntityManager : MonoBehaviour
    {
        private Transform  allPlayerStart;
        private SpawnChest[] allChest;
        private SpawnZone[] spawnZone;

        /*
        public GameObject properties{
            private set;
            get;
        }
        */
        void Awake()
        {
            //properties = Util.FindChildRecursive(transform, "properties").gameObject;
            allChest = gameObject.GetComponentsInChildren<SpawnChest>(true);
            spawnZone = gameObject.GetComponentsInChildren<SpawnZone>(true);

            SpawnChest.MaxSpawnId = 0;
            foreach(var s in allChest) {
                s.InitSpawnId();
            }

            allPlayerStart = transform.Find("AllPlayerStart");
        }

        public Vector3 GetRandomStartPos(int id) {
            //id = Mathf.Max(0, id);
            var count = allPlayerStart.childCount;
            //var n = id%count;
            var n = Random.Range(0, count);
            var cd = allPlayerStart.GetChild(n);
            Log.Sys("GetRandomStartPos: "+id+" cd "+cd.transform.position);
            return cd.transform.position;
        }

        
        public void EnableProperties()
        {
            /*
            properties.gameObject.SetActive(false);
            properties.gameObject.SetActive(true);
            */
        }

        public void DisableProperties()
        {
            //properties.SetActive(false);
        }

        public SpawnChest GetSpawnChest(int spawnId) {
            foreach(var s in allChest) {
                if(s.SpawnId == spawnId) {
                    return s;
                }
            }
            return null;
        }

        public SpawnZone GetSpawnZone(int spawnId)
        {
            foreach (var sz in spawnZone      )
            {
                if (sz.spawnId == spawnId)
                {
                    return sz;
                }
            }
            return null;
        }
    }
}
