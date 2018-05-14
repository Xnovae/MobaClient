using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using SimpleJSON;
using System.Linq ;

#if UNITY_EDITOR
using UnityEditor;

[System.Serializable]
public class StrRes
{
	public string key;
	public ResourceFile rf;
}

[System.Serializable]
public class StrAssetBundle
{
	public string key;
	public AssetBundleFile assetbundle;
}
//检测不同类型资源同名的BUG
//资源名字匹配
public class AssetBundleExport : MonoBehaviour
{
	public List<StrRes> sr = new List<StrRes> ();
	Dictionary<string, ResourceFile> resFiles = new Dictionary<string, ResourceFile> ();

	public List<StrAssetBundle> assetbundles = new List<StrAssetBundle> ();
	Dictionary<string, AssetBundleFile> abFiles = new Dictionary<string, AssetBundleFile> ();

	private List<ResourceFile> openFiles = new List<ResourceFile> ();

	[ButtonCallFunc ()]public bool Export;

	public void ExportMethod ()
	{
		resFiles.Clear();
		abFiles.Clear();
		openFiles.Clear();
		sr.Clear ();
		assetbundles.Clear ();

		var dir = Path.Combine (Application.dataPath, "Resources");
		var resList = new DirectoryInfo (dir);
		var files = resList.GetFiles ("*.*", SearchOption.AllDirectories);
		Debug.Log ("files: " + files.Length);

        //Windows的斜杠问题
	    var dp = Application.dataPath.Replace("/", "\\");
		foreach (var f in files) {
			var assPath = f.FullName.Replace (dp, "Assets");
			Debug.Log ("fileExt: " + f.Extension);
            Debug.Log("fileFullName: "+f.FullName);
            Debug.Log("DataPath: "+Application.dataPath);
			if (f.Extension == ".prefab") {
				GetDepencyFiles (AssetDatabase.LoadAssetAtPath<GameObject> (assPath));
			} else {
			}
		}

		while (openFiles.Count > 0) {
			var of = openFiles [0];
			openFiles.RemoveAt (0);
			ReadOF (of);
		}

		foreach (var r in resFiles) {
			sr.Add (new StrRes () {
				key = r.Key,
				rf = r.Value,
			});
		}

		foreach (var r in abFiles) {
			assetbundles.Add (new StrAssetBundle () {
				key = r.Key,
				assetbundle = r.Value,
			});
			r.Value.CollectDependency (abFiles);
		}

		Build();
	}

	//忽略两个目录
	//Library/unity default resources
	//Resources/unity_builtin_extra
	private void ReadOF (ResourceFile objRf)
	{
		var dp = EditorUtility.CollectDependencies (new UnityEngine.Object[]{ objRf.obj });
		foreach (var d in dp) {
			Debug.Log ("dep: " + d.GetType () + " n " + d.ToString ());
			if (d.GetType () == typeof(GameObject)) {
				var root = PrefabUtility.FindPrefabRoot (d as GameObject);
				if (root != objRf.obj) {
					var path = AssetDatabase.GetAssetPath (root);
					if (!resFiles.ContainsKey (path)) {
						var rf1 = new ResourceFile () {
							path = path,
							obj = root,
						};
						resFiles.Add (path, rf1);
						openFiles.Add (rf1);

						var dirName = Path.GetDirectoryName (path);
						if (!abFiles.ContainsKey (dirName)) {
							var ab = new AssetBundleFile () {
								path = dirName,
							};
							abFiles.Add (dirName, ab);
						}

						var abF = abFiles [dirName];
						abF.resources.Add (rf1);
						rf1.assetBundleFile = abF.path;
					}

					var rf = resFiles [path];
					rf.AddPar (objRf);
					objRf.AddChild (rf);
				}
			} else if (d.GetType () == typeof(Mesh)
			           || d.GetType () == typeof(Shader)
			           || d.GetType () == typeof(Material)
			           || d.GetType () == typeof(Texture2D)
			           || d.GetType () == typeof(RenderTexture)
					   || d.GetType() == typeof(MonoScript)
			) {
				if (d != objRf.obj) {
					var path = AssetDatabase.GetAssetPath (d);
					if (!resFiles.ContainsKey (path)) {
						var rf1 = new ResourceFile () {
							path = path,
							obj = d,
						};
						resFiles.Add (path, rf1);
						openFiles.Add (rf1);

						var dirName = Path.GetDirectoryName (path);
						if (!abFiles.ContainsKey (dirName)) {
							var ab = new AssetBundleFile () {
								path = dirName,
							};
							abFiles.Add (dirName, ab);
						}

						var abF = abFiles [dirName];
						abF.resources.Add (rf1);
						rf1.assetBundleFile = abF.path;
					}

					var rf = resFiles [path];
					rf.AddPar (objRf);
					objRf.AddChild (rf);
				}
			} 
		}
	}

	//不是孩子 也不是成员 也不是自己 则是依赖
	private void GetDepencyFiles (UnityEngine.Object obj)
	{
	    if (obj == null)
	    {
	        return;
	    }
		Debug.Log ("Col: " + obj);
		var op = AssetDatabase.GetAssetPath (obj);
		if (!resFiles.ContainsKey (op)) {
			var nf = new ResourceFile () {
				path = op,
				obj = obj,
			};
			resFiles.Add (op, nf);
			openFiles.Add (nf);

			var dirName = Path.GetDirectoryName (op);
			if (!abFiles.ContainsKey (dirName)) {
				var ab = new AssetBundleFile () {
					path = dirName,
				};
				abFiles.Add (dirName, ab);
			}

			var abF = abFiles [dirName];
			abF.resources.Add (nf);
			nf.assetBundleFile = abF.path;
		}
	}

    private void Build()
    {

        var p = Path.Combine(Application.dataPath, "../abFiles");
        if (File.Exists(p))
        {
            Directory.Delete(p);
        }

        Directory.CreateDirectory(p);
        var tempPath = Path.Combine(Application.dataPath, "../TempAb");
        if (File.Exists(tempPath))
        {
            Directory.Delete(tempPath);
        }
        Directory.CreateDirectory(tempPath);

        foreach (var ab in assetbundles)
        {
            ab.assetbundle.Build(p, abFiles);
        }

        var jobj = new JSONClass();

        foreach (var r in sr)
        {
            r.rf.WriteJson(jobj);
        }

        var fp = Path.Combine(Application.dataPath, "../abFiles/resources.json");
        var s = jobj.ToString();
        File.WriteAllText(fp, s);


        var jobj2 = new JSONClass();
        foreach (var a in assetbundles)
        {
            a.assetbundle.WriteJson(jobj2);
        }

        var fp2 = Path.Combine(Application.dataPath, "../abFiles/abDepend.json");
        var s2 = jobj2.ToString();
        File.WriteAllText(fp2, s2);
       
        var jobj3 = new JSONClass();
        foreach (var a in assetbundles)
        {
            a.assetbundle.WriteMd5(jobj3);
        }

        var fp3 = Path.Combine(Application.dataPath, "../abFiles/md5.json");
        var s3 = jobj3.ToString();
        File.WriteAllText(fp3, s3);
       

    }
}
#endif
