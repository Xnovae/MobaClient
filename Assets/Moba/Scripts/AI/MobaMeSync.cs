using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyLib;
using System;

/// <summary>
/// 服务器向自己发送Add命令 同步自己的属性
/// </summary>
public class MobaMeSync : ISyncInterface {
    private List<AvatarInfo> positions = new List<AvatarInfo>();

    public override void InitSetPos(Vector3 pos)
    {
        throw new NotImplementedException();
    }

    public override Vector3 GetCurInfoPos()
    {
        return MobaUtil.FloatPos(curInfo);
    }
    public override Vector3 GetCurInfoSpeed()
    {
        return Util.NetToGameVec(new Vector3(curInfo.SpeedX, 0, curInfo.SpeedY));
    }

    public override MyVec3 GetServerVelocity()
    {
        if(positions.Count > 0)
        {
            var p1 = positions[positions.Count - 1];
            return new MyVec3(p1.SpeedX, 0, p1.SpeedY);
        }
        return MyVec3.zero;
    }
    //迭代加权平滑 弹簧 物理运动平滑手段
    private Vector3 GetInfoPredictPos(AvatarInfo p1)
    {
        var passTime = NetworkScene.Instance.GetPredictPassServerTime(p1.FrameID);
        var speed = NetworkUtil.FloatPos(p1.SpeedX, 0, p1.SpeedY);
        if(passTime > 0)
        {
            return MobaUtil.FloatPos(p1) + speed * (passTime+Util.FrameSecTime);
        }else
        {
            return MobaUtil.FloatPos(p1)+ speed * Util.FrameSecTime;
        }
    }
    private Vector3 GetAllAvgServerPos()
    {
        var sum = Vector3.zero;
        foreach(var p in positions)
        {
            var sp = GetInfoPredictPos(p);
            sum += sp;
        }
        sum /= positions.Count;
        return sum;
    }

    //预测其它玩家的控制位置
    //服务器上的FrameId 都是整数
    //客户端的FrameId是浮点数
    //计算得到这一阵结束时候的位置
    public override Vector3 GetServerPos()
    {
        if (NetworkScene.Instance != null)
        {
            if(positions.Count > 0)
            {
                return MobaUtil.FloatPos(curInfo);
            }
            else
            {
                return MobaUtil.FloatPos(curInfo);
            }
        }else
        {
            return transform.position;
        }
    }


    public AvatarInfo curInfo
    {
        get;
        set;
    }
    private NpcAttribute attr;
    private AIBase aiBase;
    private VirtualController vcontroller;
    private void Awake()
    {
        curInfo = AvatarInfo.CreateBuilder().Build();
    }

  
    private void Start()
    {
        attr = GetComponent<NpcAttribute>();
        aiBase = GetComponent<AIBase>();
        vcontroller = GetComponent<MoveController>().vcontroller;

        gameObject.AddComponent<DebugServerPos>();
    }

    public override void InitSync(AvatarInfo info)
    {
        if (info != null)
        {
            var gc = GCPlayerCmd.CreateBuilder();
            gc.AvatarInfo = info;
            var cmd = gc.Build();
            NetworkAttribute(cmd);
        }
    }

    public override void AddFakeMove()
    {
        var h = vcontroller.inputVector.x;//CameraRight 
        var v = vcontroller.inputVector.y;//CameraForward
        var camRight = CameraController.Instance.camRight;
        var camForward = CameraController.Instance.camForward;
        var targetDirection = h * camRight + v * camForward;
        var mdir = targetDirection.normalized;

    }
    private void NetworkMove(AvatarInfo info)
    {
        if(info.HasSpeedX)
        {
            curInfo.SpeedX = info.SpeedX;
            curInfo.SpeedY = info.SpeedY;
        }

        if (info.HasX)
        {
            Log.Net("NetMove:"+curInfo.Z+":"+NetworkScene.Instance.GetSyncCmdFrame()+":"+curInfo.SpeedY);
            curInfo.X = info.X;
            curInfo.Y = info.Y;
            curInfo.Z = info.Z;
            info.FrameID = NetworkScene.Instance.GetSyncCmdFrame();
            positions.Add(info);
            if (positions.Count > 5)
            {
                positions.RemoveAt(0);
            }

        }else
        {
        }

        if (info.HasDir)
        {
            curInfo.Dir = info.Dir;
        }
    }
    public override void NetworkAttribute(GCPlayerCmd cmd)
    {
        var info = cmd.AvatarInfo;
        if (attr == null)
        {
            attr = GetComponent<NpcAttribute>();
        }

        Log.Net("MySelfSync: " + info);
        if (info == null)
        {
            Debug.LogError("NetMatchInfo is Null ");
            return;
        }
        NetworkMove(info);

        //服务器通知瞬间传送位置
        if(info.ResetPos)
        {
            if (aiBase != null)
            {
                aiBase.GetAI().ChangeState(AIStateEnum.IDLE);
            }

            var netPos = MobaUtil.FloatPos(info);
            transform.position = netPos;
            positions.Clear();
        }


        if (info.HasTeamColor)
        {
            attr.SetTeamColorNet(info.TeamColor);
            curInfo.TeamColor = info.TeamColor;
        }
        if (info.HasIsMaster)
        {
            attr.SetIsMasterNet(info.IsMaster);
            curInfo.IsMaster = info.IsMaster;
        }

        var dataChanged = false;
        var skillCount = curInfo.KillCount;
        if (info.HasKillCount)
        {
            skillCount = info.KillCount;
            curInfo.KillCount = info.KillCount;
            dataChanged = true;
        }

        var deadCount = curInfo.DeadCount;
        if (info.HasDeadCount)
        {
            deadCount = info.DeadCount;
            curInfo.DeadCount = info.DeadCount;
            dataChanged = true;
        }

        var secondaryAttackCount = curInfo.SecondaryAttackCount;
        if (info.HasSecondaryAttackCount)
        {
            secondaryAttackCount = info.SecondaryAttackCount;
            curInfo.SecondaryAttackCount = info.SecondaryAttackCount;
            dataChanged = true;
        }

        if (info.HasScore)
        {
            curInfo.Score = info.Score;
            dataChanged = true;
        }

        if (dataChanged)
        {
            var sid = attr.GetNetView().GetServerID();
            ScoreManager.Instance.NetSyncScore(sid, curInfo.Score, sid, skillCount, deadCount, secondaryAttackCount);
        }

        if (info.HasContinueKilled)
        {
            curInfo.ContinueKilled = info.ContinueKilled;
            if (curInfo.ContinueKilled > 0)
            {
            }
        }
        if (info.HasPlayerModelInGame && curInfo.PlayerModelInGame != info.PlayerModelInGame)
        {
            curInfo.PlayerModelInGame = info.PlayerModelInGame;
            GetComponent<MobaModelLoader>().LoadModel(curInfo.PlayerModelInGame);
            var unitData = Util.GetUnitData(true, curInfo.PlayerModelInGame, 0);
            attr.SetObjUnitData(unitData);
            SkillDataController.skillDataController.InitSkillShotAfterSelectSkill(curInfo.PlayerModelInGame);
        }
        if (info.HasHP)
        {
            curInfo.HP = info.HP;
            attr.SetHPNet(info.HP);
        }
        if (info.HasLevel)
        {
            curInfo.Level = info.Level;
            attr.ChangeLevel(info.Level);
        }
        if(info.HasState)
        {
            curInfo.State = info.State;
        }

        if(info.HasGold)
        {
            curInfo.Gold = info.Gold;

            var evt = new MyEvent(MyEvent.EventType.UpdateItem);
            MyEventSystem.myEventSystem.PushEvent(evt);
        }

        if (info.HasItemInfoDirty)
        {
            curInfo.ItemInfoList = info.ItemInfoList;
            var evt = new MyEvent(MyEvent.EventType.UpdateItem);
            MyEventSystem.myEventSystem.PushEvent(evt);
        }
   }

    public override void NetworkAttack(GCPlayerCmd proto)
    {
        MobaUtil.DoNetworkAttack(gameObject, proto);
    }
    public override void NetworkBuff(GCPlayerCmd gc)
    {
    }
    public override void NetworkRemoveBuff(GCPlayerCmd cmd)
    {
    }

    public override void Revive(GCPlayerCmd gc)
    {
        //throw new NotImplementedException();
        var ai = GetComponent<AIBase>().GetAI();
        ai.ChangeStateForce(AIStateEnum.IDLE);
        attr.IsDead = false;
    }

    public override void SetLevel(AvatarInfo info)
    {
        //throw new NotImplementedException();
        MobaUtil.SetLevel(gameObject, info);
    }
    public override void SetPositionAndDir(AvatarInfo info)
    {
        //throw new NotImplementedException();
    }

    public override void DoNetworkDamage(GCPlayerCmd cmd)
    {
        MobaUtil.DoNetworkDamage(gameObject, cmd);
    }
    public override void Dead(GCPlayerCmd cmd)
    {
        MobaUtil.Dead(gameObject);
    }
    public override bool CheckSyncState()
    {
        return curInfo.State == PlayerState.AfterReset;
    }
}
