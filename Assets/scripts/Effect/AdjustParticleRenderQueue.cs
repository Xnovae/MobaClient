using UnityEngine;
using System.Collections;

public class AdjustParticleRenderQueue : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
	    var ren = this.GetComponent<ParticleSystem>().GetComponent<Renderer>();
        var mat = ren.sharedMaterial;
	    mat.renderQueue = 5000;
	    ren.material = mat;
	}
	
}
