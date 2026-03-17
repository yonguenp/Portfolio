using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public static class SamandaPostBuild
{
    private const string REVERSE_URL = "com.googleusercontent.apps.622508035915-a88r7lqhs75cfacfat5srid8eobqbomn";

    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        // common

        // target specific
        switch(buildTarget)
        {
            case BuildTarget.iOS:
                PostProcessiOS(path);
                break;

            default:
                break;
        }
    }

    private static void PostProcessiOS(string path)
    {
        // pbxproj
        {
            string projectPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
            
            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(projectPath);

            //string target = pbxProject.TargetGuidByName("Unity-iPhone");
            //pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");

            pbxProject.WriteToFile(projectPath);
        }

        // Info.plist
        {
            // Load Info.plist
            string infoPlistPath = path + "/Info.plist";
            
            PlistDocument plistDoc = new PlistDocument();
            plistDoc.ReadFromFile(infoPlistPath);
            if (plistDoc.root == null)
            {
                Debug.LogError("ERROR: Can't open " + infoPlistPath);
                return;
            }

            // Our URLTypes element
            PlistElementDict urlElem = null;
            if (plistDoc.root.values.ContainsKey("CFBundleURLTypes"))
            {
                PlistElementArray urlArray = plistDoc.root.values["CFBundleURLTypes"].AsArray();
                if (null == urlArray)
                {
                    Debug.LogError("ERROR: Info.plist.root.CFBundleURLTypes is not an array");
                    return;
                }

                urlElem = urlArray.AddDict();
            }
            else
            {                
                PlistElementArray urlArray = new PlistElementArray();
                plistDoc.root.values.Add("CFBundleURLTypes", urlArray);

                urlElem = urlArray.AddDict();
            }

            // Set Role and URL Schemes
            urlElem.SetString(key: "CFBundleTypeRole", val: "Editor");
            PlistElementArray innerArray = urlElem.CreateArray("CFBundleURLSchemes");
            innerArray.AddString(REVERSE_URL);

            // Save
            plistDoc.WriteToFile(infoPlistPath);
        }
    }
}
