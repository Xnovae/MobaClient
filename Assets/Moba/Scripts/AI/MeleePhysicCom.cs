using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleePhysicCom : IPhysicCom 
{
    private Rigidbody rigid;
    private void Start()
    {
        rigid = GetComponent<Rigidbody>();

    }

    public override void TurnToDir(float dir)
    {
        transform.rotation = Quaternion.Euler(new Vector3(0, dir, 0));
    }
    public override void TurnTo(Vector3 moveDirection)
    {
        var y2 = Quaternion.LookRotation(moveDirection).eulerAngles.y;
        transform.rotation = Quaternion.Euler(new Vector3(0, y2, 0));
    }
    public override void MoveTo(Vector3 newPos)
    {
        //rigid.MovePosition(newPos);
        //transform.position = newPos;
        var gm = GridManager.Instance;
        if (gm != null)
        {
            transform.position = gm.mapPosFixHeight(newPos);
        }
    }

    public override void MoveToIgnorePhysic(Vector3 newPos)
    {
        MoveTo(newPos);
    }
}
