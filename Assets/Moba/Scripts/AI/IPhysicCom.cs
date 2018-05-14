using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class IPhysicCom : MonoBehaviour {
    public abstract void TurnToDir(float dir);
    public abstract void TurnTo(Vector3 mdir);
    public abstract void MoveTo(Vector3 newPos);
    public abstract void MoveToIgnorePhysic(Vector3 newPos);
    //分多段进行物理检测
    public virtual void MoveToWithPhysic(Vector3 np)
    {
        throw new NotImplementedException();
    }
}
