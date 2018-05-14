using UnityEngine;
using System.Collections;

public class SpawnFood : MonoBehaviour
{
    public float TimeToSpawn = 10;

    void Awake()
    {
        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(false);
        }
    }

}
