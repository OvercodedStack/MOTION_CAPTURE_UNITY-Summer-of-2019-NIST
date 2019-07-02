using UnityEngine;
using System.IO;

/// <summary>
/// Provides some helper utility methods for UniWebView.
/// </summary>
public class UniWebViewHelper {
    /// <summary>
    /// Get the local streaming asset path for a given file path related to the StreamingAssets folder.
    /// 
    /// This method will help you to concat a URL string for a file under your StreamingAssets folder for different platforms.
    /// <param name="path">The relative path to the Assets/StreamingAssets of your file. 
    /// For example, if you placed a html file under Assets/StreamingAssets/www/index.html, you should pass `www/index.html` as parameter.
    /// </param>
    /// <returns>The path you could use as the url for the web view.</returns>
    public static string StreamingAssetURLForPath(string path)
    {
#if (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS) && !UNITY_EDITOR_WIN
        return Path.Combine("file://" + Application.streamingAssetsPath, path);
#elif UNITY_ANDROID
        return Path.Combine("file:///android_asset/", path);
#else
        UniWebViewLogger.Instance.Critical("The current build target is not supported.");
        return string.Empty;
#endif
    }

    /// <summary>
    /// Get the local persistent data path for a given file path related to the data folder of your host app.
    /// 
    /// This method will help you to concat a URL string for a file under you stored in the `persistentDataPath`.
    /// </summary>
    /// <param name="path">
    /// The relative path to the Assets/StreamingAssets of your file.
    /// </param>
    /// <returns>The path you could use as the url for the web view.</returns>
    public static string PersistentDataURLForPath(string path)
    {
        return Path.Combine("file://" + Application.persistentDataPath, path);
    }
}
