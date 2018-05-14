using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyLib;

public class DebugServerPos : MonoBehaviour {
    private GameObject debugServerObj;
    private ISyncInterface sync;
    private void Start()
    {
        sync = GetComponent<ISyncInterface>();

        debugServerObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        debugServerObj.transform.localScale = Vector3.one * 4;
        debugServerObj.AddComponent<DrawDir>();
    }

    private void Update()
    {
        if (NetDebug.netDebug.TestNetSyncMove)
        {
            var sp = sync.GetCurInfoSpeed();
            var sx = sp.x;
            var sy = sp.z;
            var rad = Mathf.Atan2(sx, sy);
            var deg = Mathf.Rad2Deg * rad;
            debugServerObj.transform.position = sync.GetCurInfoPos();
            debugServerObj.transform.localRotation = Quaternion.Euler(new Vector3(0, deg, 0));
        }
    }
}
