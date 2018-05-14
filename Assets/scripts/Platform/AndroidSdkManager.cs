using UnityEngine;
using System;
using System.Text;
using MyLib;

public class AndroidSdkManager : PlatformSdkManager
{
#if UNITY_ANDROID && !UNITY_EDITOR

    private AndroidJavaObject m_mainActivity;

    void Start()
    {     
        Instance = this;
 
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        m_mainActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");   

        CheckUpdate();
        Login();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitSdk();
        }
    }
 
    public void ExitSdk()
    {
        m_mainActivity.Call("showExitDialog");
    }

    public void CheckUpdate()
    {
        m_mainActivity.Call("updateVersion");
    }
  
    public void OnCancel()
    {
 
    }

    public void LoginError()
    {

    }

    public override void Login()
    {
        m_mainActivity.Call("login");
    }

    public override void RoleLevelLog(string roleLevel, string serverId)
    {
        m_mainActivity.Call("logRoleLevel", roleLevel, serverId);
    }

    public override void CreateRoleLog(string roleId, string roleName, string serverId, string serverName)
    {
        m_mainActivity.Call("logCreateRole", roleId, roleName, serverId, serverName);
    }

    public override void SelectServerLog(string roleLevel, string userName, string serverId)
    {
        m_mainActivity.Call("logSelectServer", roleLevel, userName, serverId);
    }

    public override void EnterGameLog(string serverId, string roleName = "0", string roleLevel = "", string roleId = "", string serverName = "")
    {
        m_mainActivity.Call("logEnterGame", roleId, roleName, roleLevel, serverId, serverName);
    }

    public override void LoginGameFinishLog(string uid)
    {
        m_mainActivity.Call("logLoginFinish", uid);
    }

    public void SDKInitDone(string platformInfo)
    {
        var results = platformInfo.Split('_');
        PlatformName = results[0];
        PlatformID = results[1];
    }

    public void SDKInitFailed(string info)
    {
        
    }

    public void SDKLoginDoneName(string name)
    {
        UserName = name;
    }

    public void SDKLoginDoneUid(string uid)
    {
        Uid = uid;
    }

    public void SDKLoginDoneExt(string ext)
    {
        Ext = ext;
        InitYet = true;
        PlatformAccountLoginDone = true;
        if (MainLoginUI.Instance != null)
        {
            MainLoginUI.Instance.SetAccountName(UserName);
        }
        else if (MainLoginUI.HasShow)
        {
            Application.LoadLevel("MainLogin");
        }
    }

    public void SDKLoginFailed(string info)
    {
        PlatformAccountLoginDone = false;
    }

    public void SDKSwitchUserName(string name)
    {
        UserName = name;
    }

    public void SDKSwitchUserUid(string uid)
    {
        Uid = uid;
    }

    public void SDKSwitchUserExt(string ext)
    {
        Ext = ext;
        PlatformAccountLoginDone = true;
    
        WorldManager.worldManager.QuitWorldToLoginScene();

        if (MainLoginUI.Instance != null)
        {
            MainLoginUI.Instance.SetAccountName(UserName);
        }
        else if(MainLoginUI.HasShow)
        {
            MainLoginUI.Instance.SetAccountName(UserName);
            //Application.LoadLevel("MainLogin");
        }
    }

    public void SDKLogout(string info)
    {
        PlatformAccountLoginDone = false;
            
        UserName="0";
        Uid="0";
        Ext="{}";
        
        WorldManager.worldManager.QuitWorldToLoginScene();
    
        if (MainLoginUI.Instance != null)
        {
            MainLoginUI.Instance.SetAccountName("");
        }
        else if (MainLoginUI.HasShow)
        {
            MainLoginUI.Instance.SetAccountName("");
            //Application.LoadLevel("MainLogin");
        }

    }

    public override void ShowCallCenter()
    {

        m_mainActivity.Call("showCallCenter");
    }

    public override void ShowUserCenter()
    {
        m_mainActivity.Call("showUserCenter");
    }

    public override void SwitchUser()
    {
        m_mainActivity.Call("switchUser");
    }
#endif


}



