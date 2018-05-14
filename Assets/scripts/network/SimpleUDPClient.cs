using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;


public class SimpleUDPClient {
    private UdpClient udpClient;
    private Thread sendThread;
    private Thread recvThread;

    private Queue<byte[]> sendQueue = new Queue<byte[]>();
    private IPEndPoint remoteEnd;

    private KCPClient client;
    public SimpleUDPClient(IPEndPoint ip, KCPClient k) {
        remoteEnd = ip;
        client = k;
        udpClient = new UdpClient(0);
        recvThread = new Thread(RecvThread);
        recvThread.Start();
        sendThread = new Thread(SendThread);
        sendThread.Start();
    }

    private void RecvThread() {
        var udpPort = new IPEndPoint(IPAddress.Any, 0);
        while(!IsClose) {
            try{
                var bytes = udpClient.Receive(ref udpPort);
                client.ReceiveData(bytes);
            }catch(Exception exp) {
                client.ml.queueInLoop(() =>
                {
                    Debug.LogError(exp.ToString());
                });
                Close();
            }
        }
    }
    public void SendData(byte[] data) {
        lock(sendQueue) {
            Log.Net("SendKCPData");
            sendQueue.Enqueue(data);
        }
        signal.Set();
    }

    private void Close() {
        signal.Set();
    }

    private ManualResetEvent signal = new ManualResetEvent(false);
    private bool IsClose = false;
    private void SendThread() {
        while (!IsClose)
        {
            signal.WaitOne();
            if (IsClose)
            {
                break;
            }


            byte[] mb = null;
            lock (sendQueue)
            {
                if (sendQueue.Count > 0)
                {
                    mb = sendQueue.Dequeue();
                }
            }
            if (mb != null)
            {
                try
                {
                    udpClient.Send(mb, mb.Length, remoteEnd);
                }
                catch (Exception exp)
                {
                    client.ml.queueInLoop(() =>
                    {
                        Debug.LogError(exp.ToString());
                    });
                    Close();
                }
            }

            lock (sendQueue)
            {
                if (sendQueue.Count <= 0)
                {
                    signal.Reset();
                }
            }
        }
    }
	
}
