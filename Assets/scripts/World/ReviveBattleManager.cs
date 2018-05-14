using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class ReviveBattleManager : MonoBehaviour
    {
        public void ReviveMe()
        {
            StartCoroutine(Revive());
        }

        IEnumerator Revive()
        {
            var leftTime = 5f;
            GameObject not = null;
            WindowMng.windowMng.ShowNotifyLog("", 5.2f, delegate(GameObject n)
            {
                not = n;
            });

            //not = WindowMng.windowMng.PushView("UI/ReviveNotify");
            while (not == null)
            {
                yield return null;
            }

            var notify = not.GetComponent<NotifyUI>();
            if (notify != null)
            {
                while (leftTime > 0)
                {
                    Log.GUI("CountLeftTime " + leftTime);
                    notify.SetText(string.Format("复活倒计时{0}s", (int)leftTime));
                    leftTime -= Time.deltaTime;
                    yield return null;
                }
                notify.SetText(string.Format("复活倒计时{0}s", (int)0));
            }
            //yield return new WaitForSeconds(0.1f);
            //WindowMng.windowMng.PopView();

            StartCoroutine(ObjectManager.objectManager.GetMyAttr().Revive());
            var bat = GetComponent<BattleManager>();
            bat.levelOver = false;
        }
    }
}