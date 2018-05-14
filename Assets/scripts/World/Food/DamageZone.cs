using UnityEngine;
using System.Collections;

public class DamageZone : SpawnZone
{
    public int SpawnId = 0;
    //剩余多长时间伤害开始生效
    public float LeftTimeToSpawn = 290;
    public int MonsterID; //怪物ID

    public override void Awake()
    {
        //base.Awake();
        //不用隐藏自己
        this.spawnId = SpawnId;
    }
}
