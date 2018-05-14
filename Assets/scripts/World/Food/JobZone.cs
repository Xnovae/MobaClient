using UnityEngine;
using System.Collections;

public class JobZone : MonoBehaviour
{
    public float TimeToSpawn = 10;
    public int JobId;

    void Awake()
    {
        gameObject.SetActive(false);
    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
