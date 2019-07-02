#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Reflection;
using UnityEngine;
using UnnyNet.Android;

public class WebViewAndroidStaticListener: MonoBehaviour {
    void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    void OnMessage(string msg) {
        // {method: "", name: "", data: ""}

        UniWebViewLogger.Instance.Verbose("OnMessage: " + msg);

        Message message = null;
        try {
            message = Message.fromString(msg);
        }
        catch (Exception ex) {
            UniWebViewLogger.Instance.Critical("OnMessage: can't parse message => " + ex.Message);
            return;
        }

        var listener = UniWebViewNativeListener.GetListener(message.name);
        if (listener == null) {
            UniWebViewLogger.Instance.Critical("OnMessage: unable to find listener for " + message.name);
            return;
        }

        switch (message.method) {
            case Message.Method.ADD_JS_FINISHED:
                listener.AddJavaScriptFinished(JsonUtility.ToJson(message.Data));
                break;
            case Message.Method.ANIMATE_TO_FINISHED:
                listener.AnimateToFinished(message.Data.data);
                break;
            case Message.Method.EVAL_JS_FINISHED:
                listener.EvalJavaScriptFinished(JsonUtility.ToJson(message.Data));
                break;
            case Message.Method.HIDE_TRANSITION_FINISHED:
                listener.HideTransitionFinished(message.Data.data);
                break;
            case Message.Method.MESSAGE_RECEIVED_LEGACY:
                listener.MessageReceived(JsonUtility.ToJson(message.Data));
                break;
            case Message.Method.MESSAGE_RECEIVED:
                listener.MessageReceived(message.Data.data);
                break;
            case Message.Method.PAGE_ERROR_RECEIVED:
                listener.PageErrorReceived(JsonUtility.ToJson(message.Data));
                break;
            case Message.Method.PAGE_FINISHED:
                listener.PageFinished(JsonUtility.ToJson(message.Data));
                break;
            case Message.Method.PAGE_STARTED:
                listener.PageStarted(message.Data.data);
                break;
            case Message.Method.SHOW_TRANSITION_FINISHED:
                listener.ShowTransitionFinished(message.Data.data);
                break;
            case Message.Method.WEB_VIEW_DONE:
                listener.WebViewDone(message.Data.data);
                break;
            case Message.Method.WEB_VIEW_KEY_DOWN:
                listener.WebViewKeyDown(message.Data.data);
                break;
            default:
                UniWebViewLogger.Instance.Critical("OnMessage: incorrect method " + message.method);
                return;
        }
    }
}
#endif