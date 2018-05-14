using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class MainLoginInit : MonoBehaviour
    {
        void Start()
        {
            WindowMng.windowMng.PushView("UI/MainLoginUI");
        }
    }
}