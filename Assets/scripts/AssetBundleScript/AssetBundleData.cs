using System.Security.Cryptography;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.IO;

//一个AssetBundle 依赖多个其它AssetBundle
[System.Serializable]
public class AssetBundleFile
{
	public string path;
	public List<string> dependency = new List<string> ();
	public List<ResourceFile> resources = new List<ResourceFile> ();
	public bool buildYet = false;
	public string md5 = "";

	public void CollectDependency (Dictionary<string, AssetBundleFile> abFiles)
	{
		dependency.Clear ();
		foreach (var r in resources) {
			AddDep (r);
		}
		CollectObjs();
	}

	//包含的Resource文件 所依赖的资源文件 以及其所在的AssetBundle文件
	//只用收集所有Resource直接依赖的Resource即可
	//因为Unity分析依赖的时候是一分析到底的
	private void AddDep (ResourceFile rf)
	{
		foreach (var r in rf.dependencyFile) {
			var ab = r.rf.assetBundleFile;
			if (!dependency.Contains (ab) && ab.Contains("Assets")) {
				dependency.Add (ab);
			}
		}
	}

	private UnityEngine.Object[] objs = null;
	private string[] names = null;
	private void CollectObjs() {
		objs = new Object[resources.Count];
		names = new string[resources.Count];
		for (var i = 0; i < resources.Count; i++) {
			objs [i] = resources [i].obj;
			names[i] = Path.GetFileName(resources[i].path);
		}
	}

#if UNITY_EDITOR
	//只需要生成依赖文件的ID即可 不需要组件 除非使用了对象的组件
	//不需要收集依赖
	public void TempBuild() {
		var abPath = path.Replace ("/", "_");
		var tempPath = Path.Combine(Application.dataPath, "../TempAb");
		var fName = Path.Combine (tempPath, abPath);
		BuildPipeline.BuildAssetBundleExplicitAssetNames (objs, names, fName, BuildAssetBundleOptions.CompleteAssets | BuildAssetBundleOptions.DeterministicAssetBundle, BuildTarget.StandaloneOSXIntel64);
	}


	public void Build (string p, Dictionary<string, AssetBundleFile> allAb)
	{
		if (!path.Contains ("Assets/")) {
			return;
		}

		var abPath = path.Replace ("/", "_");
		var fName = Path.Combine (p, abPath);
		BuildPipeline.PushAssetDependencies();
		foreach(var d in dependency) {
			allAb[d].TempBuild();
		}
		//是否收集脚本
		BuildPipeline.PushAssetDependencies();
		BuildPipeline.BuildAssetBundleExplicitAssetNames (objs, names, fName, BuildAssetBundleOptions.CompleteAssets | BuildAssetBundleOptions.DeterministicAssetBundle, BuildTarget.StandaloneOSXIntel64);
		BuildPipeline.PopAssetDependencies();
		BuildPipeline.PopAssetDependencies();

		using(var m = MD5.Create()){
			using(var stream = File.OpenRead(fName)) {
				var h = m.ComputeHash(stream);
				//md5 = System.BitConverter.ToString(h).Replace("-", "");
				md5 = System.Convert.ToBase64String(h);
			}
		}
		buildYet = true;
	}
#endif


	public void WriteMd5(SimpleJSON.JSONClass jobj) {
		var abPath = path.Replace ("/", "_");
		var oj = new SimpleJSON.JSONClass();
		oj.Add("md5", md5);
		jobj.Add(abPath, oj);
	}

	public void WriteJson(SimpleJSON.JSONClass jobj) {
		var abPath = path.Replace ("/", "_");
		var ls = new SimpleJSON.JSONArray();
		foreach(var dep in dependency) {
			ls.Add(dep.Replace("/", "_"));
		}
		jobj.Add(abPath, ls);
	}
}

[System.Serializable]
public class PackRes
{
	public string n;
	[System.NonSerialized]
	public ResourceFile rf;
}

//一个Resource属于某个AssetBundle
//一个Resource依赖多个其它资源文件
[System.Serializable]
public class ResourceFile
{
	public string assetBundleFile;
	public string path;
	public UnityEngine.Object obj;
	//依赖哪些ResourceFiles
	public List<PackRes> dependencyFile = new List<PackRes> ();

	public void AddChild (ResourceFile f)
	{
		foreach (var c in dependencyFile) {
			if (c.rf == f) {
				return;
			}
		}
		dependencyFile.Add (new PackRes () {
			n = f.path + "@" + f.obj.name,
			rf = f,
		});

	}

	//哪些文件依赖自己
	public List<PackRes> parentFile = new List<PackRes> ();

	public void AddPar (ResourceFile rf)
	{
		foreach (var p in parentFile) {
			if (p.rf == rf) {
				return;
			}
		}
		parentFile.Add (new PackRes () {
			n = rf.path + "@" + rf.obj.name,
			rf = rf,
		});
	}

	public void WriteJson(SimpleJSON.JSONClass cl) {
		//var jd = new SimpleJSON.JSONData();
		//jd.Value = 
		var fn = Path.GetFileName(path);
		var abName = assetBundleFile.Replace("/", "_");
		cl.Add(fn, abName); 
	}
}

//游戏所有的资源文件 包括直接Resources目录和间接引用
public class ResourceManager
{
	public List<ResourceFile> allFiles = new List<ResourceFile> ();
}
