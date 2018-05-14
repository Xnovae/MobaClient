using System.Net.NetworkInformation;
using UnityEngine;
using System.Collections;

public class PlatformSdkManager : MonoBehaviour {
    
    public static PlatformSdkManager Instance { get; set;}

    public string PlatformName = "PC";

    public string UserName="0";
    public string Uid="0";
    public string Ext="{}";
    public string DeviceID;
    public string PlatformID = "0";

    public bool InitYet = false;

    public bool PlatformAccountLoginDone = false;
    void Awake()
    {
        Instance = this;
        DeviceID = SystemInfo.deviceUniqueIdentifier;
        Debug.LogError("DeviceID: "+DeviceID);
#if UNITY_EDITOR || UNITY_STANDALONE
        InitYet = true;
#endif
    }

    public virtual void RoleLevelLog(string roleLevel, string serverId)
    {
 
    }

    public virtual void ShowCallCenter()
    {

    }

    public virtual void ShowUserCenter()
    {
        
    }

    public virtual void SwitchUser()
    {
        
    }

    public virtual void CreateRoleLog(string roleId, string roleName, string serverId, string serverName)
    {

    }

    public virtual void SelectServerLog(string roleLevel, string userName, string serverId)
    {
        
    }

    public virtual void EnterGameLog(string serverId, string roleName = "0", string roleLevel = "", string roleId = "", string serverName = "")
    {

    }

    public virtual void LoginGameFinishLog(string uid)
    {

    }

    public virtual void Login()
    {

    }
}
