using System.IO;
#if UNITY_EDITOR
using UnityEngine;
using System.Collections;

namespace MyLib 
{
    public class TestShader : MonoBehaviour
    {
        [ButtonCallFunc()]public bool SetOther;

        public void SetOtherMethod()
        {
            var me = ObjectManager.objectManager.GetMyAttr();
            me.SetTeamHideShader();
        }

        [ButtonCallFunc()] public bool TestStr;

        public void TestStrMethod()
        {
            var warn = "超能弹药已经产生，快去争夺";
            var res = "";
            foreach (var c in warn)
            {
                Debug.Log(c);
                res += c;
            }
            Debug.Log(res);
            File.WriteAllText("test.txt", res);

        }

    }
}

#endif