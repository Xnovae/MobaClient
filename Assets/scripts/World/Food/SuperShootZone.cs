using UnityEngine;
using System.Collections;

/// <summary>
/// 生成对象区域
/// 所有怪物生成区域都需要继承SpawnZone
/// </summary>
public class SpawnZone : MonoBehaviour
{
    //[HideInInspector]
    public int spawnId = 0; //由孩子初始化
    public virtual void Awake()
    {
        //gameObject.SetActive(false);
        foreach(Transform t in transform)
        {
            t.gameObject.SetActive(false);
        }
    }
}
public class SuperShootZone : SpawnZone
{
    public float TimeToSpawn = 30;
    public int itemId;
    public string warnString = "超能弹药已经产生，快去争夺";
}
