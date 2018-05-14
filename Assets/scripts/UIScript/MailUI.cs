using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using MyLib;
using UnityEngine;

namespace MyLib
{
    public class MailInfo
    {
        public int id;
        public string title;
        public string sender;
        public string dateTime;
        public string countdown;
        public int state; //0 未读 1 已读
        public string text;
    }

    public class MailUI : IUserInterface
    {
        public static MailUI Instance;

        private List<MailInfo>   mailInfoList  = new List<MailInfo>();
        private List<GameObject> cells = new List<GameObject>();
        private GameObject cell;
        private UIGrid grid;
        private int currentSelectedIndex = -1;

        private MailCell selectedMail;

        private void Awake()
        {
            Instance = this;
            grid = GetGrid("Grid");
            cell = GetName("Cell");

            SetCallback("Close", () =>
            {
                WindowMng.windowMng.PopView();
            });

            SetCallback("DeleteMail", () =>
            {
                StartCoroutine(StatisticsManager.Instance.DeleteMail(mailInfoList[currentSelectedIndex].id));
            });

            SetCallback("OneKey", () =>
            {
                if (mailInfoList.Count == 0)
                {
                    Util.ShowMsg("当前没有任何邮件可删除");
                    return;
                }

                GetName("ConfirmDialog").SetActive(true);

                
            });

            SetCallback("Cancel", () =>
            {
                GetName("ConfirmDialog").SetActive(false);
            });

            SetCallback("Ok", () =>
            {
                StartCoroutine(StatisticsManager.Instance.OnekeyDeleteMail());
                GetName("ConfirmDialog").SetActive(false);
            });

            mailInfoList.AddRange(MainDetailUI.Instance.MailInfoList);
            UpdateFrame();
        }

        void OnDestroy()
        {
            Instance = null;
        }


        public void UpdateMailState(int id,int state)
        {
            foreach (var m in mailInfoList)
            {
                if (m.id == id)
                {
                    m.state = state;
                    RefreshView();
                    break;
                }
            }
        }

        public void DeleteMail(int id)
        {
            MailInfo mi = null;
            foreach (var m in mailInfoList)
            {
                if (m.id == id)
                {
                    mi = m;                   
                    break;
                }
            }
            if (mi != null)
            {
                mailInfoList.Remove(mi);
                UpdateFrame();
            }
        }

        public void DeleteMails(List<int> idList)
        {
            List<MailInfo> miList = new List<MailInfo>();
            foreach (var id in idList)
            {
                foreach (var m in mailInfoList)
                {
                    if (m.id == id)
                    {
                        miList.Add(m);
                        break;
                    }
                }
            }

            foreach (var mi in miList)
            {
                mailInfoList.Remove(mi);
                UpdateFrame();
            }
        }

        private void RefreshView()
        {
            for (var i = 0; i < mailInfoList.Count; i++)
            {
                var g = cells[i];
                g.SetActive(true);
                g.GetComponent<MailCell>()
                    .SetData(i, mailInfoList[i]);
            }

            if (mailInfoList.Count > 0)
            {
                currentSelectedIndex = Mathf.Clamp(currentSelectedIndex, 0, mailInfoList.Count - 1);
                ShowDetailMailInfo();
            }
        }
 
        public void UpdateFrame()
        {
            if (mailInfoList.Count == 0)
            {
                GetName("MailNoneDetailPanel").SetActive(true);
                GetName("MailDetailPanel").SetActive(false);
            }
            else
            {
                GetName("MailNoneDetailPanel").SetActive(false);
                GetName("MailDetailPanel").SetActive(true);
            }

            while (cells.Count < mailInfoList.Count)
            {
                var g = Object.Instantiate(cell) as GameObject;
                g.transform.parent = cell.transform.parent;
                Util.InitGameObject(g);
                cells.Add(g);
                g.SetActive(false);
            }
            foreach (var c in cells)
            {
                c.SetActive(false);
            }
            
            mailInfoList.Sort((obj1, obj2) =>
            {
                return obj2.id.CompareTo(obj1.id);
            });

  
            for (var i = 0; i < mailInfoList.Count; i++)
            {
                var g = cells[i];
                g.SetActive(true);
                g.GetComponent<MailCell>()
                    .SetData(i,mailInfoList[i]);
            }

            if (mailInfoList.Count > 0)
            {
                currentSelectedIndex = Mathf.Clamp(currentSelectedIndex, 0, mailInfoList.Count - 1);
                ShowDetailMailInfo();
            }


            StartCoroutine(WaitReset());
        }

        public void ChooseMailItem(int index)
        {
            if (currentSelectedIndex == index)
            {
                return;
            }

            currentSelectedIndex = index;
            ShowDetailMailInfo();
        }

        private void ShowDetailMailInfo()
        {
            if (selectedMail != null)
            {
                selectedMail.Selected(false);
            }
            selectedMail = cells[currentSelectedIndex].GetComponent<MailCell>();
            if (selectedMail != null)
            {
                selectedMail.Selected(true);
                selectedMail.SetData(currentSelectedIndex, mailInfoList[currentSelectedIndex]);
            }

            var mi = mailInfoList[currentSelectedIndex];
            if (mailInfoList[currentSelectedIndex].state == 0)
            {
                StartCoroutine(StatisticsManager.Instance.ReadMail(mailInfoList[currentSelectedIndex].id));
            }
            GetLabel("DetailTitle").text = mi.title;
            GetLabel("MailSender").text = mi.sender;
            GetLabel("MailText").text = mi.text;
        }

        private IEnumerator WaitReset()
        {
            yield return new WaitForSeconds(0.1f);
            grid.repositionNow = true;
        }
    }
}