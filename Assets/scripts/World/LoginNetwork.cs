using KBEngine;
using UnityEngine;
using System.Collections;
using MyLib;
using MonoBehaviour = UnityEngine.MonoBehaviour;

public class LoginNetwork : MonoBehaviour
{
    public static RemoteClient RemoteConnection;
    public static LoginNetwork Instance;
    MainThreadLoop ml;

    // Use this for initialization
    void Awake ()
	{
	    Instance = this;
        ml = gameObject.AddComponent<MainThreadLoop>();
    }

    public void LoginGame(string ip,string account)
    {
        StartCoroutine(Login(ip,account));
    }

    IEnumerator Login(string ip, string account)
    {
        yield return StartCoroutine(InitData(ip, account));
    }

    IEnumerator InitData(string ip, string account)
    {
        if (RemoteConnection == null)
        {
            RemoteConnection = new RemoteClient(ml);
            RemoteConnection.evtHandler = EvtHandler;
            RemoteConnection.msgHandler = MsgHandler;

            RemoteConnection.Connect(ip, ClientApp.Instance.remotePort);
        }


        var cg = CGPlayerCmd.CreateBuilder();
        cg.Cmd = "Login2";
        cg.Account = account;

        Bundle bundle;        
        var data = KBEngine.Bundle.GetPacketFull(cg, out bundle);
        yield return StartCoroutine(RemoteConnection.SendWaitResponse(data.data, data.fid, (packet) =>
        {
            var proto = packet.protoBody as GCPlayerCmd;
            var cmds = proto.Result.Split(' ');
            Debug.LogError(proto);
        }, bundle));
    }

    void EvtHandler(RemoteClientEvent evt)
    {
 
    }

    void MsgHandler(KBEngine.Packet packet)
    {
 
    }
}
