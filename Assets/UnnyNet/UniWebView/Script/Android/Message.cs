#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnnyNet.Android { 
    public class Message {
        public enum Method {
            PAGE_FINISHED = 1,
            PAGE_STARTED = 2,
            PAGE_ERROR_RECEIVED = 3,
            MESSAGE_RECEIVED_LEGACY = 4,
            WEB_VIEW_DONE = 5,
            WEB_VIEW_KEY_DOWN = 6,
            ADD_JS_FINISHED = 7,
            EVAL_JS_FINISHED = 8,
            ANIMATE_TO_FINISHED = 9,
            SHOW_TRANSITION_FINISHED = 10,
            HIDE_TRANSITION_FINISHED = 11,
            MESSAGE_RECEIVED = 12
        }

        public static Message fromString(string obj) {
            var msg = JsonUtility.FromJson<Message>(obj);
            msg.Data = JsonUtility.FromJson<WebViewResult>(msg.data);

            return msg;
        }

        [FormerlySerializedAs("data")]
        [SerializeField]
#pragma warning disable 649
        private string data;
#pragma warning restore 649
    
        [FormerlySerializedAs("method")]
        [SerializeField]
        public Method method;

        [FormerlySerializedAs("name")]
        [SerializeField]
        public string name;

        [NonSerialized]
        public WebViewResult Data;
    }
}
#endif