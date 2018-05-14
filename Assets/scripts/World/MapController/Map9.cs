using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyLib
{
    /// <summary>
    /// 选择英雄场景
    /// </summary>
    public class Map9 : CScene
    {
        protected override void Awake()
        {
            base.Awake();
            CameraController.Instance.gameObject.SetActive(false);
        }

        protected override void OnDestroy()
        {
            CameraController.Instance.gameObject.SetActive(true);
            base.OnDestroy();
        }

        private IEnumerator Start()
        {
            while (WorldManager.worldManager.station != WorldManager.WorldStation.Enter)
            {
                yield return null;
            }

            var uiRoot = WindowMng.windowMng.GetMainUI();
            WindowMng.windowMng.PushView("UI/SelectHero");
        }

        public override void BroadcastMsg(CGPlayerCmd.Builder cmd)
        {
            if (NetMatchScene.Instance != null)
            {
                NetMatchScene.Instance.BroadcastMsg(cmd);
            }
        }
    }

}