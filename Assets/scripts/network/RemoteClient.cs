using KBEngine;
using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System;
using System.Threading;
using System.Collections.Generic;

namespace MyLib
{
    public class MsgBuffer
    {
        public int position = 0;
        public byte[] buffer;
        public Bundle bundle;

        public int Size
        {
            get
            {
                return buffer.Length - position;
            }

        }
    }

    public enum RemoteClientEvent
    {
        None,
        Connected,
        Close,
    }

    public class RemoteClient
    {
        byte[] mTemp = new byte[8192];
        KBEngine.MessageReader msgReader = new KBEngine.MessageReader();

        //Socket mSocket;
        private TcpClient client;
        IPEndPoint endPoint;
        public bool IsClose = false;
        private Queue<MsgBuffer> msgBuffer = new Queue<MsgBuffer>();
        public KBEngine.MessageHandler msgHandler;
        public System.Action<RemoteClientEvent> evtHandler;

        private ThreadSafeDic flowHandler = new ThreadSafeDic();

        private IMainLoop ml;
        private Thread sendThread;
        private Thread recvThread;

        public IMainLoop GetMainLoop()
        {
            return ml;
        }
        public RemoteClient(IMainLoop loop)
        {
            msgReader.msgHandle = HandleMsg;
            msgReader.mainLoop = loop;
            ml = loop;
        }


        private ManualResetEvent signal = new ManualResetEvent(false);
        private void SendThread()
        {
            while (!IsClose && !callCloseYet)
            {
                //var tn = Thread.CurrentThread.ManagedThreadId;
                //Debug.LogError("ThreadN: "+tn);
                //var st = Util.GetTimeNow();

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
                        mb = msgBuffer.Dequeue();
                    }
                }
                if (mb != null)
                {
                    try
                    {
                        client.GetStream().Write(mb.buffer, mb.position, mb.Size);
                        /*
                        ml.queueInLoop(() =>
                        {
                            Bundle.ReturnBundle(mb.bundle);
                        });
                        */
                    }
                    catch (Exception exception)
                    {
                        callCloseYet = true;
                        ml.queueInLoop(() => {
                            Debug.LogError(exception.ToString());
                            Close();
                        });
                    }
                }

                lock (msgBuffer)
                {
                    if (msgBuffer.Count <= 0)
                    {
                        signal.Reset();
                    }
                }
                //var et = Util.GetTimeNow();
                //Debug.LogError("DiffTime: "+(et-st));
            }
        }

        /// <summary>
        /// 当消息处理器已经退出场景则关闭网络连接 
        /// </summary>
        /// <param name="packet">Packet.</param>
        void HandleMsg(KBEngine.Packet packet)
        {
            //Debug.LogError("HandlerMsg "+packet.protoBody);
            Log.Net("HandlerMsg " + packet.protoBody);

            if (msgHandler != null)
            {
                msgHandler(packet);
            } else
            {
                Close();
            }
        }

        public void Connect(string ip1, int port1)
        {
            NetDebug.netDebug.AddMsg("Connect: "+ip1+" port "+port1);
            endPoint = new IPEndPoint(IPAddress.Parse(ip1), port1);
            try
            {
                client = new TcpClient();
                client.NoDelay = true;
                client.SendBufferSize = 1024;
                client.SendTimeout = 5;

                var result = client.BeginConnect(endPoint.Address, endPoint.Port, OnConnectResult, null);
                var th = new Thread(CancelConnect);
                th.Start(result);
            } catch (Exception exception)
            {
                Debug.LogError(exception.Message);
                Close();
            }
        }
        private bool callCloseYet = false;

        private void StartReceive()
        {
            //Debug.LogError("curThread: "+Thread.CurrentThread.ManagedThreadId);
            //Debug.LogError("StartReceive");
            try
            {
                //client.GetStream().BeginRead(mTemp, 0, mTemp.Length, OnReceive, null);
                var num = client.GetStream().Read(mTemp, 0, mTemp.Length);
                OnReceive2(num);
            } catch (Exception exception)
            {
                callCloseYet = true;
                ml.queueInLoop(() =>
                {
                    Debug.LogError(exception.ToString());
                    Close();
                });
            }
        }

        private void OnReceive2(int bytes)
        {
            //Debug.LogError("curThread: "+Thread.CurrentThread.ManagedThreadId);
            //var st = Util.GetTimeNow(); 
            if (bytes <= 0 || client == null || !client.Connected)
            {
                Close();
                return;
            }

            uint num = (uint) bytes;
            msgReader.process(mTemp, num, flowHandler);
        }


        void OnConnectResult(IAsyncResult result)
        {
            if (client == null)
            {
                return;
            }
            bool success = false;
            try
            {
                //mSocket.EndConnect(result);
                client.EndConnect(result);
                success = true;

            } catch (Exception exception)
            {
                Debug.LogError(exception.Message);
                success = false;
            }
            if (success)
            {
                Debug.LogError("Connect Success");

                sendThread = new Thread(SendThread);
                sendThread.Start();
                recvThread = new Thread(RecvThread);
                recvThread.Start();
                this.ml.queueInLoop(()=>{
                    SendEvt(evtHandler, RemoteClientEvent.Connected);
                });
            } else
            {
                Close();
            }
        }

        //事件的Evt处理机制已经删除掉了
        private void SendEvt(Action<RemoteClientEvent> ehandler, RemoteClientEvent evt)
        {
            Debug.LogError("SendEvt: " + evt);
            if (ehandler != null)
            {
                var eh = ehandler;
                eh(evt);
            }
            else
            {
                Close();
            }
        }

        public void Disconnect()
        {
            Close();
        }

        //线程安全方法
        void Close()
        {
            if (IsClose)
            {
                return;
            }
            Debug.LogError("CloseRemoteClient");
            if (client != null)
            {
                try
                {
                    if (client != null && client.Connected)
                    {
                        client.Close();
                    }
                } catch (Exception exception)
                {
                    Debug.LogError(exception.ToString());
                }
            }
            client = null;
            IsClose = true;
            signal.Set();

            if (evtHandler != null)
            {
                var eh = evtHandler;
                ml.queueInLoop(() =>
                {
                    SendEvt(eh, RemoteClientEvent.Close);
                });
            }
            evtHandler = null;
            msgHandler = null;
        }

        void CancelConnect(object obj)
        {
            var res = (IAsyncResult)obj;
            if (res != null && !res.AsyncWaitHandle.WaitOne(3000))
            {
                Debug.LogError("ConnectError");
                Close();
            } else
            {
                //ml.queueInLoop(StartReceive);
                //启动线程进行数据接收
            }
        }

        private void RecvThread()
        {
            while (!IsClose && !callCloseYet)
            {
                StartReceive();
            }
        }

        public IEnumerator SendWaitResponse(byte[] data, uint fid, KBEngine.MessageHandler handler, Bundle bundle) {
            var ret = false;
            flowHandler.Add(fid, (packet)=>{
                handler(packet);
                ret = true;
            });
            Send(data, bundle);
            //float passTime = 0;
            while(!ret && !IsClose) {
                yield return null;
                //passTime += Time.deltaTime;
            }
            if (!ret)
            {
                Debug.LogError("TimeOutRequest: "+fid+" h "+handler);
                var packet = new Packet();
                packet.responseFlag = 1;
                handler(packet);
            }
        }

        public void Send(byte[] data, Bundle bundle)
        {
            lock (msgBuffer)
            {
                var mb = new MsgBuffer() {position = 0, buffer = data, bundle = null};
                Bundle.ReturnBundle(bundle);
                msgBuffer.Enqueue(mb);
            }
            signal.Set();
        }

    }

}