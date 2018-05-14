using MyLib;
using UnityEngine;
using System.Collections;

public class GameOverCell : IUserInterface
{
    public UILabel Name;
    public UILabel Driver;
    public UILabel Score;

    public UILabel KillCount;
    public UILabel DeadCount;
    public UILabel AssistCount;

    private UISprite Bg;

    void Awake()
    {
        Name = GetLabel("Name");
        Driver = GetLabel("Driver");
        Score = GetLabel("Score");

        KillCount = GetLabel("Kill");
        DeadCount = GetLabel("Dead");
        AssistCount = GetLabel("Assist");

        Bg = GetSprite("Bg");
    }

    public void SetData(int rank, string n, int s, int serverId, int killCount, int deadCount, int assistCount)
    {
        if (rank == 1)
        {
            Name.color = new Color32(235, 245, 46, 255);
            Driver.color = new Color32(235, 245, 46, 255);
            Score.color = new Color32(235, 245, 46, 255);

            KillCount.color = new Color32(235, 245, 46, 255);
            DeadCount.color = new Color32(235, 245, 46, 255);
            AssistCount.color = new Color32(235, 245, 46, 255);
        }
        else if (NetMatchScene.Instance.myId == serverId)
        {
            Name.color = new Color32(66, 240, 19, 255);
            Driver.color = new Color32(66, 240, 19, 255);
            Score.color = new Color32(66, 240, 19, 255);

            KillCount.color = new Color32(66, 240, 19, 255);
            DeadCount.color = new Color32(66, 240, 19, 255);
            AssistCount.color = new Color32(66, 240, 19, 255);
        }
        else
        {
            Name.color = new Color32(199, 227, 255, 255);
            Driver.color = new Color32(199, 227, 255, 255);
            Score.color = new Color32(199, 227, 255, 255);

            KillCount.color = new Color32(199, 227, 255, 255);
            DeadCount.color = new Color32(199, 227, 255, 255);
            AssistCount.color = new Color32(199, 227, 255, 255);
        }

        Name.text = rank.ToString();
        Driver.text = n;
        Score.text = s.ToString();

        KillCount.text = killCount.ToString();
        DeadCount.text = deadCount.ToString();
        AssistCount.text = assistCount.ToString();

        if (rank >= 1 && rank <= 3)
        {
            GetName("Reward"+rank).SetActive(true);
            Name.gameObject.SetActive(false);
        }

        Bg.gameObject.SetActive(rank%2 == 1);
    }
}
