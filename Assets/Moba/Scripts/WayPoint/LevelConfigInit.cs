using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 初始化地图物理
/// </summary>
public class LevelConfigInit : MonoBehaviour {
    public static LevelConfigInit Instance;

    private GridManager gridManager;

    private void Awake()
    {
        Instance = this;
        gridManager = gameObject.AddComponent<GridManager>();
        var mapSource = Resources.Load<TextAsset>("Config/MapSourceConfig");
        gridManager.LoadMap(mapSource.text);
    }
}
