using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class RealtimeCell : IUserInterface
    {
        public UILabel Rank;
        public UILabel Driver;
        public UILabel Score;

        public UILabel KillCount;
        public UILabel DeadCount;
        public UILabel AssistCount;

        private void Awake()
        {
            Rank = GetLabel("Name");
            Driver = GetLabel("Driver");
            Score = GetLabel("Score");

            KillCount = GetLabel("Kill");
            DeadCount = GetLabel("Dead");
            AssistCount = GetLabel("Assist");
        }

        public void SetData(int rank, string n, int s, int serverId,int killCount,int deadCount,int assistCount)
        {
            if (rank == 1)
            {
                Rank.color = new Color32(235, 245, 46, 255);
                Driver.color = new Color32(235, 245, 46, 255);
                Score.color = new Color32(235, 245, 46, 255);

                KillCount.color = new Color32(235, 245, 46, 255);
                DeadCount.color = new Color32(235, 245, 46, 255);
                AssistCount.color = new Color32(235, 245, 46, 255);
            }
            else if (NetMatchScene.Instance.myId == serverId)
            {
                Rank.color = new Color32(66, 240, 19, 255);
                Driver.color = new Color32(66, 240, 19, 255);
                Score.color = new Color32(66, 240, 19, 255);

                KillCount.color = new Color32(66, 240, 19, 255);
                DeadCount.color = new Color32(66, 240, 19, 255);
                AssistCount.color = new Color32(66, 240, 19, 255);
            }
            else
            {
                Rank.color = new Color32(199, 227, 255, 255);
                Driver.color = new Color32(199, 227, 255, 255);
                Score.color = new Color32(199, 227, 255, 255);

                KillCount.color = new Color32(199, 227, 255, 255);
                DeadCount.color = new Color32(199, 227, 255, 255);
                AssistCount.color = new Color32(199, 227, 255, 255);
            }

            Rank.text = rank.ToString();
            Driver.text = n;
            Score.text = s.ToString();

            KillCount.text = killCount.ToString();
            DeadCount.text = deadCount.ToString();
            AssistCount.text = assistCount.ToString();
            /*
            if (rank >= 1 && rank <= 3)
            {
                GetName("Reward" + rank).SetActive(true);
            }
             */
        }
    }

}
