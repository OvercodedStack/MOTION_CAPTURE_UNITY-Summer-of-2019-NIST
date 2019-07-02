using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_2018_1_OR_NEWER
using WrappedWWW = UnityEngine.Networking.UnityWebRequest;
#else
using WrappedWWW = UnityEngine.WWW;
#endif
/// <summary>
/// A structure represents a message from webview.
/// </summary>
public struct UniWebViewMessage {
    /// <summary>
    /// Gets the raw message. It is the original url which initialized this message.
    /// </summary>
    public string RawMessage {get; private set;}

    /// <summary>
    /// The url scheme of this UniWebViewMessage. "uniwebview" was added to message scheme list
    /// by default. You can add your own scheme by using `UniWebView.AddUrlScheme`.
    /// </summary>
    public string Scheme {get; private set;}

    /// <summary>
    /// The path of this UniWebViewMessage.
    /// This will be the decoded value for the path of original url.
    /// </summary>
    public string Path {get; private set;}

    /// <summary>
    /// The arguments of this UniWebViewMessage.
    ///
    /// When received url "uniwebview://yourPath?param1=value1&param2=value2", 
    /// the args is a Dictionary with: Args["param1"] = value1, Args["param2"] = value2
    /// 
    /// Both the key and valud will be url decoded from the original url.
    /// </summary>
    public Dictionary<string, string> Args{get; private set;}

    /// <summary>
    /// Initializes a new instance of the `UniWebViewMessage` struct.
    /// </summary>
    /// <param name="rawMessage">Raw message which will be parsed to a UniWebViewMessage.</param>
    public UniWebViewMessage(string rawMessage): this() {
        UniWebViewLogger.Instance.Debug("Try to parse raw message: " + rawMessage);
        this.RawMessage = WrappedWWW.UnEscapeURL(rawMessage);
        
        string[] schemeSplit = rawMessage.Split(new string[] {"://"}, System.StringSplitOptions.None);

        int index;
        if (schemeSplit.Length == 1) {
            index = 0;
            this.Scheme = "unnynet";
        } else {
            index = 1;
            this.Scheme = schemeSplit[0];
        }
        //if (schemeSplit.Length >= 2) {
            //this.Scheme = schemeSplit[0];
            UniWebViewLogger.Instance.Debug("Get scheme: " + this.Scheme);

            string pathAndArgsString = "";

            while (index < schemeSplit.Length) {
                pathAndArgsString = string.Concat(pathAndArgsString, schemeSplit[index]);
                index++;
            }
            UniWebViewLogger.Instance.Verbose("Build path and args string: " + pathAndArgsString);
            
            string[] split = pathAndArgsString.Split("?"[0]);
            
            this.Path = WrappedWWW.UnEscapeURL(split[0].TrimEnd('/'));
            this.Args = new Dictionary<string, string>();
            if (split.Length > 1) {
                foreach (string pair in split[1].Split("&"[0])) {
                    string[] elems = pair.Split("="[0]);
                    if (elems.Length > 1) {
                        var key = WrappedWWW.UnEscapeURL(elems[0]);
                        if (Args.ContainsKey(key)) {
                            var existingValue = Args[key];
                            Args[key] = existingValue + "," + WrappedWWW.UnEscapeURL(elems[1]);
                        } else {
                            Args[key] = WrappedWWW.UnEscapeURL(elems[1]);
                        }
                        UniWebViewLogger.Instance.Debug("Get arg, key: " + key + " value: " + Args[key]);
                    }
                }
            }
        //} else {
        //    UniWebViewLogger.Instance.Critical("Bad url scheme. Can not be parsed to UniWebViewMessage: " + rawMessage);
        //}
    }
}