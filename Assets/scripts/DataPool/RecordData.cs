using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using System.Collections;

public class RecordData : MonoBehaviour
{
    public static JSONClass recordData = null;

    public static IEnumerator QueryRecord()
    {
        var qs = new Dictionary<string, object>()
        {
            {"uid", PlatformSdkManager.Instance.Uid},
        };
        var ret = new String[1];
        yield return ClientApp.Instance.StartCoroutine(StatisticsManager.DoWebReq("Record" + StatisticsManager.QueToStr(qs), ret));
        if (ret[0] != null)
        {
            recordData = SimpleJSON.JSON.Parse(ret[0]).AsObject;
        }
        else
        {
            
        }
        Log.Net("RecordData: "+recordData);
    }


    public static void UpdateRecord(JSONClass js)
    {
        var qs = new Dictionary<string, object>()
        {
            {"uid", PlatformSdkManager.Instance.Uid},
        };
        if (js["total"] != null)
        {
            qs["total"] = js["total"].AsInt;
        }
        if (js["mvp"] != null)
        {
            qs["mvp"] = js["mvp"].AsInt;
        }
        if (js["threeKill"] != null)
        {
            qs["threeKill"] = js["threeKill"].AsInt;
        }
        if (js["fourKill"] != null)
        {
            qs["fourKill"] = 1;
        }
        if (js["fiveKill"] != null)
        {
            qs["fiveKill"] = 1;
        }
        if (js["totalKill"] != null)
        {
            qs["totalKill"] = 1;
        }
        if (js["dieNum"] != null)
        {
            qs["dieNum"] = 1;
        }

        var ret = new String[1];
        ClientApp.Instance.StartCoroutine(
            StatisticsManager.DoWebReq("UpdateRecord" + StatisticsManager.QueToStr(qs),
            ret));
    }
}
