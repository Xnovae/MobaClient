#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyLib;
using System.IO;
using UnityEditor;
using SimpleJSON;

public class ExportSkillConfig : MonoBehaviour {
    public GameObject skill;
    [ButtonCallFunc()]
    public bool Export;
    public void ExportMethod()
    {
        var path = AssetDatabase.GetAssetPath(skill);
        var dirInfo = new DirectoryInfo(Path.GetDirectoryName(path));
        var allNpc = dirInfo.GetFiles("*.prefab", SearchOption.AllDirectories);
        var jDict = new JSONClass();

        foreach (var n in allNpc)
        {
            var p = Util.FullPathToUnityPath(n.FullName);
            Debug.LogError(n.FullName + ":" + p);
            var sk = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            var jobj = EntityConfigExport.ExportGo(sk);
            jDict.Add(sk.name, jobj);
        }


        var etyPath = Path.Combine(Application.dataPath, "../../tankServer/SocketServer/ConfigData/AllSkill.json");
        //var file = Path.Combine(etyPath, skill.name + ".json");
        if (File.Exists(etyPath))
        {
            File.Delete(etyPath);
        }
        var s = jDict.ToString();
        File.WriteAllText(etyPath, s);
        Debug.LogError("导出配置: " + etyPath);
    }
}
#endif
