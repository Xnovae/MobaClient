using System.Linq;
using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif
[System.Serializable]
public class TypeParts
{
    public string type;
    public List<GameObject> parts = new List<GameObject>();
}

public class RoomList : MonoBehaviour
{
	public List <TypeParts> typeParts = new List<TypeParts>();

    static Dictionary<string, Dictionary<string, GameObject>> nameToObj;
    static RoomList Instance;

    /// <summary>
    /// 初始化根据名字 类型 映射到GameObject 
    /// </summary>
    void InitNameToObj()
    {
        var elements = new HashSet<string>()
        {
            "ENTRANCE",
            "EXIT",
            "EW",
            "S",
            "NE",
            "NS",
            "NW",
            "PB",
            "LM",
            "W",
            "E",
            "KG",
            "SW",
            "SE",
            "N",
            "TOWN",
            "JD",
            "CITY",
        };

        nameToObj = new Dictionary<string, Dictionary<string, GameObject>>();
        foreach (var t in typeParts)
        {
            var dic = nameToObj [t.type] = new Dictionary<string, GameObject>();
            foreach (var part in t.parts)
            {
                var ele = part.name.ToUpper().Split(char.Parse("_"));
                var eleStr = "";
                bool isFirst = true;
                foreach (var e in ele)
                {
                    if (elements.Contains(e))
                    {
                        if (isFirst)
                        {
                            isFirst = false;
                            eleStr += e;
                        } else
                        {
                            eleStr += "_" + e;
                        }
                    }
                }
                dic [eleStr] = part;
            }
        }
    }
    public static GameObject GetStaticObj(string type, string name) {
        if(Instance == null) {
            var g= Resources.Load<GameObject>("RoomList");
            Instance = g.GetComponent<RoomList>();
        }    
        return Instance.GetObj(type, name);
    }

    GameObject GetObj(string type, string name)
    {
        if(nameToObj == null) {
            InitNameToObj();
        }
        Log.Sys("GetPiece "+type+" name "+name);
        return nameToObj[type][name];
    }


#if UNITY_EDITOR
    [ButtonCallFunc()] public bool LoadAllRooms;

    public void LoadAllRoomsMethod()
    {
        //roomPieces.Clear();
        typeParts.Clear();
        var pth = Path.Combine(Application.dataPath, "Resources/room");
        var prefabList = new DirectoryInfo(pth);
        var dirInfo = prefabList.GetDirectories();
        Debug.Log("PrefabList: "+prefabList);
        Debug.Log("Path: "+pth);

        FileInfo[] fileInfo = prefabList.GetFiles("*.prefab", SearchOption.TopDirectoryOnly);
        var tp1 = Contains("mine");
        foreach (var p in fileInfo)
        {
            Debug.Log("pName: "+p.FullName);
            var n = p.FullName.Replace("\\", "/").Replace(Application.dataPath, "Assets");
			Debug.Log("AddRoom "+n);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(n);
            tp1.parts.Add(go);
        }
        Debug.Log("dirInfo: "+dirInfo.Count());
        foreach (var dir in dirInfo)
        {
            var finfo = dir.GetFiles("*.prefab", SearchOption.TopDirectoryOnly);
            var tp = Contains(dir.Name);
			Debug.Log("AddType "+tp.type);
            foreach (var p in finfo)
            {
                var n = p.FullName.Replace("\\", "/").Replace(Application.dataPath, "Assets");
				Debug.Log("AddRoom "+n);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(n);
                tp.parts.Add(go);
                
            }
        }
		Debug.Log("RoomListLength "+typeParts.Count);
        //EditorUtility.SetDirty(gameObject);
        EditorUtility.SetDirty(this);
        AssetDatabase.Refresh();

    }
#endif

    TypeParts  Contains(string type)
    {
        foreach (var t in typeParts)
        {
            if (t.type == type)
            {
                return t;
            }
        }

        var tp = new TypeParts();
        tp.type = type;
        typeParts.Add(tp);

        return tp;
    }

    public void AddRoom(string type, GameObject g)
    {
        //roomPieces.Add(g);
        var roomPieces = Contains(type);
        roomPieces.parts.Add(g);
    }
	
}
