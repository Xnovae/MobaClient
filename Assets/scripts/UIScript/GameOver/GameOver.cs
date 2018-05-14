using System.Collections.Generic;
using System.Linq;
using MyLib;
using SimpleJSON;
using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class GameOver : IUserInterface
    {
        private List<GameObject> cells = new List<GameObject>();
        private GameObject cell;
        private UIGrid grid;

        private Transform title1;
        private Transform title2;
        private GameObject quit;
        private GameObject close;

        private void Awake()
        {
            grid = GetGrid("Grid");
            cell = GetName("Cell");

            SetCallback("Quit", OnQuit);

            quit = GetName("Quit");
            close = GetName("Close");
            title1 = GetName("Title1").transform;
            title2 = GetName("Title2").transform;

            SetCallback("Quit", () =>
            {
                //UserInfo.AutoMatch = true;
                UserInfo.AutoMatch = false;
                OnQuit();
            });
        }

        private void Start()
        {
            UpdateFrame();
            if (NetDebug.netDebug.IsTest)
            {
                OnQuit();
            }
            //StartCoroutine(MoveToLeft());
        }

        IEnumerator MoveToLeft()
        {
            yield return new WaitForSeconds(2f);

            var deltaTime = 1f;
            var time = 0f;
            while (time < 1)
            {
                time += Time.deltaTime / deltaTime;
                time = Mathf.Lerp(0, 1, time);
                gameObject.transform.localPosition = new Vector3(time * -1280, 0,0);
                yield return null;
            }
        }

        IEnumerator TweenUpDown()
        {
            var sv = GetName("ScrollView").GetComponent<UIPanel>();
            var br = sv.baseClipRegion;

            quit.SetActive(false);
            close.SetActive(false);
            sv.SetRect(br.x, -170, br.z, 0);
            GetName("BG").SetActive(false);

            title1.localScale = Vector3.zero;
            title2.localScale = Vector3.zero;
            title1.localPosition = new Vector3(0, -64, 0);
            title2.localPosition = new Vector3(0, -64, 0);

            var deltaTime = 1f;
            var time = 0f;
            while (time < 1)
            {
                time += Time.deltaTime / deltaTime;
                var scale = Mathf.Lerp(0, 1, time);
                title1.localScale = new Vector3(scale, scale, scale);
                title2.localScale = new Vector3(scale, scale, scale);
                yield return null;
            }

            title1.localScale = Vector3.one;
            title2.localScale = Vector3.one;

            deltaTime = 0.7f;
            time = 0f;
            GetName("BG").SetActive(true);
            while (time < 1)
            {
                time += Time.deltaTime / deltaTime;
                GetName("BG").GetComponent<UISprite>().height = (int)Mathf.Lerp(0, 440, time);
                sv.SetRect(br.x, -170, br.z, Mathf.Lerp(0, 362, time));

                title1.localPosition = new Vector3(0, (int)Mathf.Lerp(-64, 192.15f, time), 0);
                title2.localPosition = new Vector3(0, (int)Mathf.Lerp(-64, 192.15f, time), 0);
                yield return null;
            }

            GetName("BG").GetComponent<UISprite>().height = 440;
            sv.SetRect(br.x, -170, br.z, 362);
            quit.SetActive(true);
            close.SetActive(true);
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

            var ord = 0;
           
            for (var i = 0; i < keys.Count; i++)
            {
                var k = keys[i];
                //var player = ObjectManager.objectManager.GetPlayer(k);
                var un = ScoreManager.Instance.GetCacheName(k);
                var myId = NetMatchScene.Instance.myId;
                //var un = player.GetComponent<NpcAttribute>().userName;
                if (!string.IsNullOrEmpty(un))
                {
                    var g = cells[i];
                    g.SetActive(true);
                    g.GetComponent<GameOverCell>()
                        .SetData(ord + 1, un, score[k].killed, score[k].serverId, score[k].killCount, score[k].deadCount, score[k].assistCount);
                    ord++;
                    if (ord == 1)
                    {
                        if (k != myId)
                        {
                            GetName("Title2").SetActive(true);
                            GetName("Title1").SetActive(false);
                        }
                        else//MVP 场次
                        {
                            var js = new JSONClass();
                            js.Add("mvp", new JSONData(1));
                            RecordData.UpdateRecord(js); 
                        }
                    }
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
