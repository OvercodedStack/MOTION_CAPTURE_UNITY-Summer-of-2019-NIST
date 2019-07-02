#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using System.IO;

public class UnnynetAssetsPostProcessBuild : AssetPostprocessor {

    private static readonly string[] FilesToDelete = {"Assets/UnnyNet/Plugins/Android/UniWebView.aar", "Assets/UnnyNet/Plugins/Android/UniWebView.aar.meta", "Assets/UnnyNet/Plugins/iOS/libUniWebView.a", "Assets/UnnyNet/Plugins/iOS/libUniWebView.a.meta"};

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
        foreach (string str in importedAssets) {
            if (str.IndexOf("Assets/UnnyNet", StringComparison.Ordinal) >= 0) {
                foreach (var file in FilesToDelete) {
                    FileUtil.DeleteFileOrDirectory(file);
                }
                return;
            }
        }
    }
}
#endif