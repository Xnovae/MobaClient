using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using KBEngine;
using MyLib;
using UnityEngine;
using System.Collections;
using MonoBehaviour = UnityEngine.MonoBehaviour;

public class NetworkLatency : MonoBehaviour
{
    public static NetworkLatency Instance;

    void Awake()
    {
        Instance = this;
    }
	// Use this for initialization
    private void Start()
    {
        StartCoroutine(CheckNet());
    }


    IEnumerator CheckFPS(GameObject timeLabel)
    {
        var label = timeLabel.GetComponent<IUserInterface>().GetLabel("FPS");
        label.text = "";

        double beginTime = Util.GetTimeNow();
        int frameCount = 0;
        while (true)
        {
            yield return null;
            frameCount++;
            //Log.Sys("FrameCoun: "+frameCount);
            if (frameCount >= 40)
            {
                var endTime = Util.GetTimeNow();
                var diff = endTime - beginTime;
                var fps1 = frameCount/diff;
                //Log.Sys("FrameCounTime: "+frameCount+" time "+diff);
                //if (SaveGame.saveGame.IsTest)
                {
                    label.text = (int) fps1 + "fps";
                }
                beginTime = endTime;
                frameCount = 0;
            }
        } 
    }

    private List<double> sample = new List<double>(); 
    private IEnumerator CheckNet()
    {
        yield return new WaitForSeconds(1);
        while (WorldManager.worldManager.station != WorldManager.WorldStation.Enter)
        {
            yield return null;
        }
        yield return new WaitForSeconds(5);
        //Debug.LogError("InitTimeLabel");
        var uiRoot = WindowMng.windowMng.GetMainUI();
        var timeLabel = WindowMng.windowMng.AddChild(uiRoot, Resources.Load<GameObject>("UI/timeLabel"));
        var label = timeLabel.GetComponent<IUserInterface>().GetLabel("Label");
        label.text = "";
        StartCoroutine(CheckFPS(timeLabel));

        while (true)
        {
            var ns = this.gameObject.GetComponent<NetworkScene>();

            RemoteClient rc = null;
            if (ns != null)
            {
                rc = ns.rc;
            }
            else
            {
                var nm = GetComponent<NetMatchScene>();
                if (nm != null)
                {
                    rc = nm.rc;
                }
            }


            if (rc != null)
            {
                var ht = CGPlayerCmd.CreateBuilder();
                ht.Cmd = "HeartBeat";
                Bundle bundle;
                var data = KBEngine.Bundle.GetPacketFull(ht, out bundle);
                var startTime = Util.GetTimeNow();
                yield return StartCoroutine(rc.SendWaitResponse(data.data, data.fid, (packet) =>
                {

                }, bundle));
                var endTime = Util.GetTimeNow();
                var dt = endTime - startTime;
                sample.Add(dt);
                var t = ShowTime();
                Log.Sys("HeartBeat: "+dt);
                //if (SaveGame.saveGame.IsTest)
                {
                    label.text = (int) (t*1000) + "ms";
                }
                yield return new WaitForSeconds(2);
            }
            else
            {
                yield return new WaitForSeconds(2);
            }
        }
    }


    /// <summary>
    /// 客户端发送HeartBeat 到服务器返回HeartBeat之间的时间差
    /// 网络往返时间
    /// </summary>
    public static float LatencyTime = 0.05f;
    float ShowTime()
    {
        var sum = 0.0;
        foreach (var f in sample)
        {
            sum += f;
        }
        sum /= sample.Count;
        if (sample.Count > 5)
        {
            sample.RemoveAt(0);
        }
        LatencyTime = (float)sum;
        return (float)sum;
    }
}
