using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyLib;

public class ShopUI : IUserInterface
{
    private void Awake()
    {
        SetCallback("Close", ()=>
        {
            WindowMng.windowMng.PopView();
        });
        SetCallback("Buy", OnBuy);
        UpdateUI();
        regEvt = new List<MyEvent.EventType>() {
             MyEvent.EventType.UpdateItem,
        };
        RegEvent(true);
    }

    public override void UpdateUI()
    {
        UpdateAllItem();
        UpdateOwnItem();
        UpdateItemDetail();
        UpdateGoldNum();
    }
    protected override void OnEvent(MyEvent evt)
    {
        UpdateUI();
    }

    private void UpdateItemDetail()
    {
        if(curSelect != null)
        {

        }
    }
    private void UpdateGoldNum()
    {
        if (Util.CheckInGame())
        {
            GetLabel("GoldNum").text = ObjectManager.objectManager.GetMyPlayer().GetComponent<MobaMeSync>().curInfo.Gold.ToString();
        }
    }

    private void UpdateAllItem()
    {
        var grid = GetGrid("AllGrid");
        var cell = GetName("AllCell");
        cell.SetActive(false);
        var allCells = new List<GameObject>();
        foreach(Transform t in grid.transform)
        {
            allCells.Add(t.gameObject);
            t.gameObject.SetActive(false);
        }
        while(allCells.Count < GameData.EquipConfig.Count)
        {
            var c = GameObject.Instantiate<GameObject>(cell);
            c.transform.parent = cell.transform.parent;
            Util.InitGameObject(c.gameObject);
            allCells.Add(c);
        }

        var i = 0;
        foreach(var e in GameData.EquipConfig)
        {
            //var c = GameObject.Instantiate<GameObject>(cell);
            var c = allCells[i];
            var temp = e;
            var ac = c.GetComponent<AllCell>();
            ac.cfg = e;
            ac.callback = () =>
            {
                curSelect = temp;
                UpdateItemDetail();
            };
            ac.UpdateUI();

            c.gameObject.SetActive(true);
            i++;
        }
        grid.repositionNow = true;
    }

    private void UpdateOwnItem()
    {
        var grid = GetGrid("OwnGrid");
        var cell = GetName("OwnCell");
        cell.SetActive(false);
        var allCells = new List<GameObject>();
        foreach (Transform t in grid.transform)
        {
            allCells.Add(t.gameObject);
            t.gameObject.SetActive(false);
        }
        while (allCells.Count < GameData.EquipConfig.Count)
        {
            var c = GameObject.Instantiate<GameObject>(cell);
            c.transform.parent = cell.transform.parent;
            Util.InitGameObject(c.gameObject);
            allCells.Add(c);
        }

        var i = 0;
        var curInfo = ObjectManager.objectManager.GetMyAttr().GetComponent<MobaMeSync>().curInfo;
        var itemList = curInfo.ItemInfoList;
        foreach (var e in itemList)
        {
            var c = allCells[i];
            var temp = e;
            var ac = c.GetComponent<OwnCell>();
            ac.id = e;
            ac.callback = () =>
            {
            };
            ac.UpdateUI();
            c.gameObject.SetActive(true);
            i++;
        }
        grid.repositionNow = true;
    }


    private void OnBuy()
    {
        if (curSelect != null)
        {
            var cg = CGPlayerCmd.CreateBuilder();
            cg.Cmd = "Buy "+curSelect.id;
            NetworkUtil.Broadcast(cg);
        }
    }

    private EquipConfigData curSelect;
}
