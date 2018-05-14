using MyLib;
using UnityEngine;
using System.Collections;

public class DropItemCheck : MonoBehaviour
{
    private void Update()
    {
        var col = Physics.OverlapSphere(transform.position, 2, SkillDamageCaculate.GetDropItemLayer());
        foreach (var c in col)
        {
            //Log.Sys("DropItem: "+c.gameObject);
            if (c.gameObject.GetComponent<DropItemStatic>())
            {
                c.gameObject.GetComponent<DropItemStatic>().PickCol(this.gameObject);
            }
        }
    }
}
