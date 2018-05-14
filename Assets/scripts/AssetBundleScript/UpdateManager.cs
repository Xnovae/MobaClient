using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using UnityEngine;
using System.Collections;

public class UpdateManager : MonoBehaviour
{

    private void Start()
    {
    }

    [ButtonCallFunc()] public bool UpdateFile;
    public void UpdateFileMethod()
    {
        StartCoroutine(CheckUpdate());
    }
   

    private IEnumerator DownloadAbFile(string fileName)
    {
        Debug.Log("DownloadAb: "+fileName);
		var par = Path.Combine (Application.dataPath, "../newAbFiles");
        var f = Path.Combine(par, fileName);
        var urlPar = "file://" + f;
        var www = new WWW(urlPar);
        yield return www;

        var newFile = www.bytes;
		var par1 = Path.Combine (Application.dataPath, "../abFiles");
        var f1 = Path.Combine(par1, fileName);
        if (File.Exists(f1))
        {
            File.Delete(f1);
        }
        File.WriteAllBytes(f1, newFile);
    }

    private void RemoveFile(string fileName)
    {
		var par1 = Path.Combine (Application.dataPath, "../abFiles");
        var f1 = Path.Combine(par1, fileName);
        if (File.Exists(f1))
        {
            File.Delete(f1);
        }
    }

    private void WriteJsonFile(string fileName, JSONClass jobj)
    {
        Debug.Log("UpdateJsonFile: "+fileName);
		var par1 = Path.Combine (Application.dataPath, "../abFiles");
        var f1 = Path.Combine(par1, fileName);
        Debug.Log("WriteJsonFile: "+f1);
        f1 = f1.Replace("\\", "/");

        if (File.Exists(f1))
        {
            File.Delete(f1);
        }
        File.WriteAllText(f1, jobj.ToString());
    }

    private IEnumerator DownloadFile(string fileName, JSONClass[] ret)
    {
		var par = Path.Combine (Application.dataPath, "../newAbFiles");
        var f = Path.Combine(par, fileName);
        f = f.Replace("\\", "/");
        Debug.Log("DownloadFile: "+f);

        var urlPar = "file://" + f;
        var www = new WWW(urlPar);
        yield return www;
        var newFile = SimpleJSON.JSON.Parse(www.text).AsObject;
        ret[0] = newFile;
    }

    public static IEnumerator LoadLocalFile(string fileName, JSONClass[] ret)
    {
		var par = Path.Combine (Application.dataPath, "../abFiles");
        var f = Path.Combine(par, fileName);
        f = f.Replace("\\", "/");
        var urlPar = "file://" + f;
        Debug.Log("LocalFilePath: "+urlPar);
        var www = new WWW(urlPar);
        yield return www;
        Debug.Log(www.text);
        var newFile = SimpleJSON.JSON.Parse(www.text).AsObject;
        ret[0] = newFile;
        Debug.LogError(newFile.ToString());
    }

    // Update is called once per frame
    private IEnumerator CheckUpdate()
    {
        var ret = new JSONClass[1];
        yield return StartCoroutine(DownloadFile("md5.json", ret));
        var md5File = ret[0];



        yield return StartCoroutine(LoadLocalFile("md5.json", ret));
        var localMd5 = ret[0];

        Debug.LogError("localMd5: " + localMd5.ToString());
        var result = CompareMd5(localMd5, md5File);
        if (result.Count > 0)
        {
            yield return StartCoroutine(DownloadAb(result));

            yield return StartCoroutine(DownloadFile("abDepend.json", ret));
            var abDepend = ret[0];

            yield return StartCoroutine(DownloadFile("resources.json", ret));
            var resources = ret[0];

            UpdateJsonFiles(md5File, abDepend, resources);
        }
    }


    private Dictionary<string, string> CompareMd5(JSONClass oldMd5, JSONClass newMd5)
    {
        //新增的文件
        //变更的文件
        //删除的文件
        var newFile = new Dictionary<string, string>();
        var changedFile = new Dictionary<string, string>();
        var deleteFile = new Dictionary<string, string>();
        Debug.LogError("OldMd5: "+oldMd5.ToString());
        Debug.LogError("NewMd5: "+newMd5.ToString());
        foreach (KeyValuePair<string, JSONNode> kv in newMd5)
        {
            Debug.Log("KeyValue: "+kv.Key+" "+" old "+oldMd5[kv.Key].ToString());

            if (oldMd5[kv.Key] == null) 
            {
                if (!string.IsNullOrEmpty(kv.Value["md5"].Value))
                {
                    newFile.Add(kv.Key, kv.Value);
                }
            }
            else
            {
                var om = oldMd5[kv.Key]["md5"].Value;
                if (!string.IsNullOrEmpty(om))
                {
                    var nm = kv.Value["md5"].Value;
                    if (nm != om)
                    {
                        changedFile.Add(kv.Key, kv.Value);
                    }
                }
            }
        }

        foreach (KeyValuePair<string, JSONNode> kv in oldMd5)
        {
            if (newMd5[kv.Key] == null)
            {
                deleteFile.Add(kv.Key, kv.Value);
            }
        }
        Debug.LogError("FileCount: "+newFile.Count+" change "+changedFile.Count+" delCount "+deleteFile.Count);

        var downloadFile = new Dictionary<string, string>();
        foreach (var n in newFile)
        {
            downloadFile.Add(n.Key, n.Value);
        }
        foreach (var n in changedFile)
        {
            downloadFile.Add(n.Key, n.Value);
        }
        foreach (var dfile in deleteFile)
        {
            RemoveFile(dfile.Key);
        }
        return downloadFile;
    }

    private IEnumerator DownloadAb(Dictionary<string, string> download)
    {
        foreach (var f in download)
        {
            yield return StartCoroutine(DownloadAbFile(f.Key));
        }
    }



    private void UpdateJsonFiles(JSONClass md5, JSONClass abdepend, JSONClass resources)
    {
        WriteJsonFile("md5.json", md5);
        WriteJsonFile("abDepend.json", abdepend);
        WriteJsonFile("resources.json", resources);
    }
}
