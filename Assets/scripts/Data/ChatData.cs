using System.Collections.Generic;
using MyLib;
using SimpleJSON;
using UnityEngine;
using System.Collections;

public class ChatData : MonoBehaviour
{
    public class ChatInfo
    {
        public string who;
        public string content;
        public long ord;
    }
    public static ChatData Instance;
    public Queue<ChatInfo> chatInfo = new Queue<ChatInfo>();

    void Awake()
    {
        Instance = this;
    }

    private bool inRecv = false;
    private int curOrd = 0;
    private IEnumerator Receive()
    {
        while (true)
        {
            var qs = new Dictionary<string, object>()
            {
                {"ord", curOrd},
            };
            var ret = new string[1];
            yield return StartCoroutine(StatisticsManager.DoWebReq("GetMsg"+StatisticsManager.QueToStr(qs), ret));
            if (ret[0] != null)
            {
                Log.Net("ChatResponse: "+ret[0]);
                var js = SimpleJSON.JSON.Parse(ret[0]).AsObject;
                var array = js["chat"].AsArray;
                foreach (JSONNode d in array)
                {
                    var da = d.AsArray;
                    var od = da[2].AsInt;
                    if (od >= curOrd)
                    {
                        chatInfo.Enqueue(new ChatInfo()
                        {
                            who = da[0].Value,
                            content = da[1].Value,
                            ord = od,
                        });
                        curOrd = od+1;
                    }
                }
                while (chatInfo.Count > 30)
                {
                    chatInfo.Dequeue();
                }
                
                var uc = new MyEvent(MyEvent.EventType.UpdateChat);
                MyEventSystem.myEventSystem.PushEvent(uc);
            }
            yield return null;
        }
    }

    public void StartReceive()
    {
        if (inRecv)
        {
            return;
        }
        inRecv = true;
        StartCoroutine(Receive());
    }

    public string FetchNew()
    {
        if (chatInfo.Count > 0)
        {
            ChatInfo ci = null;
            foreach (var info in chatInfo)
            {
                ci = info;
            }
            if (ci != null)
            {
                return ci.who + ":" + ci.content;
            }
        }
        return "点击聊天";
    }
}
