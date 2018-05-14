using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using MyLib;
using KBEngine;
using System.Threading;

/// <summary>
/// 建立连接
/// 连接超时断开 当发送队列长度超过一定值一段时间 要么是overflow了页面是ack不能了
/// 发送
/// 接受 
/// </summary>
public class KCPClient {
    private IPEndPoint remoteEnd;
    private KCP kcp;
    private SimpleUDPClient udpClient;
    private MessageReader msgReader;
    public IMainLoop ml;
    private NetworkScene networkScene;

    public bool IsClose = false;
    public System.Action closeEventHandler;

    public KCPClient(IMainLoop mainLoop, NetworkScene ns) {
        kcp = new KCP();
        kcp.outputFunc = this.SendKCPPacket;
        kcp.closeEventHandler = this.KCPClosed;
        //KCP 逻辑处理是在主线程上执行的

        msgReader = new MessageReader();
        msgReader.mainLoop = mainLoop;
        msgReader.msgHandle = HandleMsg;
        ml = mainLoop;
        networkScene = ns;

        ml.queueInUpdate(Update);

    }
    //可复用网络对象的重置
    //或者上层重新分配新的对象
    private void ResetNet() {
        Connected = false; 
        IsClose = true;
        ml.removeUpdate(this.Update);
    }
    //KCP 发起重连尝试
    //业务逻辑层重新初始化
    private void KCPClosed() {
        ResetNet();
        if(closeEventHandler != null) {
            closeEventHandler();
        }
    }

    //主线程执行
    private void HandleMsg(KBEngine.Packet packet) {
        var gc = packet.protoBody as GCPlayerCmd;
        if(gc.Result == "TestKCP" && !IsClose) {
            Connected = true;
        }else {
            networkScene.MsgHandler(packet);
        }
    }

    public void Connect(string ip, int port ) {
        remoteEnd = new IPEndPoint(IPAddress.Parse(ip), port);
        Log.Net("KCPREmoteEnd: "+remoteEnd);
        udpClient = new SimpleUDPClient(remoteEnd, this);
        ClientApp.Instance.StartCoroutine(TestConnect());
    }
  
    /// <summary>
    ///上层控制关闭 
    /// </summary>
    public void Close() {
        ResetNet();
        kcp.CloseKCP();
    }

    //KCP-->应用层
    private void  Update() {
        //Log.Net("KCPUpdate");
        kcp.Update(Time.deltaTime);
        while(true){
            var pack = kcp.Recv();
            if(pack != null) {
                msgReader.process(pack, (uint)pack.Length, null);
            }else {
                break;
            }
        }
    }

    //应用层--->KCP
    public void SendPacket(CGPlayerCmd.Builder cg) {
        Bundle bundle;
        var data = Bundle.GetPacketFull(cg, out bundle);
        Bundle.ReturnBundle(bundle);
        var pkData = data.data; 
        this.SendPacketData(pkData);
    }
    //KCP--->UDP
    private void SendKCPPacket(KCPPacket kcpPacket) {
        //this.SendPacketData(kcpPacket.fullData);
        udpClient.SendData(kcpPacket.fullData);
    }

    //应用层--->KCP
    private void SendPacketData(byte[] data) {
        kcp.Send(data);
    }

    //UDP-->KCP
    public void ReceiveData(byte[] data) {
        ml.queueInLoop(()=>{
            kcp.Input(data, data.Length);
        });
    }


    public bool Connected = false; 
    private IEnumerator TestConnect() {
        var cg = CGPlayerCmd.CreateBuilder();
        cg.Cmd = "TestKCP";
        var avtar = AvatarInfo.CreateBuilder();
        var myId = NetMatchScene.Instance.myId;
        avtar.Id = myId;
        cg.AvatarInfo = avtar.Build();
        Bundle bundle;
        var data = Bundle.GetPacketFull(cg, out bundle);
        Bundle.ReturnBundle(bundle);
        var pkData = data.data;
        SendPacketData(pkData);

        var waitTime = 0.0f;
        Log.Net("KCP TestBegin");
        var askTime = 0.0f;
       
        while (!Connected && waitTime < 10)
        {
            yield return null;
            waitTime += Time.deltaTime;
            askTime += Time.deltaTime;
            if(askTime >= 1f) {
                askTime = 0;
                SendPacketData(pkData);
            }
        }

        waitTime = 0;
        if(Connected) {
            Log.Net("KCP TestSuc");
            var cg1 = CGPlayerCmd.CreateBuilder();
            cg1.Cmd = "KCPConnect";
            NetworkUtil.Broadcast(cg1);
        }else {
            Close();
            Log.Net("KCP Test Fail");
        }
    }
}
