using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MyLib;


public class MainDetailUI : IUserInterface
{
    public static MainDetailUI Instance;

    private GameObject medal;
    private List<GameObject> CacheMedals = new List<GameObject>();
    public List<MailInfo> MailInfoList = new List<MailInfo>();

    private UIToggle bug;
    private UIToggle advise;
    private UIToggle other;
    private UILabel totalNum;
    private UILabel ChatLabel;
    private void Awake()
    {
        Instance = this;
        SetCallback("EnterGame", OnEnter);
        /*
        StartCoroutine(StatisticsManager.Instance.QueryUserInfo());
        medal = GetName("medal");
        UpdateInfo();

        if (UserInfo.AutoMatch)
        {
            UserInfo.AutoMatch = false;
            OnEnter();
        }

        SetCallback("Exchange", () =>
        {
            StartCoroutine(StatisticsManager.Instance.ExchangeMedal());
        });

        GetName("bgMask").GetComponent<UIEventListener>().onClick += (go) =>
        {
            GetName("GMFeedbackPanel").SetActive(false);
        };

        bug = GetName("bug").GetComponent<UIToggle>();
        advise = GetName("advise").GetComponent<UIToggle>();
        other = GetName("other").GetComponent<UIToggle>();


        SetCallback("Send", SendGMFeedback);
        SetCallback("Close", () =>
        {
            GetName("GMFeedbackPanel").SetActive(false);
        });

        GetInput("InfoText").defaultText = "点击这里输入,最多100个字";

        SetCallback("mail", () =>
        {
            MailInfoList.Clear();
            GetName("mailTip").SetActive(false);
            StartCoroutine(StatisticsManager.Instance.GetAllMail());
        });

        StartCoroutine(CheckNewMailTip());
        SetCallback("record", OnRecord);
        totalNum = GetLabel("totalNum");
        SetCallback("Knowledge", OnKnowledge);
        SetCallback("ChatInfo", OnChat);

        ChatLabel = GetLabel("ChatLabel");

        this.regEvt = new List<MyEvent.EventType>()
        {
            MyEvent.EventType.UpdateChat,
        };
        RegEvent();

        ChatData.Instance.StartReceive();
        InitChatLabel();
        */
    }

    protected override void OnEvent(MyEvent evt)
    {
        if (evt.type == MyEvent.EventType.UpdateChat)
        {
            InitChatLabel();
        }
    }

    private void InitChatLabel()
    {
        ChatLabel.text = ChatData.Instance.FetchNew();
    }

    void OnKnowledge()
    {
        WindowMng.windowMng.PushView("UI/InfoUI");
    }
    void OnRecord()
    {
        WindowMng.windowMng.PushView("UI/RecordUI");
    }

    IEnumerator CheckNewMailTip()
    {
        while (true)
        {
            StartCoroutine(StatisticsManager.Instance.CheckMailTip());
            yield return new WaitForSeconds(10);
        }
    }

    public void CheckMailTip()
    {
        GetName("mailTip").SetActive(true);
    }

    public void UpdateInfo()
    {
        GetName("name").GetComponent<UILabel>().text = UserInfo.UserName;
        GetName("level").GetComponent<UILabel>().text = "LV." + UserInfo.UserLevel;

        GetName("attrName").GetComponent<UILabel>().text = UserInfo.UserName;
        GetName("attrLevelValue").GetComponent<UILabel>().text = UserInfo.UserLevel + "";
        GetName("attrExpValue").GetComponent<UILabel>().text = UserInfo.UserExp + "";

        GetName("Rename").SetActive(UserInfo.HaveRename == 0);

        SetCallback("Rename", () =>
        {
            WindowMng.windowMng.PushView("UI/RenameUI");
        });

        for (int i = 0; i < CacheMedals.Count; i++)
        {
            CacheMedals[i].SetActive(false);
        }

        for (int i = 0; i < UserInfo.MedalNum; i++)
        {
            if (i >= CacheMedals.Count)
            {
                CacheMedals.Add(GameObject.Instantiate(medal) as GameObject);
            }

            CacheMedals[i].transform.parent = medal.transform.parent;
            CacheMedals[i].transform.localScale = Vector3.one;
            CacheMedals[i].transform.localPosition = new Vector3(133 + i * 50, -90, 0);
            CacheMedals[i].SetActive(true);
        }

        if (UserInfo.UserLevel < 40)
        {
            GetName("UserExp").SetActive(true);
            GetName("UserUpgradeExp").SetActive(false);
            if (UserInfo.UserExp > 0)
            {
                GetName("expBar").SetActive(true);
                var ratio = UserInfo.UserExp / (float)GameData.RoleUpgradeConfig[UserInfo.UserLevel - 1].exp;
                GetSprite("expBar").width = (int)(ratio * 247);
            }
            else
            {
                GetName("expBar").SetActive(false);
            }
        }
        else
        {
            GetName("UserExp").SetActive(false);
            GetName("UserUpgradeExp").SetActive(true);
        }


    }

    public void OpenMail()
    {
        if (MailUI.Instance == null)
        {
            WindowMng.windowMng.PushView("UI/MailUI");
        }
        else
        {
            MailUI.Instance.UpdateFrame();
        }
    }

    void Start()
    {
        FakeObjSystem.fakeObjSystem.OnUIShown(-1, null, 2);
        if (NetDebug.netDebug.IsTest)
        {
            OnEnter();
        }
        //StartCoroutine(InitRecord());
    }

    private IEnumerator InitRecord()
    {
        yield return StartCoroutine(RecordData.QueryRecord());
        var js = RecordData.recordData;
        if (js != null)
        {
            totalNum.text = string.Format("战绩#总参战场次:{0}场", js["total"].AsInt);
        }
    }

    protected override void OnDestroy()
    {
        Instance = null;
        FakeObjSystem.fakeObjSystem.OnUIHide(-1);
        base.OnDestroy();
    }

    private void OnEnter()
    {
        if (StatisticsManager.Instance.IsBlack)
        {
            Util.ShowMsg("对不起你已经被加入黑名单,无法进行游戏");
        }
        else if (StatisticsManager.Instance.IsClose)
        {
            Util.ShowMsg(StatisticsManager.Instance.CloseMsg);
        }
        else
        {
            if (string.IsNullOrEmpty(UserInfo.UserName))
            {
                Util.ShowMsg("玩家名字为空");
            }
            ServerData.Instance.playerInfo.Roles.Name = UserInfo.UserName;
            ServerData.Instance.playerInfo.Roles.Job = Job.WARRIOR;
            WorldManager.worldManager.WorldChangeScene((int)LevelDefine.Prepare, false);
        }
    }

    private void SendGMFeedback()
    {
        var content = GetInput("InfoText").value;
        if (string.IsNullOrEmpty(content))
        {
            Util.ShowMsg("反馈内容不能为空,请重新输入");
            return;
        }

        if (content.Length > 100)
        {
            Util.ShowMsg("输入字数过多,请重新输入");
            return;
        }

        string type = "";
        if (bug.value)
        {
            type = "11";
        }
        if (advise.value)
        {
            type = "13";
        }
        if (other.value)
        {
            type = "10";
        }

        GetName("GMFeedbackPanel").SetActive(false);
        StartCoroutine(StatisticsManager.Instance.GMFeedback(content, type));

    }

    private void OnChat()
    {
        WindowMng.windowMng.PushView("UI/ChatUI");
    }
}
