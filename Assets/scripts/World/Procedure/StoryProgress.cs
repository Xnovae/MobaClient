using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class StoryProgress : MonoBehaviour
    {
        void Awake()
        {
            Log.Sys("StoryProcess");
        }
        // Use this for initialization
        void Start()
        {
            WindowMng.windowMng.PushView("UI/StoryDialog");
        }
    
        // Update is called once per frame
        void Update()
        {
    
        }
    }

}