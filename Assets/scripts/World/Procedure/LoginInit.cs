
/*
Author: liyonghelpme
Email: 233242872@qq.com
*/

/*
Author: liyonghelpme
Email: 233242872@qq.com
*/
using UnityEngine;
using System.Collections;
using KBEngine;
using Google.ProtocolBuffers;

namespace MyLib
{

	public class LoginInit : UnityEngine.MonoBehaviour
	{
		static LoginInit loginInit;

	    public bool testNormalLogin;

		public static LoginInit GetLogin() {
			return loginInit;
		}
		void Awake ()
		{
			loginInit = this;

			if (SaveGame.saveGame == null) {
				var saveGame = new GameObject("SaveGame");
				saveGame.AddComponent<SaveGame>();
				saveGame.GetComponent<SaveGame>().InitData();
				saveGame.GetComponent<SaveGame>().InitServerList();
			}
		    var sd = new ServerData();
            sd.LoadData();
		}



		void Start ()
		{
            WindowMng.windowMng.PushView("UI/loginUI2");
            MyEventSystem.myEventSystem.PushEvent(MyEvent.EventType.UpdateLogin);
		}
	}

}
