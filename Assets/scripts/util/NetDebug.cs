
/*
Author: liyonghelpme
Email: 233242872@qq.com
*/

using System;
using System.IO;
using MyLib;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public enum AIDiff
{
    Easy = 0,
    Medium,
    Hard,
}
public class NetDebug : MonoBehaviour
{
    public bool debug;
    public static NetDebug netDebug;

    public bool IsWuDi = false;
    public bool IsTest = false;

    public bool JumpLogin = false;

    public bool TestNetTraffic = false;

    public bool TestServer = false;
    public bool TestLiYong = false;

    public bool TestUbuntu = false;
    public bool TestLocal = false;

    public bool TestHttp = false;
    public bool TestOutServer = false;

    public AIDiff aiDiff = AIDiff.Easy;

    public bool TestAndroid = false;

    public bool IsNewUser = false;

    public bool TestNetSyncMove = false;

    List<string> consoleDebug = new List<string>();
    public void AddConsole(string msg)
    {
        consoleDebug.Add(msg);
        if (consoleDebug.Count > 20)
        {
            consoleDebug.RemoveAt(0);
        }
    }

    private StreamWriter mWriter;
    void Awake()
    {
        netDebug = this;
#if UNITY_ANDROID && !UNITY_EDITOR
        Application.RegisterLogCallback(LogCallback);
        mWriter = new StreamWriter(Path.Combine(Application.persistentDataPath, "error.log"), true);
        mWriter.AutoFlush = false;
#endif
    }

    public void OnApplicationPause(bool pauseStatus)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (pauseStatus && mWriter != null)
        {
            mWriter.Flush();
        }
#endif
    }

    [ButtonCallFunc()] public bool Quit;

    public void QuitMethod()
    {
        WorldManager.worldManager.QuitWorldToLoginScene();
    }

    [ButtonCallFunc()] public bool TestError;

    public void TestErrorMethod()
    {
        GameObject a = null;
        var mb = a.GetComponents<MonoBehaviour>();
    }

    [ButtonCallFunc()] public bool Flush;

    public void FlushMethod()
    {
        OnApplicationPause(true);
    }

    void LogCallback(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Exception)
        {
            Log.Sys("Exception: "+logString);
            mWriter.WriteLine("{0}: {1}\n{2}", type, logString, stackTrace);
        }
    }

    // Use this for initialization
    void Start()
    {
        if (TestNetTraffic)
        {
            StartCoroutine(MoveAndShoot());
        }
    }

    //绕着尽量攻击
    IEnumerator MoveAndShoot()
    {
        int[] rdV = new[] { -1, 1 };
        var passTime = 0.0;
        var h = 1;
        var v = 1;
        MyEventSystem.PushEventStatic(MyEvent.EventType.EnterShoot);

        var sleepTime = 0.0f;
        var useDun = false;
        Debug.LogError("StartAI");
        var attPLsit = new List<float>()
        {
            4,
            2,
            1,
        };
        var moveLowList = new List<float>()
        {
            2,
            3,
            4,
        };
        float attackPer = attPLsit[(int)aiDiff];
        float movePerLow = moveLowList[(int)aiDiff];
        float movePer = 6;
        while (true)
        {
            if (ObjectManager.objectManager != null)
            {
                var player = ObjectManager.objectManager.GetMyPlayer();
                if (player != null)
                {
                    var enemy = SkillLogic.FindNearestEnemy(player);
                    if (enemy != null)
                    {
                        MyEventSystem.PushEventStatic(MyEvent.EventType.EnterShoot);
                        var dir = enemy.transform.position - player.transform.position;
                        dir.y = 0;
                        dir.Normalize();
                        var sevt = new MyEvent()
                        {
                            type = MyEvent.EventType.ShootDir,
                            vec2 = new Vector2(dir.x, dir.z),
                        };
                        MyEventSystem.myEventSystem.PushEvent(sevt);
                    }
                    if (passTime > attackPer)
                    {
                        var rd = Random.Range(0, 2);
                        h = rdV[rd];
                        rd = Random.Range(0, 2);
                        v = rdV[rd];
                        passTime -= attackPer;
                        try
                        {
                            GameInterface.gameInterface.PlayerAttack();
                        }
                        catch (Exception)
                        {
                        }
                    }

                    sleepTime += Time.deltaTime;

                    if (sleepTime >= movePerLow && sleepTime <= movePer)
                    {
                        var evt = new MyEvent(MyEvent.EventType.MovePlayer);
                        evt.localID = ObjectManager.objectManager.GetMyLocalId();
                        evt.vec2 = new Vector2(0, 0);
                        MyEventSystem.myEventSystem.PushLocalEvent(evt.localID, evt);

                        if (!useDun)
                        {
                            useDun = true;
                            GameInterface_Skill.OnSkill(0);

                        }
                        //yield return null;
                    }
                    else
                    {
                        /*
                        var evt = new MyEvent(MyEvent.EventType.MovePlayer);
                        evt.localID = ObjectManager.objectManager.GetMyLocalId();
                        evt.vec2 = new Vector2(h, v);
                        evt.vec2.Normalize();
                        MyEventSystem.myEventSystem.PushLocalEvent(evt.localID, evt);
                        */
                        var v2 = new Vector2(h, v);
                        v2.Normalize();
                        LeftController.Instance.MoveDir = v2;
                    }
                    if (sleepTime > movePer)
                    {
                        sleepTime = 0;
                        useDun = false;
                    }
                }
            }
            passTime += Time.deltaTime;
            yield return null;
        }

        Debug.LogError("StopAI");
        MyEventSystem.PushEventStatic(MyEvent.EventType.ExitShoot);
    }
    public List<string> testStr = new List<string>();

    public void AddMsg(string msg)
    {
        testStr.Add(msg);
        if (testStr.Count > 30)
        {
            testStr.RemoveAt(0);
        }
    }
    void OnGUI()
    {

        if (debug)
        {
            GUILayout.BeginVertical();
            //GUILayout.TextField(string.Join("\n", KBEngine.Bundle.sendMsg.ToArray()));
            GUILayout.TextField(string.Join("\n", testStr.ToArray()));
            GUILayout.EndVertical();

            GUI.TextField(new Rect(Screen.width * 3.0f / 4, 0, Screen.width / 4, Screen.height / 2), string.Join("\n", KBEngine.Bundle.recvMsg.ToArray()));
            GUI.TextField(new Rect(0, Screen.height * 3 / 4.0f, Screen.width / 4, Screen.height / 4), string.Join("\n", consoleDebug.ToArray()));
        }

    }

    [ButtonCallFunc()]
    public bool CloseNet;

    public void CloseNetMethod()
    {
        if (NetworkScene.Instance != null)
        {
            NetworkScene.Instance.rc.Disconnect();
        }
        else
        {
            NetMatchScene.Instance.rc.Disconnect();
        }
    }
}
