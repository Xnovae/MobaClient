using UnityEngine;
using System.Collections;

public class TestApplication : MonoBehaviour
{

    [ButtonCallFunc()] public bool Ap;

    public void ApMethod()
    {
        ClientApp.Instance.OnApplicationPause(true);
    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
