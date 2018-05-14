using UnityEngine;
using System.Collections;

namespace MyLib
{
    /// <summary>
    /// 坦克匹配界面
    /// </summary>
    public class Map6 : CScene
    {
        public override bool IsNet
        {
            get
            {
                return true;
            }
        }

        public override bool IsMatch
        {
            get { return true; }
        }

        public override bool CanFight
        {
            get { return false; }
        }

        //和服务器建立连接之后 连接不会断开不会因为切换场景而删除掉这个map只负责网络连接
        private NetMatchScene netScene;

        protected override void Awake()
        {
            base.Awake();
            CameraController.Instance.gameObject.SetActive(false);
            var go = new GameObject("NetMatchScene");
            netScene = go.AddComponent<NetMatchScene>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CameraController.Instance.gameObject.SetActive(true);
        }

        IEnumerator Start()
        {
            while (WorldManager.worldManager.station != WorldManager.WorldStation.Enter)
            {
                yield return null;
            }
            var uiRoot = WindowMng.windowMng.GetMainUI();
            //WindowMng.windowMng.AddChild(uiRoot, Resources.Load<GameObject>("UI/TankNum"));
            WindowMng.windowMng.PushView("UI/TankNum");
        }


        public override void BroadcastMsg(CGPlayerCmd.Builder cmd)
        {
            if (state == SceneState.InGame)
            {
                netScene.BroadcastMsg(cmd);
            }
        }
    }

}