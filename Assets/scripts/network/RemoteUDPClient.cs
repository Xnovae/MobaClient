using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using KBEngine;
using UnityEngine;
using System.Collections;
using MyLib;


/// <summary>
/// 向服务器同步玩家位置然后服务器使用TCP信道的Update函数来更新客户端位置
/// 后面使用UDP来做服务器的通知
/// 创建一个连接 lastAck lastSend
///  下一个要Send的时候lastAck 做比较得到的delta
///  服务器会发送ACK给客户端
///  客户端会接受服务器的ACK
/// 
/// 
/// 1：NetworkScene TCP 连接服务器成功， 数据先走TCP通道
/// 2：和服务器建立UDP连接
/// 3: 同步位置和朝向以及towerDir 给远程服务器
/// 4：接受远程服务器 发送过来的方向同步信息
/// </summary>
public class RemoteUDPClient
{
    //移动协议不保证可靠性
    //客户端向服务器发送最新的 Move位置朝向信息
    //服务器向客户端也只发送最新的信息

    //需要确保最后一个状态是发送过去的
    //发送了报文 FrameID 服务器确认FrameID
    
    //TODO:如果不考虑可靠性，允许一个最新位置状态没有广播成功，后面加验证
    private int lastMaxFlowId = -1;
    //private byte lastSendFlowId;
    //private AvatarInfo lastAck;

    //客户端只要最新的UDP报文
    private UdpClient client;
    private List<MsgBuffer> msgBuffer = new List<MsgBuffer>();
    private MessageReader msgReader = new MessageReader();
    private IMainLoop ml;
    private NetworkScene networkScene;

    private Thread sendThread;
    private Thread recvThread;

    public RemoteUDPClient(IMainLoop loop, NetworkScene ns)
    {
        msgReader.msgHandle = HandleMsg;
        msgReader.mainLoop = loop;
        ml = loop;
        networkScene = ns;
        Log.Net("Init UDP: "+loop);
    }

    public bool Connected = false;
    private void HandleMsg(Packet packet)
    {
        Log.Net("UDP Receive: "+packet.protoBody.ToString());
        var gc = packet.protoBody as GCPlayerCmd;
        if (gc.Result == "TestUDP" && !IsClose)
        {
            Connected = true;
        }
        else
        {
            networkScene.MsgHandler(packet);
        }
    }

    private IPEndPoint endPoint;
    public void Connect(string ip, int port, int listenPort)
    {
        endPoint = new IPEndPoint(IPAddress.Parse(ip), port);

        client = new UdpClient(0);
        var rp = new IPEndPoint(IPAddress.Any, 0);
        recvThread = new Thread(RecvThread);
        recvThread.Start();

        Log.Net("UDP Connect: "+ip+" port "+port+" listenPort: "+" cp "+client.Client.LocalEndPoint);
        NetDebug.netDebug.AddMsg("UDP Port: "+client.Client.LocalEndPoint);
        sendThread = new Thread(SendThread);
        sendThread.Start();

        ClientApp.Instance.StartCoroutine(TestConnect());
    }
    private bool callCloseYet = false;
    private void RecvThread()
    {
        while (!IsClose && !callCloseYet)
        {
            StartReceive();
        }
    }

    private ManualResetEvent signal = new ManualResetEvent(false);

    private void SendThread()
    {
        while (!IsClose)
        {
            signal.WaitOne();
            if (IsClose)
            {
                break;
            }


            MsgBuffer mb = null;
            lock (msgBuffer)
            {
                if (msgBuffer.Count > 0)
                {
                    mb = msgBuffer[0];
                    msgBuffer.RemoveAt(0);
                }
            }
            if (mb != null)
            {

                try
                {
                    //client.BeginSend(mb.buffer, mb.buffer.Length, endPoint, OnSend, null);
                    client.Send(mb.buffer, mb.buffer.Length, endPoint);
                }
                catch (Exception exp)
                {
                    Debug.LogError(exp.ToString());
                    Close();
                }
            }

            lock (msgBuffer)
            {
                if (msgBuffer.Count <= 0)
                {
                    signal.Reset();
                }
            }
        }
    }

    private void StartReceive()
    {
        var udpPort = new IPEndPoint(IPAddress.Any, 0);
        try
        {
            var bytes = client.Receive(ref udpPort);
            ReceiveData(bytes);
        }
        catch (Exception exp)
        {
            callCloseYet = true;
            ml.queueInLoop(() =>
            {
                Debug.LogError(exp.ToString());
                Close();
            });
        }
    }

    private void ReceiveData(byte[] bytes)
    {
        receiveYet = true;
        lastReceiveTime = Util.GetTimeNow();
        if (bytes.Length > 0)
        {
            msgReader.process(bytes, (uint) bytes.Length, null);
        }
        else
        {
            Close();
        }
    }

    public double lastReceiveTime = 0;
    public bool receiveYet = false;

    private IEnumerator TestConnect()
    {
        var cg = CGPlayerCmd.CreateBuilder();
        cg.Cmd = "TestUDP";
        var avtar = AvatarInfo.CreateBuilder();
        var myId = NetMatchScene.Instance.myId;
        avtar.Id = myId;
        cg.AvatarInfo = avtar.Build();

        SendPacket(cg);
        var waitTime = 0.0f;
        Log.Net("UDP TestBegin");
        while (!Connected && waitTime < 10)
        {
            yield return null;
            waitTime += Time.deltaTime;
        }
        Log.Net("UDP Connnect: " + Connected);
        if (Connected)
        {
            var cg1 = CGPlayerCmd.CreateBuilder();
            cg1.Cmd = "UDPConnect";
            NetworkUtil.Broadcast(cg1);
            Util.ShowMsg("开始使用UDP同步");
        }
        else
        {
            Close();
            Util.ShowMsg("UDP测试失败，使用TCP通信");
        }
    }

    public void SendPacket(CGPlayerCmd.Builder cg)
    {
        Log.Net("UDP Send: "+cg.ToString());
        Bundle bundle;
        var data = Bundle.GetPacketFull(cg, out bundle);
        Bundle.ReturnBundle(bundle);
        var mb = new MsgBuffer()
        {
            position = 0,
            buffer = data.data,
            bundle = null,
        };
        lock (msgBuffer)
        {
            msgBuffer.Add(mb);
        }
        signal.Set();
        
    }

    

    public bool IsClose = false;
    private int refCount = 0;
    private void Close()
    {
        if (Interlocked.Increment(ref refCount) != 1)
        {
            return;
        }

        if (IsClose)
        {
            return;
        }
        IsClose = true;
        Debug.LogError("UDP Close: "+client.Client.LocalEndPoint);
        try
        {
            client.Close();
        }
        catch (Exception exp)
        {
            Debug.LogError("UDP Exception: " + exp.ToString());
        }
        client = null;
        Connected = false;
        signal.Set();
    }

    public void Quit()
    {
        Close();
    }
}
