
/*
Author: liyonghelpme
Email: 233242872@qq.com
*/

/*
Author: liyonghelpme
Email: 233242872@qq.com
*/

using MyLib;
using UnityEngine;
using System.Collections;
using KBEngine;
using System;

public class ClientApp : UnityEngine.MonoBehaviour
{
    public static KBEngineApp gameapp = null;
    public static GameObject client;
    public static ClientApp Instance;
    public int updateInterval;

    /*
     * Player Position Update Frequency
     */ 
    public int updateIntervalOnSerialize;
    int nextSendTickCount = Environment.TickCount;
    //public string url = "10.1.2.223";
    //public int port = 17000;
    //public string testUrl = "192.168.2.5";
    public int testPort = 20000;
    //public bool debug = false;
    public string remoteServerIP;
    public int remotePort = 10001;
    public float syncFreq = 0.1f;

    public int remoteUDPPort = 10001;
    public int UDPListenPort = 10002;

    public int ServerHttpPort = 12002;
    public int remoteKCPPort = 6060;

    public bool testAI = false;

    public string QueryServerIP;
    void Awake()
    {
        client = gameObject;
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void StartServer() {
        Debug.Log("StartServer");
        KBEngineApp.url = "http://10.1.2.210";
        KBEngineApp.app.ip = "127.0.0.1";
        KBEngineApp.app.port = Convert.ToUInt16(testPort);

        //new MyLib.DemoServer();

        StartCoroutine(CheckConnectState());
    }
    // Use this for initialization
    void Start()
    {
        UnityEngine.MonoBehaviour.print("client app start");
        gameapp = new KBEngineApp(this);
    }

    //First Connect
    //Check State OK?
    //游戏转入后台 会导致网络连接断开
    IEnumerator CheckConnectState()
    {
        while (true)
        {
            bool conSuc = false;
            while (!conSuc)
            {
                var ret = KBEngine.KBEngineApp.app.login_loginapp();
                if (!ret)
                {
                    //MyLib.WindowMng.windowMng.ShowNotifyLog("网络连接失败");
                    Debug.LogError("FirstConnect Error");
                    //WaitTry
                    yield return new WaitForSeconds(1);
                }else {
                    //MyLib.WindowMng.windowMng.ShowNotifyLog("网络连接成功");
                    conSuc = true;
                }
                yield return new WaitForSeconds(1);
            }

            //Check Net not connect then reconnect
            while(true) {
                /*
                if(!KBEngine.KBEngineApp.app.networkInterface().valid()){
                    MyLib.WindowMng.windowMng.ShowNotifyLog("网络中断");
                    break;
                }
                */
                yield return new WaitForSeconds(1);
            }

            yield return new WaitForSeconds(1);
        }

    }

    void OnDestroy()
    {
        UnityEngine.MonoBehaviour.print("clientapp destroy");
        if (KBEngineApp.app != null)
        {
            //KBEngineApp.app.destroy();
            UnityEngine.MonoBehaviour.print("client app over " + gameapp.isbreak + " over = " + gameapp.kbethread.over);
        }
        /*
        if (MyLib.DemoServer.demoServer != null)
        {
            MyLib.DemoServer.demoServer.ShutDown();
        }
        */
    }

    void Update()
    {
        KBEUpdate();
    }

    //处理网络数据
    void KBEUpdate()
    {
        //处理网络回调
        gameapp.UpdateMain();
    }
    public bool IsPause = false;
    public void OnApplicationPause(bool pauseStatus) {
        IsPause = pauseStatus;
        if(pauseStatus && MyLib.ServerData.Instance != null){
            //ChuMeng.DemoServer.demoServer.GetThread().CloseServerSocket();
            MyLib.ServerData.Instance.SaveUserData();
        }
        if (pauseStatus)
        {
            var act = WorldManager.worldManager.GetActive();
            if (act != null && !act.IsCity)
            {
                WorldManager.ReturnCity();
            }
            StatisticsManager.Instance.QuitGame();
        }
    }

    [ButtonCallFunc()]
    public bool PauseTest;

    public void PauseTestMethod()
    {
        OnApplicationPause(true);
    }

}
