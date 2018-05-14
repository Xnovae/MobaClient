using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyLib;

public class MobaModelLoader : MonoBehaviour {
    private GameObject model;
    private NpcAttribute attr;

    private SkinnedMeshRenderer[] skins;

    private void Start()
    {
        attr = GetComponent<NpcAttribute>();
    }

    public void LoadModel(int modelId)
    {
        var udata = Util.GetUnitData(true, modelId, 0);
        model = Object.Instantiate<GameObject>(Resources.Load<GameObject>(udata.ModelName));
        var scale = model.transform.localScale;
        model.transform.parent = transform;
        Util.InitGameObject(model);
        model.transform.localScale = scale;
        var attri = GetComponent<NpcAttribute>();
        MyEventSystem.myEventSystem.PushLocalEvent(attri.GetLocalId(), MyEvent.EventType.UpdateModel);
        skins = model.GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    private bool inGrassYet = false;
    /// <summary>
    /// 自己队友则Alpha显示
    /// 敌人则不显示
    /// </summary>
    public void SetInGrass(bool inG)
    {
        if (inGrassYet != inG && skins != null)
        {
            inGrassYet = inG;
            if (inG)
            {
                var teamColor = attr.TeamColor;
                var me = ObjectManager.objectManager.GetMyAttr();
                if (me.TeamColor == teamColor)
                {
                    foreach(var s in skins)
                    {
                        var c = s.material.color;
                        c.a = 0.5f;
                        s.material.color = c;
                        s.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        s.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        s.material.SetInt("_ZWrite", 0);
                        s.material.EnableKeyword("IN_GRASS");
                        s.material.renderQueue = 3000;
                    }
                    model.SetActive(true);
                }else
                {
                    model.SetActive(false);
                }
            }
            else
            {
                foreach(var s in skins)
                {
                    var c = s.material.color;
                    c.a = 1f;
                    s.material.color = c;
                    s.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    s.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    s.material.SetInt("_ZWrite", 1);
                    s.material.DisableKeyword("IN_GRASS");
                    s.material.renderQueue = 2000;
                }
                model.SetActive(true);
            }
        }
    }
}
