using SimpleJSON;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MyLib
{
    public class ScoreData
    {
        public int killed = 0;
        public int beKilled = 0;
        public int serverId;

        public int killCount;
        public int deadCount;
        public int assistCount;
    }

    /// <summary>
    /// 统计得分
    /// 倒计时
    /// 显示游戏结束面板 
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public int leftTime = 300;
        public Dictionary<int, ScoreData> score = new Dictionary<int, ScoreData>();
        private LeftTimeUI ltui;
        private ScoreUI scoreUI;


        public static ScoreManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            StartCoroutine(CountDown());
            //机器人自动退出房间 如果超过5个人的话
            if (NetDebug.netDebug.IsTest)
            {
                StartCoroutine(CheckPlayerNum());
            }
        }

        private IEnumerator CheckPlayerNum()
        {
            var wt = new WaitForSeconds(10);
            var count5 = 0;
            var maxPlayer = 4;
            var waitTimePer = 5;
            while (true)
            {
                var playerNum = ObjectManager.objectManager.photonViewList.Count;
                Log.Sys("PlayerNum: "+playerNum);
                if (playerNum >= maxPlayer)
                {
                    Debug.LogError("PlayerTooMany Robot ToQuit: "+playerNum);
                    if (count5 >= 3)
                    {
                        Application.Quit();
                        break;
                    }
                    else
                    {
                        count5++;
                        //随机等待时间防止大家都退化
                        var tm = Random.Range(1, 4) * waitTimePer;
                        yield return new WaitForSeconds(tm);
                    }
                }
                else
                {
                    count5 = 0;
                    yield return wt;
                }
            }
            Debug.LogError("ApplicationQuit");
        }

        public void NetSyncScore(int id, int s, int serverId, int killCount, int deadCount, int assistCount)
        {
            if (!score.ContainsKey(id))
            {
                score[id] = new ScoreData();
            }
            score[id].killed = s;
            score[id].serverId = serverId;
            score[id].killCount = killCount;
            score[id].deadCount = deadCount;
            score[id].assistCount = assistCount;
            scoreUI.SetData(score);

            MyEventSystem.PushEventStatic(MyEvent.EventType.UpdateScoreDetail);
        }


        public void NetAddScore(int attacker, int enemy)
        {
            if (!score.ContainsKey(attacker))
            {
                score[attacker] = new ScoreData();
            }
            score[attacker].killed++;

            if (!score.ContainsKey(enemy))
            {
                score[enemy] = new ScoreData();
            }
            score[enemy].beKilled++;

            scoreUI.SetData(score);
        }


        public void NetSyncTime(int lt)
        {
            leftTime = lt;
        }

        private bool overYet = false;

        private Dictionary<int, string> CacheName = new Dictionary<int, string>();

        public void RemoveCacheName(int id)
        {
            CacheName.Remove(id);
        }

        public void UpdateCacheName(int id, string n)
        {
            CacheName[id] = n;
        }

        public string GetCacheName(int id)
        {
            if (CacheName.ContainsKey(id))
            {
                return CacheName[id];
            }
            return null;
        }

        public void NetworkGameOver()
        {
            Log.Sys("NetworkGameOver: " + leftTime);
            if (!overYet)
            {
                overYet = true;
                leftTime = 0;
                //ltui.SetLabel("游戏结束，请点击退出按钮");
                var active = WorldManager.worldManager.GetActive();
                //不能操控也别接受控制命令了 也不发送网络命令了
                active.state = SceneState.GameOver;
                WindowMng.windowMng.PopAllView();

                var player = ObjectManager.objectManager.GetMyPlayer();
                var ai = player.GetComponent<AIBase>().GetAI();
                ai.ChangeStateForce(AIStateEnum.STOP);
                WindowMng.windowMng.PushView("UI/GameOver");

                /*
                var que = new Dictionary<string, object>()
                {
                    {"total", 1},

                };
                StartCoroutine(StatisticsManager.DoWebReq("UpdateRecord"))
                */
            }
        }


        private IEnumerator CountDown()
        {
            while (WorldManager.worldManager.station != WorldManager.WorldStation.Enter)
            {
                yield return null;
            }

            var uiRoot = WindowMng.windowMng.GetMainUI();
            while (uiRoot == null)
            {
                uiRoot = WindowMng.windowMng.GetMainUI();
                yield return null;
            }
            var lt = WindowMng.windowMng.AddChild(uiRoot, Resources.Load<GameObject>("UI/LeftTimeUI"));
            ltui = lt.GetComponent<LeftTimeUI>();
            var sui = WindowMng.windowMng.AddChild(uiRoot, Resources.Load<GameObject>("UI/ScoreUI"));
            scoreUI = sui.GetComponent<ScoreUI>();

            var rtp = Util.FindChildRecursive(uiRoot.transform, "RightTop");
            rtp.gameObject.SetActive(false);

            Color32 white = new Color32(255, 255, 255, 255);
            Color32 red = new Color32(255, 0, 0, 255);
            while (leftTime > 0)
            {
                var color = white;
                if (leftTime <= 60 & leftTime%2 == 0)
                {
                    color = red;
                }
                ltui.SetLabel("" + Util.ConvertTime(leftTime), color);
                leftTime--;
                yield return new WaitForSeconds(1);
            }
        }

        private List<string> killSound = new List<string>()
        {
            "1firstblood",
            "2DoubleKill",
            "3tripleKill",
            "4UltraKill",
            "5Rampage",
            "6Unstoppable",
            "7WhickedSick",
            "8monsterKill",
            "9GodLike",
            "10HolyShit",
        };

        private void PlayerKillSound(int num)
        {
            Log.Sys("PlayerKillSound: " + num);
            var c = num;
            if (num > killSound.Count)
            {
                c = killSound.Count;
            }
            BackgroundSound.Instance.PlayEffect(killSound[c - 1], true);
        }

        public void Dead(GCPlayerCmd cmd)
        {
            var pa = ObjectManager.objectManager.GetPlayer(cmd.DamageInfo.Attacker);
            var pb = ObjectManager.objectManager.GetPlayer(cmd.DamageInfo.Enemy);
            Log.Sys("DeadMsg: " + pa + " pb " + pb);
            if (pa != null && pb != null)
            {
                var aname = pa.GetComponent<NpcAttribute>().userName;
                var bname = pb.GetComponent<NpcAttribute>().userName;
                var killInfo = string.Format("[ff1010]{0}[-]击杀了[10ff10]{1}[-],获得了[ff1010]{2}[-]杀", aname, bname,
                    cmd.AvatarInfo.ContinueKilled);
                Util.ShowMsg(killInfo);
                if (cmd.AvatarInfo.ContinueKilled > 0)
                {
                    PlayerKillSound(cmd.AvatarInfo.ContinueKilled);
                }

                var myId = NetMatchScene.Instance.myId;
                if (cmd.DamageInfo.Attacker == myId)
                {
                    var js = new JSONClass();
                    js.Add("totalKill", new JSONData(1));

                    if (cmd.AvatarInfo.ContinueKilled == 3)
                    {
                        js.Add("threeKill", new JSONData(1));
                    }
                    else if (cmd.AvatarInfo.ContinueKilled == 4)
                    {
                        js.Add("fourKill", new JSONData(1));
                    }
                    else if (cmd.AvatarInfo.ContinueKilled == 5)
                    {
                        js.Add("fiveKill", new JSONData(1));
                    }
                    RecordData.UpdateRecord(js);
                }else if (cmd.DamageInfo.Enemy == myId)
                {
                    var js = new JSONClass();
                    js.Add("dieNum", new JSONData(1));
                    RecordData.UpdateRecord(js);
                }
            }
        }
    }

}