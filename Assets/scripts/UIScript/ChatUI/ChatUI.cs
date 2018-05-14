using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class ChatCell : IUserInterface
    {
        public UILabel Name;
        public UILabel Content;
        void Awake()
        {
            Name = GetLabel("Name");
            Content = GetLabel("Content");
        }
    }

    public class ChatUI : IUserInterface
    {
        private UIInput input;
        private long chatId = 0;
        private GameObject Cell;
        private Queue<GameObject> cells = new Queue<GameObject>();
        private UIGrid grid;
        private UIScrollView sv;
        void Awake()
        {
            sv = GetName("ScrollView").GetComponent<UIScrollView>();
            SetCallback("Close", Hide);
            input = GetInput("Input");
            SetCallback("Send", OnSend);
            Cell = GetName("Cell");
            Cell.AddComponent<ChatCell>();
            Cell.SetActive(false);
            grid = GetGrid("Grid");

            InitChat();
            this.regEvt = new List<MyEvent.EventType>()
            {
                MyEvent.EventType.UpdateChat,
            };

            RegEvent();
        }

        private void InitChat()
        {
            foreach (var chatInfo in ChatData.Instance.chatInfo)
            {
                var ord = chatInfo.ord;
                if (ord >= chatId)
                {
                    var cell = GameObject.Instantiate(Cell) as GameObject;
                    cell.transform.parent = Cell.transform.parent;
                    Util.InitGameObject(cell);
                    cell.SetActive(true);
                    var cc = cell.GetComponent<ChatCell>();
                    cc.Name.text = chatInfo.who;
                    cc.Content.text = chatInfo.content;
                    cc.transform.localPosition = new Vector3(0, -cells.Count*50, 0);
                    cells.Enqueue(cell);
                    chatId = ord + 1;
                }
            }
            while (cells.Count >= 30)
            {
                var d = cells.Dequeue();
                GameObject.Destroy(d);
            }

            StartCoroutine(WaitReset());

        }
        protected override void OnEvent(MyEvent evt)
        {
            InitChat();
        }

        private IEnumerator WaitReset()
        {
            yield return null;
            grid.repositionNow = true;
            yield return new WaitForSeconds(0.2f);
            Log.GUI("ScrollView: "+sv.bounds);
            
            //sv.SetDragAmount(1, 0, true);
            //sv.MoveRelative(new Vector3(0, 1000, 0));
            //sv.RestrictWithinBounds();


            var pivot = UIWidget.Pivot.BottomLeft;
			Vector2 pv = NGUIMath.GetPivotOffset(pivot);
			sv.SetDragAmount(pv.x, 1f - pv.y, false);
        }

        private double lastSendTime = 0;
        void OnSend()
        {
            var iv = input.value;
            //var now = Util.GetTimeNow();
            //if (now - lastSendTime < GameConst.Instance.SpeakWaitTime)
            if(now > 0)
            {
                Util.ShowMsg("你说话太快了");
                return;
            }

            if (iv.Length > 20)
            {
                Util.ShowMsg("聊天长度不能超过20个字符");
                return;
            }
            if (iv.Length == 0)
            {
                Util.ShowMsg("不能发送空内容哦");
                return;
            }


            lastSendTime = Util.GetTimeNow();
            var qs = new Dictionary<string, object>()
            {
                {"who", ObjectManager.objectManager.GetMyName() },
                {"content", iv},
            };
            var ret = new string[1];
            StartCoroutine(StatisticsManager.DoWebReq("Chat" + StatisticsManager.QueToStr(qs), ret));

            StartCoroutine(CountTime());
        }

        private int now = 0;
        private IEnumerator CountTime()
        {
            var wt = new WaitForSeconds(1);
            now = GameConst.Instance.SpeakWaitTime;
            while (now > 0)
            {
                input.value = string.Format("{0}秒后可以发言", now);
                now--;
                yield return wt;
            }
            input.value = "";
        }
    }
}