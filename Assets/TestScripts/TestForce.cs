using UnityEngine;
using System.Collections;

public class TestForce : MonoBehaviour
{
    public float f = 100;
    [ButtonCallFunc()] public bool AddForce;

    // Use this for initialization
    public void AddForceMethod()
    {
        this.GetComponent<Rigidbody>().AddExplosionForce(f, transform.parent.position+transform.parent.forward*4, 10);
    }

    void Start()
    {
        AddForceMethod();
    }
    // Update is called once per frame
    private void Update()
    {

    }
}
