using MyLib;
using UnityEngine;
using System.Collections;

public class InfoUI : IUserInterface {

    void Awake()
    {
        SetCallback("Ok", Hide);
        
    }
}
