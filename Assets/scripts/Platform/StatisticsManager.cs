using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using MyLib;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Text;
using MyLib;
using SimpleJSON;

/// <summary>
/// 类似于友盟统计的功能
/// 向服务器Http发送统计请求
/// </summary>
public class StatisticsManager : MonoBehaviour
{
    public static StatisticsManager Instance;

    public string DeviceID;
    public bool IsBlack = false;
    public bool IsClose = false;
    public string CloseMsg = "";
    void Awake()
    {
        Instance = this;
        DeviceID = SystemInfo.deviceUniqueIdentifier;
    }

    public IEnumerator CheckBlack()
    {
        var qs = new Dictionary<string, object>()
        {
            {"did", DeviceID},
        };
        var ret = new string[1];
        yield return StartCoroutine(DoWebReq("IsClose"+QueToStr(qs), ret));

        if (ret[0] != null)
        {
            var js = SimpleJSON.JSON.Parse(ret[0]);
            var isClose = js["IsClose"].AsBool;
            var isBlack = js["IsBlack"].AsBool;
            IsClose = isClose;
            if (isClose)
            {
                IsBlack = isBlack;
                if (IsBlack)
                {
                    Util.ShowMsg("对不起你已经被加入黑名单,无法进行游戏");
                }
                else
                {
                    CloseMsg = js["CloseMsg"].Value;
                    Util.ShowMsg(CloseMsg);
                }
            }
        }
    }

    public static IEnumerator DoWebReq(string que, string[] result)
    {
        var url = "http://" + ClientApp.Instance.QueryServerIP + ":" + ClientApp.Instance.ServerHttpPort + "/" + que;
        Log.Net("HttpReq: " + url);
        var w = new WWW(url);
        yield return w;
        if (string.IsNullOrEmpty(w.error))
        {
            result[0] = w.text;
            Log.Sys("HttpResult: "+result[0]);
        }
        else
        {
            result[0] = null;
        }
    }

    public static string QueToStr(Dictionary<string, object> nvc)
    {
        var arr = new List<string>();
        foreach (var kv in nvc)
        {
            var k = kv.Key;
            var v = kv.Value;
            Log.Net("Http: kv: "+k+" v "+v);
            arr.Add(string.Format("{0}={1}", WWW.EscapeURL(k), WWW.EscapeURL(v.ToString())));
        }
        var array = arr.ToArray();
        return "?" + string.Join("&", array);
    }

    public IEnumerator CheckNewUser(string name)
    {
        NetDebug.netDebug.AddMsg("WaitForSDKInit");
        while (!PlatformSdkManager.Instance.InitYet)
        {
            yield return null;
        }
        NetDebug.netDebug.AddMsg("SDK Init:"+PlatformSdkManager.Instance.Uid
            +" userName "+PlatformSdkManager.Instance.UserName
            +" plat "+PlatformSdkManager.Instance.PlatformID
            +" pn "+PlatformSdkManager.Instance.PlatformName
            +" ext "+PlatformSdkManager.Instance.Ext);

        Log.Net("CheckNewUser");
        var ret = new String[1];
        var qs1 = new Dictionary<string, object>()
        {
            {"name", PlatformSdkManager.Instance.UserName},
            {"uid", PlatformSdkManager.Instance.Uid},
            {"ext", PlatformSdkManager.Instance.Ext},
        };
        yield return StartCoroutine(DoWebReq("LoginVerify"+QueToStr(qs1), ret));


        NetDebug.netDebug.AddMsg("LoginVerify");


        NetDebug.netDebug.AddMsg("LoginTimeUpdate");
    }

    public IEnumerator LoginServer(string uid)
    {
        NetDebug.netDebug.AddMsg("WaitForSDKInit");
        while (!PlatformSdkManager.Instance.InitYet)
        {
            yield return null;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        PlatformSdkManager.Instance.Uid = uid;
#endif

        var ret = new String[1];
        var qs1 = new Dictionary<string, object>()
        {
            {"pid", PlatformSdkManager.Instance.PlatformID},
            {"uid", PlatformSdkManager.Instance.Uid},
            {"did", PlatformSdkManager.Instance.DeviceID}
        };

        yield return StartCoroutine(DoWebReq("LoginServer" + QueToStr(qs1), ret));

        if (ret[0] != null)
        {
            var js = SimpleJSON.JSON.Parse(ret[0]);
            if (js["ret"].AsInt == 0)
            {
                PlatformSdkManager.Instance.EnterGameLog("1", "NoName", "1", "1", "1");
                Application.LoadLevel("CreateChar");
            }
            else
            {
                UserInfo.UserName  = js["info"]["name"];
                UserInfo.UserLevel = js["info"]["level"].AsInt;
                UserInfo.UserExp   = js["info"]["exp"].AsInt;
                UserInfo.MedalNum  = js["info"]["medal"].AsInt;
                WorldManager.ReturnCity();
            }
        }
        else
        {
            Util.ShowMsg("服务器未开放");
        }

        if (NetDebug.netDebug.TestHttp)
        {
            Application.LoadLevel("MainLogin");
        }
    }


    public IEnumerator CreateChar(string name)
    {
        var ret = new String[1];
        var qs1 = new Dictionary<string, object>()
        {
            {"pid", PlatformSdkManager.Instance.PlatformID},
            {"uid", PlatformSdkManager.Instance.Uid},
            {"name", name}
        };

        yield return StartCoroutine(DoWebReq("CreateChar" + QueToStr(qs1), ret));
        
        if (ret[0] != null)
        {            
            var js = SimpleJSON.JSON.Parse(ret[0]);
            if (js["ret"].AsInt == 0)
            {
                UserInfo.UserName = js["info"]["name"];
                UserInfo.UserLevel = js["info"]["level"].AsInt;
                UserInfo.UserExp = js["info"]["exp"].AsInt;
                UserInfo.MedalNum = js["info"]["medal"].AsInt;
                WorldManager.ReturnCity();
            }
            else if(js["ret"].AsInt == 1)
            {
                Util.ShowMsg("创建角色失败,请重试");
            }
            else if (js["ret"].AsInt == 2)
            {
                Util.ShowMsg("角色名重复,创建角色失败");
            }
        }
        else
        {
            Util.ShowMsg("无法连接到服务器");
        }
    }

    public IEnumerator ExchangeMedal()
    {
        var ret = new String[1];
        var qs1 = new Dictionary<string, object>()
        {
            {"pid", PlatformSdkManager.Instance.PlatformID},
            {"uid", PlatformSdkManager.Instance.Uid},
        };

        yield return StartCoroutine(DoWebReq("ExchangeMedal" + QueToStr(qs1), ret));

        if (ret[0] != null)
        {
            var js = SimpleJSON.JSON.Parse(ret[0]);
            if (js["ret"].AsInt == 0)
            {
                UserInfo.UserName  = js["info"]["name"];
                UserInfo.UserLevel = js["info"]["level"].AsInt;
                UserInfo.UserExp   = js["info"]["exp"].AsInt;
                UserInfo.MedalNum  = js["info"]["medal"].AsInt;

                if (MainDetailUI.Instance != null)
                {
                    MainDetailUI.Instance.UpdateInfo();
                }
            }
            else if (js["ret"].AsInt == 1)
            {
                Util.ShowMsg("兑换勋章失败");
            }
        }
        else
        {
            Util.ShowMsg("无法连接到服务器");
        }
    }

    public IEnumerator QueryUserInfo()
    {
        var ret = new String[1];
        var qs1 = new Dictionary<string, object>()
        {
            {"pid", PlatformSdkManager.Instance.PlatformID},
            {"uid", PlatformSdkManager.Instance.Uid},
        };

        yield return StartCoroutine(DoWebReq("QueryUserInfo" + QueToStr(qs1), ret));

        if (ret[0] != null)
        {
            var js = SimpleJSON.JSON.Parse(ret[0]);
            if (js["ret"].AsInt == 1)
            {
                UserInfo.UserName   = js["info"]["name"];
                UserInfo.UserLevel  = js["info"]["level"].AsInt;
                UserInfo.UserExp    = js["info"]["exp"].AsInt;
                UserInfo.MedalNum   = js["info"]["medal"].AsInt;
                UserInfo.HaveRename = js["info"]["rename"].AsInt;
                
                if (MainDetailUI.Instance != null)
                {
                    MainDetailUI.Instance.UpdateInfo();
                }
            }
        }
        else
        {
            Util.ShowMsg("无法连接到服务器");
        }
    }

    public IEnumerator GetAllMail()
    {
        var ret = new String[1];
        var qs1 = new Dictionary<string, object>()
        {
            {"pid", PlatformSdkManager.Instance.PlatformID},
            {"uid", PlatformSdkManager.Instance.Uid},
        };

        yield return StartCoroutine(DoWebReq("GetAllMail" + QueToStr(qs1), ret));

        if (ret[0] != null)
        {
            var js = SimpleJSON.JSON.Parse(ret[0]);
            if (js["ret"].AsInt == 0)
            {
                MainDetailUI.Instance.MailInfoList.Clear();
                foreach (var d in js["result"].AsArray)
                {
                    var mailInfo = new MailInfo();
                    mailInfo.id = (d as JSONNode)["mailid"].AsInt;
                    mailInfo.title = (d as JSONNode)["title"];
                    mailInfo.sender = (d as JSONNode)["sender"];
                    mailInfo.dateTime = (d as JSONNode)["sendtime"];
                    mailInfo.state = (d as JSONNode)["state"].AsInt;
                    mailInfo.text = (d as JSONNode)["content"];
                    MainDetailUI.Instance.MailInfoList.Add(mailInfo);
                }

                if (MainDetailUI.Instance != null)
                {
                    MainDetailUI.Instance.OpenMail();
                }
            }
            else if (js["ret"].AsInt == 1)
            {
                if (MainDetailUI.Instance != null)
                {
                    MainDetailUI.Instance.OpenMail();
                }
            }
        }
        else
        {
            Util.ShowMsg("无法连接到服务器");
        }
    }

    public IEnumerator ReadMail(int id)
    {
        var ret = new String[1];
        var qs1 = new Dictionary<string, object>()
        {
            {"mailid",id }
        };

        yield return StartCoroutine(DoWebReq("ReadMail" + QueToStr(qs1), ret));

        if (ret[0] != null)
        {
            var js = SimpleJSON.JSON.Parse(ret[0]);
            if (js["ret"].AsInt == 0)
            {
                if (MailUI.Instance != null)
                {
                    MailUI.Instance.UpdateMailState(js["mailid"].AsInt,js["mailstate"].AsInt);
                }
            } 
        }
    }

    public IEnumerator DeleteMail(int id)
    {
        var ret = new String[1];
        var qs1 = new Dictionary<string, object>()
        {
            {"mailid",id }
        };

        yield return StartCoroutine(DoWebReq("DeleteMail" + QueToStr(qs1), ret));

        if (ret[0] != null)
        {
            var js = SimpleJSON.JSON.Parse(ret[0]);
            if (js["ret"].AsInt == 0)
            {
                if (MailUI.Instance != null)
                {
                    MailUI.Instance.DeleteMail(js["mailid"].AsInt);
                }
            }
        }
    }

    public IEnumerator OnekeyDeleteMail()
    {
        var ret = new String[1];
        var qs1 = new Dictionary<string, object>()
        {
            {"pid", PlatformSdkManager.Instance.PlatformID},
            {"uid", PlatformSdkManager.Instance.Uid},
        };

        yield return StartCoroutine(DoWebReq("OneKeyDeleteMail" + QueToStr(qs1), ret));

        if (ret[0] != null)
        {
            var js = SimpleJSON.JSON.Parse(ret[0]);
            if (js["ret"].AsInt == 0)
            {
                List<int> idList = new List<int>();
                foreach (var d in js["result"].AsArray)
                {
                    idList.Add((d as JSONNode)["mailid"].AsInt);
                }

                if (idList.Count > 0)
                {
                    if (MailUI.Instance != null)
                    {
                        MailUI.Instance.DeleteMails(idList);
                    }
                }
            }
        }
    }

    public IEnumerator CheckMailTip()
    {
        var ret = new String[1];
        var qs1 = new Dictionary<string, object>()
        {
            {"pid", PlatformSdkManager.Instance.PlatformID},
            {"uid", PlatformSdkManager.Instance.Uid},
        };

        yield return StartCoroutine(DoWebReq("CheckMailTip" + QueToStr(qs1), ret));

        if (ret[0] != null)
        {
            var js = SimpleJSON.JSON.Parse(ret[0]);
            if (js["ret"].AsInt == 0)
            {
                if (MainDetailUI.Instance != null && MailUI.Instance == null)
                {
                    MainDetailUI.Instance.CheckMailTip();
                }
            }
        }
    }


    public void QuitGame()
    {
        var ret = new String[1];
        var queryString = new Dictionary<string, object>();
        queryString["did"] = DeviceID;
        queryString["account_name"] = PlatformSdkManager.Instance.Uid;
        StartCoroutine(DoWebReq("QuitGame" + QueToStr(queryString), ret));
    }


    public IEnumerator GMFeedback(string content,string type)
    {
        var ret = new String[1];
        var platform = PlatformSdkManager.Instance.PlatformName;
        if (string.IsNullOrEmpty(platform))
        {
            platform = "PC";
        }
        var qs1 = new Dictionary<string, object>()
        {
            {"RoleId", PlatformSdkManager.Instance.Uid},
            {"RoleName", UserInfo.UserName},
            {"AccountName", PlatformSdkManager.Instance.Uid},
            {"ComplaintType", type},
            {"Content", content},
            {"Platform", platform},
        };

        yield return StartCoroutine(DoWebReq("GMFeedback" + QueToStr(qs1), ret));

        if (ret[0] != null)
        {
            var js = SimpleJSON.JSON.Parse(ret[0]);
            if (js["ret"].AsInt == 0)
            {
                Util.ShowMsg("发送反馈成功");
            }
            else if (js["ret"].AsInt == 1)
            {
                Util.ShowMsg("发送反馈失败");
            }
        }
        else
        {
            Util.ShowMsg("无法连接到服务器");
        }
    }

    public IEnumerator RenameUserName(string name)
    {
        var ret = new String[1];

        var qs1 = new Dictionary<string, object>()
        {
            {"pid", PlatformSdkManager.Instance.PlatformID},
            {"uid", PlatformSdkManager.Instance.Uid},
            {"name", name},
        };

        yield return StartCoroutine(DoWebReq("RenameUserName" + QueToStr(qs1), ret));

        if (ret[0] != null)
        {
            var js = SimpleJSON.JSON.Parse(ret[0]);
            if (js["ret"].AsInt == 0)
            {
                Util.ShowMsg(js["desc"]);
                UserInfo.UserName = js["name"];
                UserInfo.HaveRename = js["rename"].AsInt;
                if (MainDetailUI.Instance != null)
                {
                    MainDetailUI.Instance.UpdateInfo();
                }
            }
            else
            {
                Util.ShowMsg(js["desc"]);
            }
        }
        else
        {
            Util.ShowMsg("无法连接到服务器,改名失败");
        }
    }
}
