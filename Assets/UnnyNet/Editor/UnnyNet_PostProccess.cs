#if UNITY_IOS
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using UnityEditor.iOS.Xcode;
using System.IO;
 
public class MyPluginPostProcessBuild
{
    [PostProcessBuild]
    public static void ChangeXcodePlist(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if ( buildTarget == BuildTarget.iOS )
        {
            // Get plist
            string plistPath = pathToBuiltProject + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));
           
            // Get root
            PlistElementDict rootDict = plist.root;
           
            rootDict.SetString("NSCameraUsageDescription", "Users can take pictures and send them to UnnyNet");
           
            // Write to file
            File.WriteAllText(plistPath, plist.WriteToString());
        }
    }
}
#endif