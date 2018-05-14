using MyLib;
using UnityEngine;
using System.Collections;

public class Newbee : IUserInterface
{
    private GameObject left, right;
    void Awake()
    {
        left = GetName("Left");
        right = GetName("Right");
        left.SetActive(false);
        right.SetActive(false);

        StartCoroutine(TeachUseController());
    }

    private IEnumerator TeachUseController()
    {
        left.SetActive(true);
        //Util.ShowMsg("拖动左摇杆进行移动");
        WindowMng.windowMng.ShowNotifyLog("拖动左摇杆进行移动", 5, null, true);
        yield return new WaitForSeconds(5);
        left.SetActive(false);
        right.SetActive(true);
        //Util.ShowMsg("拖动右摇杆瞄准，松手射击");
        WindowMng.windowMng.ShowNotifyLog("拖动右摇杆瞄准，松手射击", 5, null, true);
        yield return new WaitForSeconds(5);
        left.SetActive(false);
        right.SetActive(false);
    }
}
