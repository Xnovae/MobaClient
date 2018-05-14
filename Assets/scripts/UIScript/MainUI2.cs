using System.Collections.Generic;
using MyLib;
using UnityEngine;
using System.Collections;

/// <summary>
/// 玩家登陆创建角色
/// </summary>
public class MainUI2 : IUserInterface
{

    private UIInput uname;

    private void Awake()
    {
        uname = GetInput("NameInput");
        uname.value = RandomName.GetRandName();
        SetCallback("createChar", OnEnter);
        SetCallback("randomName", OnName);
    }

    void OnName()
    {
        uname.value = RandomName.GetRandName();
    }

    void Start()
    {
        FakeObjSystem.fakeObjSystem.OnUIShown (-1, null, 2);
        if (NetDebug.netDebug.IsTest)
        {
            OnEnter();
        }
    }

    protected override void OnDestroy()
    {
        FakeObjSystem.fakeObjSystem.OnUIHide(-1);
        base.OnDestroy();
    }

    private void OnEnter()
    {
        if (string.IsNullOrEmpty(uname.value))
        {
            Util.ShowMsg("角色名不能为空");
            return;
        }

        var label = uname.GetComponentsInChildren<UILabel>()[0];
        if (label.text.Length > 6)
        {
            Util.ShowMsg("名字长度不能超过6个字符");
            return;
        }

        StartCoroutine(StatisticsManager.Instance.CreateChar(uname.value));
    }
}
