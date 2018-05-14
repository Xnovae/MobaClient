using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class DropItemStatic : MonoBehaviour
    {
        public ItemData itemData;
        private GameObject Particle;
        private int num;
        private bool pickYet = false;
        KBEngine.KBNetworkView netView;

        void Awake()
        {
            netView = gameObject.AddMissingComponent<KBEngine.KBNetworkView>();
        }

        void Start()
        {
            var player = ObjectManager.objectManager.GetMyPlayer();
            var c = gameObject.AddComponent<SphereCollider>();
            c.center = new Vector3(0, 0, 0);
            c.radius = 1;
            c.isTrigger = true;
            gameObject.layer = (int)GameLayer.IgnoreCollision;
        }

        public void PickCol(GameObject other)
        {
            var me = ObjectManager.objectManager.GetMyPlayer();
            var attr = NetworkUtil.GetAttr(other.gameObject);
            Log.Sys("OnTriggerEnter: "+attr+" oth "+other.gameObject);
            if (attr != null && attr.gameObject == me && !pickYet)
            {
                pickYet = true;
                PickByMe();
            }
        }

        private void PickByMe()
        {
            var whoAttr = ObjectManager.objectManager.GetMyAttr();
            var cg = CGPlayerCmd.CreateBuilder();
            cg.Cmd = "Pick";
            var pickAction = PickItemAction.CreateBuilder();
            pickAction.Id = netView.GetServerID();
            pickAction.ItemId = itemData.ObjectId;
            pickAction.ItemNum = num;
            pickAction.Who = whoAttr.GetComponent<KBEngine.KBNetworkView>().GetServerID();
            cg.PickAction = pickAction.Build();
            NetworkUtil.Broadcast(cg);
        }


        public void PickItemFromNetwork(GameObject who) {
            pickYet = true;
            var whoAttr = NetworkUtil.GetAttr(who);
            BackgroundSound.Instance.PlayEffect("pickup");
            GameObject.Destroy(gameObject);
            var me = ObjectManager.objectManager.GetMyPlayer();
            Log.Sys("PickItemFromNetwork: "+whoAttr+" itemType: "+itemData.UnitType);

            var gen = Resources.Load<GameObject>("particles/drops/generic_item");
            var par = ParticlePool.Instance.GetGameObject(gen, ParticlePool.InitParticle);
            //var par = Instantiate() as GameObject;

            //par.transform.parent = g.transform;
            par.transform.position = gameObject.transform.position;
            var rm = par.AddMissingComponent<RemoveSelf>();
            rm.StartCoroutine(rm.WaitReturn(3));

            if (whoAttr != null && whoAttr.gameObject == me)
            {
                if (itemData.UnitType == ItemData.UnitTypeEnum.POWER_DRUG)
                {
                    WindowMng.windowMng.ShowNotifyLog("炸弹威力上升");
                    GameInterface_Backpack.LearnSkill((int) SkillData.SkillConstId.Bomb);
                }
                else if (itemData.UnitType == ItemData.UnitTypeEnum.QIPAO_DRUG)
                {
                    var attr = ObjectManager.objectManager.GetMyAttr();
                    attr.AddMpMax(20);
                    attr.AddThrowSpeed(0.1f);
                    WindowMng.windowMng.ShowNotifyLog("MP上限增加");

                }
                else if (itemData.UnitType == ItemData.UnitTypeEnum.XieZi_DRUG)
                {
                    ObjectManager.objectManager.GetMyAttr().AddNetSpeed(0.1f);
                    WindowMng.windowMng.ShowNotifyLog("速度提升");
                }
                else if (itemData.UnitType == ItemData.UnitTypeEnum.LIAN_FA)
                {
                    GameInterface_Skill.AddSkillBuff(me, (int)GameBuff.LianFa, Vector3.zero);
                    WindowMng.windowMng.ShowNotifyLog("连发弹");
                }else if (itemData.UnitType == ItemData.UnitTypeEnum.POTION)
                {
                    GameInterface_Skill.AddSkillBuff(me, (int)GameBuff.AddHP, Vector3.zero);
                    WindowMng.windowMng.ShowNotifyLog("恢复生命");
                }else if (itemData.UnitType == ItemData.UnitTypeEnum.FASHI)
                {
                    var attr = ObjectManager.objectManager.GetMyAttr();
                    attr.SetNetSpeed(0);
                    attr.SetJob(Job.ALCHEMIST);
                    WindowMng.windowMng.ShowNotifyLog("攻击提升", 2, null, true);
                    WindowMng.windowMng.ShowNotifyLog("生命值变化", 2, null, true);
                }else if (itemData.UnitType == ItemData.UnitTypeEnum.CIKE)
                {
                    var attr = ObjectManager.objectManager.GetMyAttr();
                    attr.SetJob(Job.ARMOURER);
                    attr.SetNetSpeed(0.5f);
                    WindowMng.windowMng.ShowNotifyLog("移动速度加快", 2, null, true);
                    WindowMng.windowMng.ShowNotifyLog("生命值变化", 2, null, true);
                }else if (itemData.UnitType == ItemData.UnitTypeEnum.SUPER)
                {
                    GameInterface_Skill.AddBuffWithNet(me, (int)GameBuff.SuperShoot, Vector3.zero);
                    WindowMng.windowMng.ShowNotifyLog("拾获超能子弹", 2, null, true);
                }else if (itemData.UnitType == ItemData.UnitTypeEnum.HIDE)
                {
                    GameInterface_Skill.AddBuffWithNet(me, (int)GameBuff.Hide, Vector3.zero);
                    WindowMng.windowMng.ShowNotifyLog("拾获隐身药水", 2, null, true);
                }else if (itemData.UnitType == ItemData.UnitTypeEnum.NUCLEAR)
                {
                    GameInterface_Skill.AddBuffWithNet(me, (int)GameBuff.NuclearBuff, Vector3.zero);
                    WindowMng.windowMng.ShowNotifyLog("拾获核弹,使用后立即逃离区域", 4, null, true);
                }else if (itemData.UnitType == ItemData.UnitTypeEnum.KNOCK)
                {
                    GameInterface_Skill.AddBuffWithNet(me, (int)GameBuff.KnockBuff, Vector3.zero);
                    WindowMng.windowMng.ShowNotifyLog("获得击退能力", 4, null, true);
                }else if (itemData.UnitType == ItemData.UnitTypeEnum.WUDI)
                {
                    GameInterface_Skill.AddBuffWithNet(me, (int)GameBuff.WuDi, Vector3.zero);
                }else if (itemData.UnitType == ItemData.UnitTypeEnum.DaoDan)
                {
                    GameInterface_Skill.AddBuffWithNet(me, (int) GameBuff.DaoDan, Vector3.zero);
                }else if (itemData.UnitType == ItemData.UnitTypeEnum.DefaultSkill)
                {
                    GameInterface_Skill.AddBuffWithNet(me, itemData.triggerBuffId, Vector3.zero);
                    Util.ShowMsg("获得"+itemData.ItemName);
                }
                else
                {
                    GameInterface_Backpack.PickItem(itemData, num);
                }
            }
        }


        public static GameObject MakeDropItemFromNet(ItemData itemData, Vector3 pos, int num, EntityInfo info)
        {
            var g = Instantiate(Resources.Load<GameObject>(itemData.DropMesh)) as GameObject;
            g.name += "_"+info.Id;
            var com = g.AddComponent<DropItemStatic>();
            com.itemData = itemData;
            g.transform.position = pos;

            var netView = g.GetComponent<KBEngine.KBNetworkView>();
            netView.SetServerID(info.Id);
            netView.IsPlayer = false;
            ObjectManager.objectManager.AddObject(info.Id, netView);

            var spawnId = info.SpawnId;
            var spanwer = ObjectManager.objectManager.GetPlayer(spawnId);
            if (spanwer != null)
            {
                var sp = spanwer.GetComponent<DropAIController>();
                if (sp != null)
                {
                    sp.dropGoods = g;
                }
            }

            com.num = num;
            if (info.ItemId == (int)ItemData.ItemID.SUPER_SHOOT)
            {
                BackgroundSound.Instance.PlayEffect("shockhit", true);
            }
            if (com.itemData.UnitType != ItemData.UnitTypeEnum.POTION)
            {
                g.gameObject.AddComponent<ItemName>();
            }
            return g;
        }

        public static void  MakeDropItem(ItemData itemData, Vector3 pos, int num, int serverId)
        {
            if (NetworkUtil.IsNetMaster())
            {
                var cg = CGPlayerCmd.CreateBuilder();
                cg.Cmd = "AddEntity";
                var etyInfo = EntityInfo.CreateBuilder();
                etyInfo.ItemId = itemData.ObjectId;
                etyInfo.ItemNum = num;
                var po = NetworkUtil.ConvertPos(pos);
                etyInfo.X = po [0];
                etyInfo.Y = po [1];
                etyInfo.Z = po [2];
                etyInfo.SpawnId = serverId;
                etyInfo.EType = EntityType.DROP;
                cg.EntityInfo = etyInfo.Build();
                NetworkUtil.Broadcast(cg);
            }
        }

        static IEnumerator WaitSound(string s)
        {
            yield return new WaitForSeconds(0.2f);
            BackgroundSound.Instance.PlayEffect(s);
        }

        void OnDestroy()
        {
            var view = GetComponent<KBEngine.KBNetworkView>();

            ObjectManager.objectManager.DestroyByLocalId(view.GetLocalId());
        }
    }

}