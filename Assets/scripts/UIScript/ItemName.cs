using MyLib;
using UnityEngine;
using System.Collections;

public class ItemName : MonoBehaviour
{
    private DropItemStatic attr;
    // Use this for initialization
    private GameObject bar;
    void Start()
    {
        attr = GetComponent<DropItemStatic>();
        bar = GameObject.Instantiate(Resources.Load<GameObject>("UI/ItemNameUI")) as GameObject;
        bar.transform.parent = WindowMng.windowMng.GetUIRoot().transform;
        Util.InitGameObject(bar);

        var iu = bar.AddComponent<IUserInterface>();
        iu.GetLabel("NameLabel").text = attr.itemData.ItemName;

    }

    void LateUpdate()
    {
        Vector3 sp = CameraController.cameraController.GetComponent<Camera>().WorldToScreenPoint(transform.position + new Vector3(0, 1.5f, 0));
        var uiWorldPos = UICamera.mainCamera.ScreenToWorldPoint(sp);
        uiWorldPos.z = 0;
        bar.transform.position = uiWorldPos;
    }

    void OnDestroy()
    {
        GameObject.Destroy(bar);
    }
}
