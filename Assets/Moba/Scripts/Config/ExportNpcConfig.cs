#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using MyLib;
using SimpleJSON;

public class ExportNpcConfig : MonoBehaviour {

    public GameObject allNpcConfig;
    [ButtonCallFunc()]
    public bool ExportLocal;
    /// <summary>
    /// 导出GameObjectPrefab 到本地
    /// </summary>
    public void ExportLocalMethod()
    {
        var path = AssetDatabase.GetAssetPath(allNpcConfig);
        var dirInfo = new DirectoryInfo(Path.GetDirectoryName(path));
        var allNpc = dirInfo.GetFiles("*.prefab", SearchOption.AllDirectories);
        var go = new GameObject("AllSoldier");
        foreach (var n in allNpc)
        {
            var p = Util.FullPathToUnityPath(n.FullName);
            Debug.LogError(n.FullName + ":" + p);
            var npc = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            var nn = GameObject.Instantiate<GameObject>(npc);
            nn.transform.parent = go.transform;
            Util.InitGameObject(nn);
        }

        var path2 = "Assets/Resources/AllSoldier.prefab";
        var old = AssetDatabase.LoadAssetAtPath<GameObject>(path2);
        if (old != null)
        {
            PrefabUtility.ReplacePrefab(go, old);
        } else {
            PrefabUtility.CreatePrefab(path2, go);
        }
        GameObject.DestroyImmediate(go);
    }

    [ButtonCallFunc()]
    public bool Export;
    public void ExportMethod()
    {
        var path = AssetDatabase.GetAssetPath(allNpcConfig);
        var dirInfo = new DirectoryInfo(Path.GetDirectoryName(path));
        var allNpc = dirInfo.GetFiles("*.prefab", SearchOption.AllDirectories);
        //var allStr = new JSONArray();
        var go = new GameObject("AllSoldier");
        //var lists = go.AddComponent<ListConfig>();
        //lists.lists = new List<GameObject>();
        foreach(var n in allNpc)
        {
            var p = Util.FullPathToUnityPath(n.FullName);
            Debug.LogError(n.FullName+":"+p);
            var npc = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            //lists.lists.Add(npc);
            var nn = GameObject.Instantiate<GameObject>(npc);
            nn.transform.parent = go.transform;
            Util.InitGameObject(nn);
            //Debug.LogError(jobj.ToString());
            //allStr.Add(jobj);
        }
        var jobj = EntityConfigExport.ExportGo(go);


        var etyPath = Path.Combine(Application.dataPath, "../../tankServer/SocketServer/ConfigData/");
        var file = Path.Combine(etyPath, go.name + ".json");
        if (File.Exists(file))
        {
            File.Delete(file);
        }
        var s = jobj.ToString();

        File.WriteAllText(file, s);
        Debug.LogError("导出配置: " + file);


        GameObject.DestroyImmediate(go);
    }
}

#endif