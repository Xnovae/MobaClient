using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System;
namespace MyLib
{
	public class GameInterface_Login
	{
		public static GameInterface_Login loginInterface = new GameInterface_Login();
		public JSONArray GetServerList() {
			return SaveGame.saveGame.serverList;
		}
		public int GetSaveAccountNum() {
			return SaveGame.saveGame.otherAccounts.Count;
		}

		public void LoginGame ()
		{
            Application.LoadLevel("MainLogin");
		}

		public void SelectAccounAndLogin (int currentSelect)
		{
			throw new NotImplementedException ();
		}




		public GCLoginAccount GetCharInfo() {
			//return LoginInit.loginInit.GetCharInfo ();
			return SaveGame.saveGame.charInfo;
		}


		public void StartGame (RolesInfo roleInfo)
		{
			CharSelectProgress.charSelectLogic.StartGame (roleInfo);
		}

	}

}
