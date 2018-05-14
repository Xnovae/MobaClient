using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
	    transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
	}
	
    /*
	// Update is called once per frame
	void Update ()
	{
	    var cm = Camera.main;
	    if (cm != null)
	    {
	        var cf = cm.transform.localRotation.x;
	        transform.localRotation = Quaternion.Euler(new Vector3(cf+180, 0, 0));
	    }

	}
     */ 
}
