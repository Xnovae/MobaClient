using MyLib;
using UnityEngine;
using System.Collections;

public class EnvParticle : MonoBehaviour
{

    private GameObject go;
    private EnvConfig d;
    public void Load(EnvConfig d1)
    {
        d = d1;
        StartCoroutine(TracePlayer());
    }

    IEnumerator TracePlayer()
    {
        yield return new WaitForSeconds(1);
        while (Camera.main == null)
        {
            yield return null;
        }
	    var active = WorldManager.worldManager.GetActive();
        go = Object.Instantiate(Resources.Load<GameObject>(d.envParticle)) as GameObject;
        var player = ObjectManager.objectManager.GetMyPlayer();
        while (player == null)
        {
            player = ObjectManager.objectManager.GetMyPlayer();
            yield return null;
        }
        while (player != null)
        {
            go.transform.position = player.transform.position;
            yield return null;
        }
    }


}
