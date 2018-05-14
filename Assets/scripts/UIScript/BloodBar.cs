/*
Author: liyonghelpme
Email: 233242872@qq.com
*/

using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class BloodBar : IUserInterface
    {
        float barDisplay = 0;
        Vector2 pos = new Vector2(20, 40);
        Vector2 size = new Vector2(40, 10);

		
        float curValue = 0;
        GameObject bar;
        UISlider[] fill;
        UILabel label;
        UILabel masterLabel;
        private UILabel NameLabel;

        void Awake()
        {
            bar = GameObject.Instantiate(Resources.Load<GameObject>("UI/BloodBar")) as GameObject;
            bar.transform.parent = WindowMng.windowMng.GetUIRoot().transform;
            Util.InitGameObject(bar);
            var barUI = bar.gameObject.AddComponent<BloodBarUI>();
            label = barUI.GetLabel("SkillLabel");
            label.gameObject.SetActive(false);
            masterLabel = barUI.GetLabel("MasterLabel");
            NameLabel = barUI.GetLabel("NameLabel");

            fill =  new UISlider[1];
            fill[0] = bar.transform.Find("BloodBarAnchor/BloodBar1").gameObject.GetComponent<UISlider>();
            //fill[1] = bar.transform.FindChild("BloodBarAnchor/BloodBar2").gameObject.GetComponent<UISlider>();
            //fill[2] = bar.transform.FindChild("BloodBarAnchor/BloodBar3").gameObject.GetComponent<UISlider>();
            //fill[3] = bar.transform.FindChild("BloodBarAnchor/BloodBar4").gameObject.GetComponent<UISlider>();

        }

        void SetMaster()
        {
            var attr = GetComponent<NpcAttribute>();
            if (attr.IsMine())
            {
                //masterLabel.gameObject.SetActive(true);
                //masterLabel.text = "主机";
                //NameLabel.color = new Color32(71,255,1,255);
            }
            else
            {
                //masterLabel.gameObject.SetActive(false);
                //NameLabel.color = new Color32(255, 255, 255, 255);
            }
        }

        public void HideBar() {
            bar.SetActive(false);
            this.enabled = false;
        }

        public void ShowBar() {
            bar.SetActive(true);
            this.enabled = true;
        }

        private bool isMvp = false;
        void SetTeamColor()
        {
            var mvp = isMvp;
            var attr = GetComponent<NpcAttribute>();
            if (attr == null)
            {
                return;
            }

            if (WorldManager.worldManager.GetActive().ShowTeamColor && attr.GetNetView().IsPlayer)
            {
                Log.GUI("SetTeamColor: "+attr.TeamColor);
                label.gameObject.SetActive(true);
                label.text = "c:" + attr.TeamColor;
            } else
            {
                label.gameObject.SetActive(false);
            }
            if (WorldManager.worldManager.GetActive().ShowNameLabel && attr.GetNetView().IsPlayer)
            {
                NameLabel.gameObject.SetActive(true);
                if (mvp)
                {
                    NameLabel.text = "[ff1010]星战王者_[-]"+"[10f010]"+attr.userName+"[-]";
                }
                else
                {
                    NameLabel.text = "[10f010]"+attr.userName+"[-]";
                }
            }
            else
            {
                NameLabel.gameObject.SetActive(false);
            }
            var sk = attr.GetComponent<BuffComponent>().GetSkillName();
            if (!string.IsNullOrEmpty(sk))
            {
                label.gameObject.SetActive(true);
                label.text = sk;
            }
        }

        private NpcAttribute attribute;
        // Use this for initialization
        void Start()
        {
            attribute = GetComponent<NpcAttribute>();
            Log.GUI("BloodBar Start Event");
            regLocalEvt = new System.Collections.Generic.List<MyEvent.EventType>()
            {
                MyEvent.EventType.UnitHP,
                MyEvent.EventType.TeamColor,
                MyEvent.EventType.IsMaster,
                MyEvent.EventType.BuffChange,
            };
            this.regEvt = new List<MyEvent.EventType>()
            {
               MyEvent.EventType.IAmFirst, 
            };
            RegEvent(true); 

            GetComponent<NpcAttribute>().NotifyHP();
            SetTeamColor();
            SetMaster();
        }

        protected override void OnEvent(MyEvent evt)
        {
            if (evt.type == MyEvent.EventType.IAmFirst)
            {
                var serverId = attribute.GetNetView().GetServerID();
                isMvp = serverId == evt.intArg;
                SetTeamColor();
            }
        }


        protected override void OnLocalEvent(MyEvent evt)
        {	
            Log.Important("Blood bar OnEvent " + gameObject.name + " type " + evt.type + " " + evt.localID + " localId " + GetComponent<KBEngine.KBNetworkView>().GetLocalId());
            Log.Important("Init HP And Max " + GetComponent<NpcAttribute>().HP + " " + GetComponent<NpcAttribute>().HP_Max);
            if (evt.type == MyEvent.EventType.UnitHP)
            {
				
                SetBarDisplay(GetComponent<CharacterInfo>().GetProp(CharAttribute.CharAttributeEnum.HP) * 1.0f / GetComponent<CharacterInfo>().GetProp(CharAttribute.CharAttributeEnum.HP_MAX));
            } else if (evt.type == MyEvent.EventType.TeamColor)
            {
                SetTeamColor();
            } else if (evt.type == MyEvent.EventType.IsMaster)
            {
                SetMaster();
            }
            else
            {
                SetTeamColor();
            }
        }


        void SetBarDisplay(float v)
        {
            barDisplay = v;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            barDisplay = 0;
            fill[0].value = 0;
            //fill[1].value = 0;
            //fill[2].value = 0;
            //fill[3].value = 0;
            GameObject.Destroy(bar);
            //GameObject.Destroy(bar, 0.1f);
        }
        // Update is called once per frame
        void LateUpdate()
        {
            if (CameraController.cameraController == null)
            {
                return;
            }
            if (UICamera.mainCamera == null)
            {
                return;
            }
            Vector3 sp = CameraController.cameraController.GetComponent<Camera>().WorldToScreenPoint(transform.position + new Vector3(0, 2.5f, 0));
            var uiWorldPos = UICamera.mainCamera.ScreenToWorldPoint(sp);
            uiWorldPos.z = 0;
            bar.transform.position = uiWorldPos;
            var ratio = Mathf.FloorToInt(barDisplay * 100);
            fill[0].value = ratio/100.0f;

        }
    }

}