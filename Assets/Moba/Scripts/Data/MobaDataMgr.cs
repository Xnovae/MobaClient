using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobaDataMgr : MonoBehaviour {

    public static MobaDataMgr Instance;
    public static void Init()
    {
        if(Instance != null)
        {
            GameObject.Destroy(Instance.gameObject);
        }
        var go = new GameObject("MobaDataMgr");
        go.AddMissingComponent<MobaDataMgr>();
    }
    void Awake()
    {
        Instance = this;
        GameObject.DontDestroyOnLoad(gameObject);
    }

    public List<int> ownItems = new List<int>();
}
