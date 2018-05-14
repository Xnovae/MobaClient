using UnityEngine;

namespace MyLib
{
    public class CreateCharInit : MonoBehaviour
    {
        private void Start()
        {
            WindowMng.windowMng.PushView("UI/MainUI2");
        }
    }
}
