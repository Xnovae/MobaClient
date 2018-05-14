using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MyLib
{
    public class ScoreUI : IUserInterface
    {
        UIGrid grid;
        GameObject cell;
        private List<GameObject> cells = new List<GameObject>();
        private float oldHeight;
        private float oldCenter;
        private float newCenter;
        private float newHeight = 154;
        private int spriteHeight = 192;
        void Awake()
        {
            grid = GetGrid("Grid");
            cell = GetName("Cell");
            //cell.SetActive(false);
            SetCallback("UpDown", OnUpDown);
            SetCallback("UpDown1", OnUpDown1);

            SetCallback("DetailButton", () => { WindowMng.windowMng.PushView("UI/RealtimeStatistics"); });

            var sv = GetName("ScrollView").GetComponent<UIPanel>();
            Debug.LogError("sv: "+sv.clipOffset+" base "+sv.baseClipRegion);
            oldHeight = sv.baseClipRegion.w;
            oldCenter = sv.baseClipRegion.y;
            newCenter = oldCenter+oldHeight/2.0f - newHeight/2.0f;
            this.regEvt = new List<MyEvent.EventType>()
            {
                MyEvent.EventType.UpdateName,
                MyEvent.EventType.RemovePlayer,
            };
        }

        protected override void OnEvent(MyEvent evt)
        {
            SetData(ScoreManager.Instance.score);
        }

        void Start()
        {
            OnUpDown();
        }

        void OnUpDown()
        {
            GetName("UpDown").SetActive(false);
            StartCoroutine(TweenUpDownUp());         
        }

        IEnumerator TweenUpDownUp()
        {
            var sv = GetName("ScrollView").GetComponent<UIPanel>();
            var br = sv.baseClipRegion;

            var deltaTime = 0.3f;
            var time = 0f;

            while (time < 1)
            {
                time += Time.deltaTime / deltaTime;

                GetName("BG").GetComponent<UISprite>().height = (int)Mathf.Lerp(318, spriteHeight, time);
                GetName("Detail").transform.localPosition = new Vector3(75, Mathf.Lerp(-48, 75, time), 0);
                sv.SetRect(br.x, Mathf.Lerp(oldCenter, newCenter, time), br.z, Mathf.Lerp(oldHeight, newHeight, time));
                yield return null;
            }

            sv.SetRect(br.x, newCenter, br.z, newHeight);
            GetName("UpDown1").SetActive(true);
            GetName("BG").GetComponent<UISprite>().height = spriteHeight;
            GetName("Button").transform.localPosition = new Vector3(115, -8, 0);
            GetName("Detail").transform.localPosition = new Vector3(75, 75, 0);
        }

        void OnUpDown1()
        {

            GetName("UpDown1").SetActive(false);
            StartCoroutine(TweenUpDown());
        }

        IEnumerator TweenUpDown()
        {
            var sv = GetName("ScrollView").GetComponent<UIPanel>();
            var br = sv.baseClipRegion;

            var deltaTime = 0.3f;
            var time = 0f;

            while (time < 1)
            {
                time += Time.deltaTime / deltaTime;

                GetName("BG").GetComponent<UISprite>().height = (int)Mathf.Lerp(spriteHeight, 318, time);
                GetName("Detail").transform.localPosition = new Vector3(75, Mathf.Lerp(75, -48, time), 0);
                sv.SetRect(br.x, Mathf.Lerp(newCenter, oldCenter, time), br.z, Mathf.Lerp(newHeight, oldHeight, time));
                yield return null;
            }

            sv.SetRect(br.x, oldCenter, br.z, oldHeight);
            GetName("UpDown").SetActive(true);
            GetName("BG").GetComponent<UISprite>().height = 318;
            GetName("Button").transform.localPosition = new Vector3(115, -131, 0);
            GetName("Detail").transform.localPosition = new Vector3(75, -48, 0);
        }



        public void SetData(Dictionary<int, ScoreData> score)
        {

            var gameOver = WorldManager.worldManager.GetActive().state == SceneState.GameOver;
            //游戏结束不要更新榜单了
            if (gameOver)
            {
                return;
            }

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
            var myId = NetMatchScene.Instance.myId;
            var myLocalId = ObjectManager.objectManager.GetMyLocalId();
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
                        g.GetComponent<ScoreCell>().SetData(rank + 1, un, score[k].killed, score[k].serverId);
                        rank++;
                        if (rank == 1)
                        {
                            var evt = new MyEvent(MyEvent.EventType.IAmFirst);
                            evt.intArg = k;
                            MyEventSystem.myEventSystem.PushEvent(evt);
                        }
                    }
                }
                else
                {
                    ScoreManager.Instance.RemoveCacheName(k);
                }
            }
            StartCoroutine(WaitReset());
        }

        IEnumerator WaitReset()
        {
            yield return new WaitForSeconds(0.1f);
            grid.repositionNow = true;
        }
    }
}