
/*
Author: liyonghelpme
Email: 233242872@qq.com
*/

/*
Author: liyonghelpme
Email: 233242872@qq.com
*/
using UnityEngine;
using System.Collections;

public class DumpMono : MonoBehaviour
{
    
}

public class RemoveSelf : MonoBehaviour {
    public GameObject connectDestory;
    public System.Action<GameObject> returnToPool;
	void OnDisable() {
	    if (returnToPool != null)
	    {
	        returnToPool(gameObject);
	    }
	    else
	    {
		    GameObject.Destroy(gameObject);
	    }
        if(connectDestory != null) {
            GameObject.Destroy(connectDestory);
        }
	}

    public IEnumerator WaitReturn(float t)
    {
        yield return new WaitForSeconds(t);
        ParticlePool.Instance.ReturnGameObject(gameObject, ParticlePool.ResetParticle);
    }
}
