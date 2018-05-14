using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class RecordUI : IUserInterface
    {
        private string txt = @"对战共计:{0}场
最强场次：{1}场
胜率：{2}
三杀次数：{3}
四杀次数：{4}
五杀次数：{5}
总共击毁坦克数量：{6}
总共死亡次数：{7}";

        void Awake()
        {
            SetCallback("Ok", Hide);
        }
        // Use this for initialization
        private void Start()
        {
            StartCoroutine(QueryRecord());
        }

        private IEnumerator QueryRecord()
        {
            yield return StartCoroutine(RecordData.QueryRecord());
            var js = RecordData.recordData;
            if (js != null)
            {
                var total = js["total"].AsInt;
                var keys = new List<string>()
                {
                    "total",
                    "mvp",
                    "rate",
                    "threeKill",
                    "fourKill",
                    "fiveKill",
                    "totalKill",
                    "dieNum",
                };

                if (total > 0)
                {
                    var rate = js["mvp"].AsInt*100/js["total"].AsInt;
                    js["rate"] = rate + "%";
                    /*
                    var con = string.Format(txt, js["total"].AsInt, js["mvp"].AsInt, rate + "%", js["threeKill"].AsInt,
                        js["fourKill"].AsInt, js["fiveKill"].AsInt, js["totalKill"].AsInt, js["dieNum"]);
                    GetLabel("Content").text = con;
                    */
                    var ret = Util.GetAllChild(this.transform, "Label1");
                    Log.GUI("Label1: "+ret.Count);
                    for (var i = 0; i < keys.Count; i++)
                    {
                        ret[i].GetComponent<UILabel>().text = js[keys[i]];
                    }

                }
            }
        }

    }
}