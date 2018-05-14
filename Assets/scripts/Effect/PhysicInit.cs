using UnityEngine;
using System.Collections;

public class PhysicInit : MonoBehaviour
{

    public float KnockBackSpeed = 10;
    public static PhysicInit Instance;

    void Awake()
    {
        Instance = this;
    }
}
