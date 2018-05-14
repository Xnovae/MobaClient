using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class MonsterSyncToServer : MonoBehaviour
    {
        EntityInfo lastInfo;
        //EntityInfo.Builder info;
        private void Awake()
        {
            lastInfo = EntityInfo.CreateBuilder().Build();
            //info = EntityInfo.CreateBuilder().Build();
        }
    }
}