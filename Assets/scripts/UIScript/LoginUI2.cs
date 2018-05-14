using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class LoginUI2 : IUserInterface
    {
        //UIInput ip;
        UIInput port;
        UIInput sync;
        private UIPopupList popList;
        void Awake(){
            SetCallback("StartButton", OnStart);
            //ip = GetInput("IPInput");
            port = GetInput("PortInput");
            SetCallback("StartServer", OnServer);
            sync = GetInput("SyncInput");
            popList = GetName("ServerList").GetComponent<UIPopupList>();
        }
        private List<string> ips = new List<string>()
        {
            "192.168.3.118",
            "127.0.0.1",
            "127.0.0.1",
            "127.0.0.1",
            "127.0.0.1",
        };

        private void Start()
        {
            if (!SaveGame.saveGame.IsTest && !NetDebug.netDebug.TestAndroid)
            {
                NetDebug.netDebug.JumpLogin = true;
                popList.value = popList.items[0];
                OnStart(null);
                return;
            }
            else
            {
                GetName("MaskPanel").SetActive(false);
            }

            if (ServerData.Instance.playerInfo.HasServerIP)
            {
                var ipv = ServerData.Instance.playerInfo.ServerIP;
                var ind = ips.IndexOf(ipv);
                if (ind >=0 && ind < ips.Count)
                {
                    popList.value = popList.items[ind];
                }
            }
            if (NetDebug.netDebug.TestLocal)
            {
                popList.value = popList.items[2];
            }
            else if (NetDebug.netDebug.TestLiYong)
            {
                popList.value = popList.items[3];
            }else if (NetDebug.netDebug.TestUbuntu)
            {
                popList.value = popList.items[4];
            }else if (NetDebug.netDebug.TestOutServer)
            {
                popList.value = popList.items[0];
            }
            else
            {

                if (NetDebug.netDebug.IsTest)
                {
                    //ip.value = "127.0.0.1";
                    popList.value = popList.items[2];
                }
                if (NetDebug.netDebug.TestServer)
                {
                    //ip.value = "172.16.11.102";
                    popList.value = popList.items[1];
                }

                if (!NetDebug.netDebug.IsTest && !NetDebug.netDebug.TestServer)
                {
                    if (!ServerData.Instance.playerInfo.HasServerIP)
                    {
                        var ipv = ServerData.Instance.playerInfo.ServerIP;
                        var ind = ips.IndexOf(ipv);
                        if (ind >= 0 && ind < ips.Count)
                        {
                            popList.value = popList.items[ind];
                        }
                        else
                        {
                            popList.value = popList.items[0];
                        }
                    }
                    else
                    {
                        popList.value = popList.items[0];
                    }
                }
            }

            if (NetDebug.netDebug.JumpLogin)
            {
                OnStart(null);
            }
        }

        private bool serverYet = false;

        private void OnServer()
        {
            var ca = ClientApp.Instance;
            var ind = popList.items.IndexOf(popList.value);
            //ca.remoteServerIP = ips[ind];
            ca.QueryServerIP = ips[ind];
            ca.testPort = System.Convert.ToInt32(port.value);
            ca.syncFreq = System.Convert.ToSingle(sync.value);
            ca.StartServer();
            serverYet = true;
        }


        void OnStart(GameObject g){
            if(!serverYet) {
                OnServer();
            }
            var ind = popList.items.IndexOf(popList.value);
            //ip.value = ips[ind];
            ServerData.Instance.playerInfo.ServerIP = ips[ind];//ip.value;
            Log.Sys("ipValue: "+ips[ind]);

            GameInterface_Login.loginInterface.LoginGame();
#if UNITY_EDITOR
            StatisticsManager.Instance.StartCoroutine(StatisticsManager.Instance.CheckNewUser("test"));
#endif
        }

      
    }

}