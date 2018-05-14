using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawDir : MonoBehaviour
{
    public void OnDrawGizmos()
    {
        var p = transform.position;
        var e = p + transform.forward * 4;

        Debug.DrawLine(p, e, Color.yellow);
    }
}
