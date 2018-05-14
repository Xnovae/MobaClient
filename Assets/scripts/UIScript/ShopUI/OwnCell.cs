using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyLib;

public class OwnCell : IUserInterface
{
    public int id;
    public System.Action callback;
    public override void UpdateUI()
    {
        var equipCfg = GMDataBaseSystem.database.SearchId<EquipConfigData>(GameData.EquipConfig, id);
        GetLabel("Label").text = equipCfg.name;
    }
}
