using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 支持玩家自己移动的物理网格检测
/// 服务器端也要采用这种物理网格检测机制
/// </summary>
public class MobaMePhysic : IPhysicCom {
    public override void TurnToDir(float dir)
    {
        transform.rotation = Quaternion.Euler(new Vector3(0, dir, 0));
    }
    public override void TurnTo(Vector3 moveDirection)
    {
        var y2 = Quaternion.LookRotation(moveDirection).eulerAngles.y;
        transform.rotation = Quaternion.Euler(new Vector3(0, y2, 0));
    }

    /// <summary>
    /// 从最近的碰撞网格挤出去
    /// 避免循环多迭代几次
    /// 但是要剔除掉已经测试过的网格
    /// </summary>
    /// <param name="newPos"></param>
    public override void MoveTo(Vector3 newPos)
    {
        var gm = GridManager.Instance;
        if (gm != null)
        {
            var fixPos = gm.FindNearestWalkableGridPos(newPos);
            transform.position = fixPos;
        }
    }
    public override void MoveToWithPhysic(Vector3 np)
    {
        var cutNum = 2;
        var cp = transform.position;
        var deltaPos = np - cp;
        var halfDelta = deltaPos / cutNum;
        var initPos = cp;
        if (GridManager.Instance != null)
        {
            for (var i = 0; i < cutNum; i++)
            {
                var p1 = initPos + halfDelta;
                var newPos1 = GridManager.Instance.FindNearestWalkableGridPos(p1);
                initPos = newPos1;
            }
        }
        transform.position = initPos;
    }

    public override void MoveToIgnorePhysic(Vector3 newPos)
    {
        var gm = GridManager.Instance;
        if(gm != null)
        {
            transform.position = gm.mapPosFixHeight(newPos);
        }
    }

}
