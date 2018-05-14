using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;

using System.IO;

public class PackObj
{
	public UnityEngine.Object obj;
}

public class AssetBundleLoader : MonoBehaviour
{
	public JSONClass res;
	public JSONClass abDepend;

	[ButtonCallFunc ()] public bool Load;

	public void LoadMethod ()
	{
	    StartCoroutine(LoadCor());
	}

    private IEnumerator LoadCor()
    {
        var ret = new JSONClass[1];
        yield return StartCoroutine(UpdateManager.LoadLocalFile("resources.json", ret));
        res = ret[0];

        yield return StartCoroutine(UpdateManager.LoadLocalFile("abDepend.json", ret));
        abDepend = ret[0];

		StartCoroutine (GetObj (fileName, new PackObj ()));
    }



    public string fileName;

	public IEnumerator GetObj (string resName, PackObj ret)
	{
		if (!string.IsNullOrEmpty (res [resName].Value)) {
			var ab = res [resName].Value;
			var arr = abDepend [ab].AsArray;
			List<AssetBundle> allBundles = new List<AssetBundle> ();
			foreach (var ab2 in arr) {
				var abName = (ab2 as JSONNode).Value;
				yield return StartCoroutine (LoadAb (abName, ret));
				if (ret.obj != null) {
					allBundles.Add (ret.obj as AssetBundle);
				}
			}
			foreach (var ab3 in allBundles) {
				//ab3.LoadAll ();
			}

			yield return StartCoroutine (LoadAb (ab, ret));
			Debug.Log ("LoadAb: " + ret.obj);
			var abLoad = ret.obj as AssetBundle;

            /*
			var res2 = abLoad.Load (resName);
			Debug.Log ("Resource: " + res2);
			var newGo = GameObject.Instantiate (res2) as GameObject;
            */

		}
	}

	private IEnumerator LoadAb (string name, PackObj obj)
	{
	    //var dp = Application.dataPath.Replace("/", "\\");
		var par = Path.Combine (Application.dataPath, "../abFiles");
		var f = Path.Combine (par, name);
		Debug.Log ("fileName: " + f);
	    f = f.Replace("\\", "/");
		var www = new WWW ("file://" + f);
		yield return www;
		obj.obj = www.assetBundle;
	}

}
