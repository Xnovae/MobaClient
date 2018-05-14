using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class ScoreCell : IUserInterface
    {

        public UILabel Rank;
        public UILabel Driver;
        public UILabel Score;

        private void Awake()
        {
            Rank = GetLabel("Rank");
            Driver = GetLabel("Driver");
            Score = GetLabel("Score");
        }

        public void SetData(int rank, string n, int s,int serverId)
        {
            if (rank == 1)
            {
                Rank.color = new Color32(235,245,46,255);
                Driver.color = new Color32(235, 245, 46, 255);
                Score.color = new Color32(235, 245, 46, 255);          
            }
            else if (NetMatchScene.Instance.myId == serverId)
            {
                Rank.color = new Color32(66, 240, 19, 255);
                Driver.color = new Color32(66, 240, 19, 255);
                Score.color = new Color32(66, 240, 19, 255);
            }
            else
            {
                Rank.color = new Color32(199, 227, 255, 255);
                Driver.color = new Color32(199, 227, 255, 255);
                Score.color = new Color32(199, 227, 255, 255);
            }

            Rank.text = rank.ToString();
            Driver.text = n;
            Score.text = s.ToString();
            /*
            if (rank >= 1 && rank <= 3)
            {
                GetName("Reward" + rank).SetActive(true);
            }
             */ 
        }
    }

}