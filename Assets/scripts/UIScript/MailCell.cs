using System;
using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class MailCell : IUserInterface
    {
        public UILabel title;
        public UILabel sender;
        public UILabel dateTime;
        public UILabel countDown;

        public UISprite mailStateIcon;
        public GameObject selectedBg;
        private int index = -1;

        private void Awake()
        {
            title = GetLabel("Title");
            dateTime = GetLabel("DateTime");
            sender = GetLabel("Sender");
            countDown = GetLabel("Countdown");

            mailStateIcon = GetSprite("MailStateIcon");
            selectedBg = GetName("SelectedBg");
        }

        public void SetData(int i,MailInfo mi)
        {
            index = i;
            title.text = mi.title;
            var dt = UnixTimesStampToDateTime(long.Parse(mi.dateTime));       
            dateTime.text = dt.ToString("yyyy.MM.dd");
            sender.text = mi.sender;
            countDown.text = mi.countdown;

            UIEventListener.Get(gameObject).onClick = delegate (GameObject go)
            {
                if (MailUI.Instance != null)
                    MailUI.Instance.ChooseMailItem(index);
            };

            if (mi.state == 0)
            {
                mailStateIcon.spriteName = "email";
            }
            else
            {
                mailStateIcon.spriteName = "email_read";
            }
        }

        public void Selected(bool select)
        {
            selectedBg.SetActive(select);
        }

        public static DateTime UnixTimesStampToDateTime(long unixTime)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTime);
        }
    }
}
