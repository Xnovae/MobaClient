using UnityEngine;
using System.Collections;

public class TestDisable : MonoBehaviour
{
    void OnEnable()
    {
        Debug.LogError("Enable: "+gameObject);
    }

    void OnDisable()
    {
        Debug.LogError("Disable:"+gameObject);
    }
}
