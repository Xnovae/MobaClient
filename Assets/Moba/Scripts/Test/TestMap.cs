using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class TestMap : MonoBehaviour {
    private void Awake()
    {
        
    }
    private void Update()
    {
        
    }
    public TextAsset textFile;

    [ButtonCallFunc()]
    public bool Load;
    public void LoadMethod()
    {
        //grid = new GridManager();
        grid = gameObject.AddMissingComponent<GridManager>();
        grid.LoadMap(textFile.text);
    }
    private GridManager grid;


    public GameObject p;
    [ButtonCallFunc()]
    public bool GetPos;
    public void GetPosMethod()
    {
        LoadMethod();
        var gid = grid.mapPosToGrid(p.transform.position);
        Debug.LogError(gid);
    }

    public GameObject p1;
    public GameObject p2;
    [ButtonCallFunc()]
    public bool GetPath;
    public void GetPathMethod()
    {
        LoadMethod();
        var g1 = grid.mapPosToGrid(p1.transform.position);
        var g2 = grid.mapPosToGrid(p2.transform.position);
        var path = grid.FindPath(g1, g2);

        nodes = new List<Vector3>(path.Count);
        foreach(var p in path)
        {
            var wp = grid.gridToMapPos(new Vector2(p.x, p.y));
            wp.y += 0.2f;
            nodes.Add(wp);
        }
    }

    public List<Vector3> nodes;

    private void OnDrawGizmos()
    {
        if(nodes != null)
        {
            Gizmos.color = Color.red;
            //Gizmos.DrawSphere(transform.position, 5);
            //iTween.DrawPath(nodes.ToArray(), Color.red);
            for (var i = 0; i < nodes.Count-1; i++)
            {
                var p1 = nodes[i];
                var p2 = nodes[i + 1];
                Gizmos.DrawLine(p1, p2);
            }
        }
    }

    public GameObject wayPoint;
    public TextAsset exportPath;

    [ButtonCallFunc()]
    public bool SaveWayPoint;
    /// <summary>
    /// 保存到JSON文件
    /// </summary>
    public void SaveWayPointMethod()
    {
        var pathes = wayPoint.GetComponentsInChildren<iTweenPath>();
        var result = new SimpleJSON.JSONClass();
        foreach (var p in pathes)
        {
            var obj = new JSONClass();
            obj["name"] = p.name;
            var jarr = new JSONArray();
            obj["nodes"] = jarr;
            var nodes = p.nodes;
            foreach (var n in nodes)
            {
                var varr = new JSONArray();
            }
        }
    }
}
