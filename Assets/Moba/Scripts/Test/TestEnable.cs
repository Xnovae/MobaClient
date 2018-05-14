using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnable : MonoBehaviour {

    private void OnEnable()
    {
        Debug.LogError("Enable:"+gameObject);
    }
    private void OnDisable()
    {
        Debug.LogError("Disable:" + gameObject);
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
