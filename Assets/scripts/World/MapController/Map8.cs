using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyLib
{

    public class Map8 : CScene
    {
        public override bool ShowTeamColor
        {
            get
            {
                return false;
            }
        }

        public override bool IsEnemy(GameObject a, GameObject b)
        {
            var aattr = NetworkUtil.GetAttr(a);
            var battr = NetworkUtil.GetAttr(b);
            if (aattr != null && battr != null)
            {
                return a != b && b.tag == GameTag.Player && aattr.TeamColor != battr.TeamColor;
            }
            return false;
        }

        public override bool IsNet
        {
            get
            {
                return true;
            }
        }

        public override bool IsRevive
        {
            get
            {
                return true;
            }
        }


        private NetworkScene netScene;

        protected override void Awake()
        {
            base.Awake();
            netScene = gameObject.AddComponent<NetworkScene>();
            gameObject.AddComponent<NetworkLatency>();
        }

        private void Start()
        {
            gameObject.AddComponent<ScoreManager>();
            var newBee = PlayerPrefs.GetInt("NewBee", 0);
            //var newBee = 0;
            if (newBee == 0)
            {
                StartCoroutine(Util.WaitCb(5, () =>
                {
                    newBee = 1;
                    PlayerPrefs.SetInt("NewBee", 1);
                    //WindowMng.windowMng.PushView("UI/Newbee", false, false);
                    var uiMain = WindowMng.windowMng.GetMainUI();
                    WindowMng.windowMng.AddChild(uiMain, "UI/Newbee");
                }));
            }
            StartCoroutine(WaitInitMap());
        }

        /// <summary>
        /// 等待世界初始化结束 再初始化坦克网络
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitInitMap()
        {
            yield return StartCoroutine(NetworkUtil.WaitForWorldInit());
            netScene.InitMap();
        }
        public override void BroadcastMsg(CGPlayerCmd.Builder cmd)
        {
            if (state == SceneState.InGame)
            {
                netScene.BroadcastMsg(cmd);
            }
        }

        public override void BroadcastMsgUDP(CGPlayerCmd.Builder cmd)
        {
            if (state == SceneState.InGame)
            {
                netScene.BroadcastUDPMsg(cmd);
            }
        }
        public override void BroadcastKCP(CGPlayerCmd.Builder cmd)
        {
            if (state == SceneState.InGame)
            {
                netScene.BroadcastKCPMsg(cmd);
            }
        }
    }

}
