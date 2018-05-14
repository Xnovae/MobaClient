using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyLib;

public class AllCell : IUserInterface {

    public EquipConfigData cfg;

    public System.Action callback;
    private void Awake()
    {
        SetCheckBox("_Main", (b) =>
        {
            if(b)
            {
                if(callback != null)
                {
                    callback();
                }
            }
        });
    }
    public override void UpdateUI()
    {
        GetLabel("Label").text = cfg.name;
    
    }
}
