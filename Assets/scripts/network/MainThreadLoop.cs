using System.Threading;
using MyLib;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public interface IMainLoop {
    //在主线程上执行
    void queueInLoop(Action cb);
    //在主线程上循环执行
    void queueInUpdate(Action cb);
    void removeUpdate(Action cb);
}

public class MainThreadLoop : MonoBehaviour, IMainLoop
{
    private Queue<Action> pendingCallbacks = new Queue<Action>();
    private List<Action> updateCb = new List<Action>();
    private List<Action> removeCb = new List<Action>();
    //KCP 客户端连接为什么总收不到服务器的ACK ??断开连接了
    private List<Action> tobeAdd = new List<Action>();

    public void queueInLoop(System.Action cb)
    {
        lock (pendingCallbacks)
        {
            pendingCallbacks.Enqueue(cb);
        }
    }
    //非线程安全
    public void queueInUpdate(System.Action cb)
    {
        tobeAdd.Add(cb);
        //updateCb.Add(cb);
    }
    public void removeUpdate(System.Action cb)
    {
        //updateCb.Remove(cb);
        removeCb.Add(cb);
    }

    private double lastUpdateTime = 0;
    private void Update()
    {
        lock (pendingCallbacks)
        {
            while (pendingCallbacks.Count > 0)
            {
                var p = pendingCallbacks.Dequeue();
                try
                {
                    p();
                }
                catch (Exception e)
                {
                    Debug.LogError("CallBacks: " + e.ToString());
                }
            }
        }
        foreach (var c in updateCb)
        {
            try
            {
                c();
            }
            catch (Exception exp)
            {
                Debug.LogError("UpdateCB: " + exp.ToString());
            }
        }
        //删除安全
        foreach (var c in removeCb)
        {
            updateCb.Remove(c);
        }
        removeCb.Clear();
        foreach(var c in tobeAdd)
        {
            updateCb.Add(c);
        }
        tobeAdd.Clear();
    }

}
