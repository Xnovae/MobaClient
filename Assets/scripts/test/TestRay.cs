using UnityEngine;
using System.Collections;

public class TestRay : MonoBehaviour
{
    public Vector3 pos;
    public Vector3 dir;
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        var tar = pos + dir*10;
        Gizmos.DrawLine(pos, tar);
    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
