using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TextLocalizeFinder : MonoBehaviour
{   
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetLocalizeDic(Dictionary<string,string> dic)
    {
        string path = "Assets/Resources/Data/dummy.csv";
        StreamWriter writer = new StreamWriter(path, true);

        int LocalizeIndex = 1;
        foreach (KeyValuePair<string,string> iter in dic)
        {
            string key = "LOCALIZE_" + LocalizeIndex++;
            string value = iter.Value.Replace("\n", "\\n");
            writer.WriteLine(key + "\t" + value);
        }

        writer.Close();
    }
}
