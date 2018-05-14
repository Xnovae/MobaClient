using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyLib
{
    /// <summary>
    /// 服务器和客户端的通信的浮点数
    /// TODO:应该修正为 Vector3d 实现
    /// </summary>
    public struct MyVec3
    {
        //Unity 坐标*100 厘米
        public int x, y, z;

        public static MyVec3 Parse(string s)
        {
            var t = s.IndexOf('>');
            Debug.Log("SubStringIs: " + s);
            var v = s.Substring(t + 1).Split(',');
            var xx = Convert.ToInt32(v[0]);
            var yy = Convert.ToInt32(v[1]);
            var zz = Convert.ToInt32(v[2]);
            var vc = new MyVec3()
            {
                x = xx,
                y = yy,
                z = zz,
            };
            return vc;
        }
        public Vector3 ToFloat()
        {
            return new Vector3(x / 100.0f, y / 100.0f, z / 100.0f);
        }
        public static MyVec3 FromFloat(float x, float y, float z)
        {
            return new MyVec3((int)(x * 100), (int)(y * 100), (int)(z * 100));
        }

        public static MyVec3 zero = new MyVec3(0, 0, 0);
        public MyVec3(int x1, int x2, int x3)
        {
            x = x1;
            y = x2;
            z = x3;
        }
        public override string ToString()
        {
            return x + ":" + y + ":" + z;
        }

    }
}
