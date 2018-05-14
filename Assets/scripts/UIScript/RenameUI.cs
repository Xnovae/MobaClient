using MyLib;

public class RenameUI : IUserInterface
{
    private UIInput uname;

    private void Awake()
    {
        uname = GetInput("InfoText");
        uname.value = UserInfo.UserName;
 
        SetCallback("random", OnName);
        SetCallback("Close", () =>
        {
            WindowMng.windowMng.PopView();
        });

        SetCallback("Send", OnSend);
    }

    void OnName()
    {
        uname.value = RandomName.GetRandName();
    }

    void OnSend()
    {
        if (uname.value.Length > 6)
        {
            Util.ShowMsg("名字长度不能超过6个字符");
            return;
        }

        if (uname.value == UserInfo.UserName)
        {
            Util.ShowMsg("名称没有改变,无须改名");
            return;
        }

        ClientApp.Instance.StartCoroutine(StatisticsManager.Instance.RenameUserName(uname.value));
        WindowMng.windowMng.PopView();
    }
}
