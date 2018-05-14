using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class MainLoginUI : IUserInterface
    {
        public static MainLoginUI Instance;
        UIInput name;
        public static bool HasShow;
        private void Awake()
        {
            Instance = this;
            HasShow = true;
            name = GetInput("AccountInput");
#if UNITY_ANDROID && !UNITY_EDITOR
            if (PlatformSdkManager.Instance.UserName == "0")
                name.value = "";
            else
                name.value = PlatformSdkManager.Instance.UserName;
            name.enabled = false;   
            name.collider.enabled = false;
#endif



            SetCallback("EnterGame", EnterGame);

#if UNITY_ANDROID && !UNITY_EDITOR
            SetCallback("SwitchAccount", PlatformSdkManager.Instance.SwitchUser);
            SetCallback("AccountInfo", PlatformSdkManager.Instance.ShowUserCenter);
            SetCallback("CallCenter", PlatformSdkManager.Instance.ShowCallCenter);
#else
#endif

            GetName("SwitchAccount").SetActive(false);
            GetName("AccountInfo").SetActive(false);
            GetName("CallCenter").SetActive(false);

            if (NetDebug.netDebug.TestAndroid)
            {
                name.value = "123";
                name.enabled = true;
                name.GetComponent<Collider>().enabled = true;
            }

            if (NetDebug.netDebug.IsTest)
            {
                var rid = Random.Range(0, 999);
                name.value = ""+rid;
                EnterGame();
            }
        }

        private bool pressYet = false;
        private IEnumerator CheckBlack()
        {
            if (pressYet)
            {
                yield break;
            }
            pressYet = true;
            yield return StatisticsManager.Instance.StartCoroutine(StatisticsManager.Instance.CheckBlack());
            if (StatisticsManager.Instance.IsClose)
            {
                pressYet = false;
                yield break;
            }
            if (string.IsNullOrEmpty(name.value))
            {
                Util.ShowMsg("帐号名不能为空,请先登录帐号");
                pressYet = false;
                yield break;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            StatisticsManager.Instance.StartCoroutine(StatisticsManager.Instance.CheckNewUser(name.value));
#else

            var role = RolesInfo.CreateBuilder();
            role.PlayerId = 1;
            role.Name = "test";
            role.Level = 1;
            role.Job = Job.WARRIOR;
            role.CreateTime = 0;

            SaveGame.saveGame.SetSelectChar(role.Build());
            StartCoroutine(StatisticsManager.Instance.LoginServer(name.value));
#endif
            pressYet = false;
        }

        private void EnterGame()
        {
            StartCoroutine(CheckBlack());
        }


        private void OnDestory()
        {
            Instance = null;
        }

        public void SetAccountName(string name)
        {
            this.name.value = name;
        }
    }
}
