using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyLib;
using System;

/// <summary>
/// 向服务器同步网络数据
/// </summary>
public class MobaMeSyncToServer : MeSyncToServer {
    //private byte frameId = 0;
    private AvatarInfo lastInfo;
    private float lastSyncTime = 0;
    private MoveController mv;
    private NpcAttribute attr;
    private bool low = true;
    private void Start()
    {
        mv = GetComponent<MoveController>();
        attr = GetComponent<NpcAttribute>();
    }
    public override void InitData(AvatarInfo info)
    {
        lastInfo = AvatarInfo.CreateBuilder(info).Build();
    }
    /// <summary>
    /// 同步客户端位置给服务器
    /// </summary>
    public override void SyncPos()
    {
        Log.Net("SyncPos");
        var me = gameObject;
        var pos = me.transform.position;
        var dir = (int)me.transform.localRotation.eulerAngles.y;
        var intPos = NetworkUtil.ConvertPos(pos);

        var ainfo = AvatarInfo.CreateBuilder();
        ainfo.Id = GetComponent<NpcAttribute>().GetNetView().GetServerID();
        var change = false;
        //第一个报文用于建立UDP连接 在UDP连接没有建立起来一直走TCP
        /*
        if (frameId == 1)
        {
            change = true;
        }
        */

        var diffTime = Time.time - lastSyncTime;
        //防止UDP连接断开
       /*
        if (dir != lastInfo.Dir)
        {
            ainfo.Dir = dir;
            lastInfo.Dir = ainfo.Dir;
            change = true;
        }
        */

        if (mv != null)
        {
            //摇杆的操控命令 如何处理命令是服务器的问题
            var vcontroller = mv.vcontroller;
            var h = vcontroller.inputVector.x; //CameraRight 
            var v = vcontroller.inputVector.y; //CameraForward
            var camRight = CameraController.Instance.camRight;
            var camForward = CameraController.Instance.camForward;
            var targetDirection = h * camRight + v * camForward;
            var mdir = targetDirection.normalized;
            var speed = attr.GetSpeed();
            var sp = mdir * speed;
            var SpeedX = Util.GameToNetNum(sp.x);
            var SpeedY = Util.GameToNetNum(sp.z);
            Log.Sys("MoveSpeed:"+SpeedX+":"+SpeedY+":"+h+":"+v+":"+targetDirection+":"+camRight+":"+camForward);
            if (SpeedX != lastInfo.SpeedX || SpeedY != lastInfo.SpeedY)
            {
                ainfo.SpeedX = SpeedX;
                ainfo.SpeedY = SpeedY;
                lastInfo.SpeedX = ainfo.SpeedX;
                lastInfo.SpeedY = ainfo.SpeedY;
                change = true;
            }

            if (diffTime > 1 ||
           (intPos[0] != lastInfo.X || intPos[1] != lastInfo.Y || intPos[2] != lastInfo.Z 
           || dir != lastInfo.Dir 
           || SpeedX != lastInfo.SpeedX
           || SpeedY != lastInfo.SpeedY)
           )
            {
                lastSyncTime = Time.time;

                ainfo.X = intPos[0];
                ainfo.Y = intPos[1];
                ainfo.Z = intPos[2];
                ainfo.Dir = dir;
                ainfo.SpeedX = SpeedX;
                ainfo.SpeedY = SpeedY;

                change = true;

                lastInfo.X = ainfo.X;
                lastInfo.Y = ainfo.Y;
                lastInfo.Z = ainfo.Z;
                lastInfo.Dir = ainfo.Dir;

                lastInfo.SpeedX = ainfo.SpeedX;
                lastInfo.SpeedY = ainfo.SpeedY;
            }
        }

        if (change)
        {
            var cg = CGPlayerCmd.CreateBuilder();
            cg.Cmd = "UpdateData";

            cg.AvatarInfo = ainfo.Build();
            NetworkUtil.Broadcast(cg);
        }
    }
    public override void SyncAttribute()
    {
        Log.Net("SyncAttribute");
        var me = gameObject;
        var pos = me.transform.position;
        var dir = (int)me.transform.localRotation.eulerAngles.y;
        var meAttr = me.GetComponent<NpcAttribute>();
        var intPos = NetworkUtil.ConvertPos(pos);

        var ainfo = AvatarInfo.CreateBuilder();
        var change = false;

        var intNetSpeed = (int)(meAttr.NetSpeed * 100);
        if (intNetSpeed != lastInfo.NetSpeed)
        {
            ainfo.NetSpeed = intNetSpeed;
            change = true;

            lastInfo.NetSpeed = ainfo.NetSpeed;
        }
        var intThrowSpeed = (int)(meAttr.ThrowSpeed * 100);
        if (intThrowSpeed != lastInfo.ThrowSpeed)
        {
            ainfo.ThrowSpeed = intThrowSpeed;
            change = true;

            lastInfo.ThrowSpeed = ainfo.ThrowSpeed;
        }
        var intJumpSpeed = (int)(meAttr.JumpForwardSpeed * 100);
        if (intJumpSpeed != lastInfo.JumpForwardSpeed)
        {
            ainfo.JumpForwardSpeed = intJumpSpeed;
            change = true;

            lastInfo.JumpForwardSpeed = ainfo.JumpForwardSpeed;
        }

        if (!string.Equals(meAttr.userName, lastInfo.Name))
        {
            ainfo.Name = meAttr.userName;
            change = true;
            lastInfo.Name = ainfo.Name;
        }

        if (lastInfo.Job != ServerData.Instance.playerInfo.Roles.Job)
        {
            ainfo.Job = ServerData.Instance.playerInfo.Roles.Job;
            change = true;
            lastInfo.Job = ainfo.Job;
        }

        if (change)
        {
            var cg = CGPlayerCmd.CreateBuilder();
            cg.Cmd = "UpdateData";
            cg.AvatarInfo = ainfo.Build();
            NetworkUtil.Broadcast(cg);
        }
    }
  
}
