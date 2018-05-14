using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyLib;

namespace MyLib
{
    public class RealtimeStatisticsUI : IUserInterface
    {
        private List<GameObject> cells = new List<GameObject>();
        private GameObject cell;
        private UIGrid grid;

        private void Awake()
        {
            grid = GetGrid("Grid");
            cell = GetName("Cell");

            SetCallback("Quit", OnQuit);
            SetCallback("Close", () => { WindowMng.windowMng.PopView(); });

            this.regEvt = new List<MyEvent.EventType>()
            {
                MyEvent.EventType.UpdateName,
                MyEvent.EventType.RemovePlayer,
                MyEvent.EventType.UpdateScoreDetail
            };
        }

        protected override void OnEvent(MyEvent evt)
        {
            base.OnEvent(evt);
            if (evt.type == MyEvent.EventType.UpdateScoreDetail)
            {
                UpdateFrame();
            }
        }

        private void Start()
        {
            UpdateFrame();
            if (NetDebug.netDebug.IsTest)
            {
                OnQuit();
            }
        }

        private void UpdateFrame()
        {
            var score = ScoreManager.Instance.score;
            while (cells.Count < score.Count)
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
            var keys = score.Keys.ToList();
            keys.Sort((a, b) =>
            {
                var ad = score[a];
                var bd = score[b];
                if (ad.killed > bd.killed)
                {
                    return -1;
                }
                if (ad.killed < bd.killed)
                {
                    return 1;
                }
                return 0;
            });

            var rank = 0;
            for (var i = 0; i < keys.Count; i++)
            {
                var k = keys[i];
                var player = ObjectManager.objectManager.GetPlayer(k);
                if (player != null)
                {
                    var un = player.GetComponent<NpcAttribute>().userName;
                    if (!string.IsNullOrEmpty(un))
                    {
                        ScoreManager.Instance.UpdateCacheName(k, un);
                        var g = cells[i];
                        g.SetActive(true);
                        g.GetComponent<RealtimeCell>().SetData(rank + 1, un, score[k].killed, score[k].serverId, score[k].killCount, score[k].deadCount, score[k].assistCount);
                        rank++;
                    }
                }
                else
                {
                    ScoreManager.Instance.RemoveCacheName(k);
                }
            }
            StartCoroutine(WaitReset());
        }

        private IEnumerator WaitReset()
        {
            yield return new WaitForSeconds(0.1f);
            grid.repositionNow = true;
        }

        private void OnQuit()
        {
            WorldManager.worldManager.WorldChangeScene(2, false);
        }
    }
}