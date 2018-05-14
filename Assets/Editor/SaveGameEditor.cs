using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SaveGameEditor  {
    [MenuItem("Tools/SaveGame")]
    public static void SaveGame()
    {
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

}
