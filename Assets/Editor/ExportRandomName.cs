using System.IO;
using System.Text;
using UnityEngine;
using System.Xml;
using UnityEditor;

public class ExportRandomName : ScriptableObject
{
    [MenuItem("Tools/Export Random Name")]
    static void ExportRandomNameToCs()
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(Application.dataPath+ "/../Scripts/random_name.xml");

        var templateString = File.ReadAllText(Application.dataPath + "/Editor/Templates/CSTemplate.txt", Encoding.UTF8);

        var root = xmlDoc.SelectSingleNode("root");
        var firstName = "";
        var lastName = "";

        foreach (var childNode in root.ChildNodes)
        {
            
            var childElement = (XmlElement)childNode;
            var nameMap = childElement.ChildNodes;
            foreach (var node in nameMap)
            {
                var element = (XmlElement)node;
                if (element.Name == "lib1")
                {
                    firstName += "\t\t\"" +element.InnerText+ "\",\n";
                }
                else if (element.Name == "lib2")
                {
                    lastName += "\t\t\"" + element.InnerText + "\",\n";
                }
            }
        }
        
        templateString = templateString.Replace("$$firstName$$", firstName);
        templateString = templateString.Replace("$$lastName$$", lastName);
     
        File.WriteAllText(Application.dataPath + "/scripts/util/RandomName.cs", templateString,Encoding.UTF8);
        AssetDatabase.Refresh();
    }
}
