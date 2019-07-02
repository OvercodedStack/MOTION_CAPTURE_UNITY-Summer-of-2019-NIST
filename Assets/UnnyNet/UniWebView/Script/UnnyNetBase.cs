using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_ANDROID
using UnnyNet.Android;
#endif
using Object = UnityEngine.Object;

namespace UnnyNet
{
    #region enums
    public enum ChannelType
    {
        Global = 1,
        Direct = 2,
        Guild = 3
    }
    #endregion

    public class UnnyNetBase : MonoBehaviour
    {
        #region CallBacks
        public static Action<Dictionary<string, string>> m_OnPlayerAuthorized;
        public static Action<string> m_OnPlayerNameChanged;
        public static Action<Dictionary<string, string>> m_OnRankChanged;
        public static Action<Dictionary<string, string>> m_OnNewGuild;
        public static Action<Dictionary<string, string>> m_OnAchievementCompleted;
        public static Action<Dictionary<string, string>> m_OnNewMessageReceived;
        public static Action m_OnGameLoginRequest;

        public delegate string UnnyRequest(Dictionary<string, string> prms);
        public static UnnyRequest m_OnNewGuildRequest;
        #endregion

        #region Constants
        public const string JSON_GAME_ID = "gameId";
        public const string JSON_GAME_LOGIN = "game_login";
        public const string JSON_DEFAULT_CHANNEL = "default_channel";
        public const string JSON_PUBLIC_KEY = "public_key";
        public const string JSON_OPEN_WITH_FADE = "open_fade";
        public const string JSON_OPEN_WITH_ANIMATION = "open_animation";


        public const UniWebViewTransitionEdge DEFAULT_ANIMATION = UniWebViewTransitionEdge.Left;

        const float SEND_REQUEST_RETRY_DELAY = 1f;
        #endregion

        const string Error_NotInitialized = "UnnyNet wasn't initialized";
        const int UnnyNetPluginVersion = 2;

        const float ShakeActivationDelay = 3;
        private const float CHECK_PERIOD = 0.3f;
        private const int MAX_RETRIES = 3;
        private float m_Current = 0;

        string m_GameId;
        string m_PublicKey;
        bool m_ActivateOnShake = false;
        bool m_LoginWithCredentials = false;
        string m_DefaultChannel = "general";
        bool m_OpenWithFade = false;
        UniWebViewTransitionEdge m_OpenAnimationDirection = DEFAULT_ANIMATION;

        UniWebView m_WebView;
        static protected UnnyNetBase m_Instance = null;

        int m_LastWinWidth = 0;
        int m_LastWinHeight = 0;

        bool m_WebViewVisible;
        float m_LastShakeActivation;

        private List<CommandInfo> m_Queue = new List<CommandInfo>(); 

        void LoadGameId() {
            TextAsset textAsset = Resources.Load<TextAsset>("unnynet.data");
            if (textAsset != null)
            {
                Dictionary<string, object> json = UnnyNetMiniJSON.Json.Deserialize(textAsset.text) as Dictionary<string, object>;

                if (json.ContainsKey(JSON_GAME_ID))
                    m_GameId = json[JSON_GAME_ID] as string;

                if (json.ContainsKey(JSON_PUBLIC_KEY))
                    m_PublicKey = json[JSON_PUBLIC_KEY] as string;

                if (json.ContainsKey(JSON_GAME_LOGIN))
                    m_LoginWithCredentials = (bool)json[JSON_GAME_LOGIN];

                if (json.ContainsKey(JSON_DEFAULT_CHANNEL))
                    m_DefaultChannel = json[JSON_DEFAULT_CHANNEL] as string;

                if (json.ContainsKey(JSON_OPEN_WITH_FADE))
                    m_OpenWithFade = (bool)json[JSON_OPEN_WITH_FADE];

                if (json.ContainsKey(JSON_OPEN_WITH_ANIMATION))
                    m_OpenAnimationDirection = (UniWebViewTransitionEdge)System.Convert.ToInt32(json[JSON_OPEN_WITH_ANIMATION]);
            }

            if (string.IsNullOrEmpty(m_GameId))
                m_GameId = "8ff16d3c-ebcc-4582-a734-77ca6c14af29";//Default UnnyNet group for developers
        }

        private void Awake() {
            if (m_Instance != null) {
                Debug.LogWarning("[Not harmful] Self Destroying second UnnyNet gameobject. You've probably added UnnyNet prefab to teh scene, which was loaded twice");
                GameObject.Destroy(gameObject);
                return;
            }

            StartCoroutine(LoadQueue());
            LoadGameId();

            UniWebViewLogger.Instance.LogLevel = UniWebViewLogger.Level.Off;
            m_WebView = GetComponent<UniWebView>();
            //UniWebView.SetWebContentsDebuggingEnabled(true);
            m_ActivateOnShake = PlayerPrefs.GetInt("UnnyNet_ActivateOnShake", m_ActivateOnShake ? 1 : 0) == 1;
            m_Instance = this;
            DontDestroyOnLoad(gameObject);
            m_Instance.m_WebView.OnMessageReceived += OnOnMessageReceived;
            m_Instance.m_WebView.OnPageErrorReceived += OnPageErrorReceived;

            Init();

            UpdateWebFrame();

            m_WebViewVisible = false;
#if !UNITY_EDITOR
            m_WebView.BackgroundColor = new Color(54f / 255, 57f / 255, 63f / 255, 1);
#endif
        }

        private string LoadErrPage() {
            var txt = Resources.Load("unnynet.com.error") as TextAsset;
            return txt != null ? txt.text : null;
        }
        private void OnPageErrorReceived(UniWebView webView, int errorCode, string errorMessage) {
            string errPage = LoadErrPage();
            if (string.IsNullOrEmpty(errPage)) {
                UniWebViewLogger.Instance.Critical("No error page");
                return;
            }

            webView.LoadHTMLString(errPage, "error");
        }

        private const string ExtStoragePermissionStatus = "ExtStoragePermissionStatus";
        public const int PERMISSION_GRANTED = 0;
        public const int PERMISSION_DENIED = -(1 << 1);
        public const int PERMISSION_DENIED_AND_NEVER_AGAIN = -(1 << 2);

#if UNITY_ANDROID
        private const int DEF_PERMISSION_STATUS = PERMISSION_DENIED;
#else
        private const int DEF_PERMISSION_STATUS = PERMISSION_GRANTED;
#endif

        private void OnAttachmentPermissionAsked(bool force)
        {
#if UNITY_ANDROID
            OnAttachmentPermissionAskedAndroid(force);
#else
            var status = PlayerPrefs.GetInt(ExtStoragePermissionStatus, DEF_PERMISSION_STATUS);
            SetAttachmentsPermissionStatus(status);
            if (force) { }
#endif
        }

#if UNITY_ANDROID
        private void OnAttachmentPermissionAskedAndroid(bool force) {
            var status = PlayerPrefs.GetInt(ExtStoragePermissionStatus, DEF_PERMISSION_STATUS);
            switch (status) {
                case PERMISSION_GRANTED:
                    SetAttachmentsPermissionStatus(status);
                    break;
                case PERMISSION_DENIED:
                    UnityAndroidPermissions.RequestPermission(UnityAndroidPermissions.WRITE_EXTERNAL_STORAGE, new AndroidPermissionCallback((permission, res) => {
                        OnExtStoragePermissionResult(res);
                    }));
                    break;
                case PERMISSION_DENIED_AND_NEVER_AGAIN:
                    if(force)
                        UnityAndroidPermissions.RequestPermission(UnityAndroidPermissions.WRITE_EXTERNAL_STORAGE, new AndroidPermissionCallback((permission, res) => {
                            OnExtStoragePermissionResult(res);
                        }));
                    else
                        SetAttachmentsPermissionStatus(status);
                    break;
            }
        }

        private void OnExtStoragePermissionResult(int res) {
            Debug.Log("OnExtStoragePermissionResult: " + res);
            PlayerPrefs.SetInt(ExtStoragePermissionStatus, res);
            SetAttachmentsPermissionStatus(res);
        }
#endif

        public static void OpenUnnyNet()
        {
#if UNITY_EDITOR_WIN
            Debug.LogWarning("UnnyNet doesn't support Unity Editor. Please make an iOS or Android build to test everything!");
#endif
            if (m_Instance != null)
            {
                if (m_Instance.m_WebView == null)
                {
                    GameObject.Destroy(m_Instance.gameObject);
                    m_Instance = null;
                    InitializeUnnyNet();
                }
            }
            else
            {
                Debug.LogError("UnnyNet Error: you should call UnnyNet.UnnyNet.InitializeUnnyNet();");
                InitializeUnnyNet();//Just in case developer forgets to call it
            }
            m_Instance.ShowWebView();
        }

        public static void InitializeUnnyNet()
        {
            if (m_Instance != null)
            {
                if (m_Instance.m_WebView == null)
                {
                    GameObject.Destroy(m_Instance.gameObject);
                    m_Instance = null;
                } else 
                    return;
            }

            Object prefab = Resources.Load("UnnyNet");
            GameObject.Instantiate(prefab);
        }

        private void Init()
        {
            ApplyStartURL();
        }

        string GetAdditionalPath()
        {
            string deviceId = Utils.GetUniqId();

            List<string> prms = new List<string>();
            if (deviceId != null)
                prms.Add("device_id=" + deviceId);
            prms.Add("version=" + UnnyNetPluginVersion);
            if (m_LoginWithCredentials)
                prms.Add("game_login=1");
            if (!string.IsNullOrEmpty(m_PublicKey))
                prms.Add("public_key=" + m_PublicKey);
#if UNITY_EDITOR
            prms.Add("editor=1");
#endif

            string addPath = "/#/plugin/" + m_GameId + "?";

            for (int i = 0; i < prms.Count - 1; i++)
                addPath += prms[i] + "&";
            addPath += prms[prms.Count - 1];

            return addPath;
        }

        void ApplyStartURL()
        {
#if UNNY_TEST
            m_GameId = "e3063509-2a56-49f2-9527-763a944c1378";
            string addPath = GetAdditionalPath();
            m_WebView.urlOnStart = "http://localhost:8888/" + addPath;
            //m_WebView.urlOnStart = "https://test-nakama-react2.unnynet.com/" + addPath;
#else
            string addPath = GetAdditionalPath();
            m_WebView.urlOnStart = "https://unnynet.com" + addPath;
#endif
        }

        void UpdateWebFrame()
        {
            m_LastWinWidth = Screen.width;
            m_LastWinHeight = Screen.height;
            int width, height;
#if UNITY_EDITOR
            if (m_LastWinWidth > m_LastWinHeight) {
                width = Math.Max(m_LastWinWidth, 1000);
                height = Math.Max(m_LastWinHeight, 800);
            }
            else {
                width = Math.Max(m_LastWinWidth, 800);
                height = Math.Max(m_LastWinHeight, 1000);
            }
#else
            width = m_LastWinWidth;
            height = m_LastWinHeight;
#endif
            m_WebView.Frame = new Rect(0, 0, width, height);
        }

        private void OnAction(UniWebViewMessage message) {
            if (message.Args.ContainsKey("exit")) {
                HideWebView();
            }
            if (message.Args.ContainsKey("askpermission")) {
                OnAttachmentPermissionAsked(message.Args.ContainsKey("force"));
            }
            if (message.Args.ContainsKey("retry")) {
                m_WebView.Load(m_WebView.urlOnStart, true);
            }
        }

        private void OnOnMessageReceived(UniWebView webView, UniWebViewMessage message) {
            switch (message.Path) {
                case "action":
                    OnAction(message);
                    break;
                case "authorised":
                    SetWebView();
                    var status = PlayerPrefs.GetInt(ExtStoragePermissionStatus, DEF_PERMISSION_STATUS);
                    SetAttachmentsPermissionStatus(status);
                    if (m_OnPlayerAuthorized != null)
                        m_OnPlayerAuthorized(message.Args);
                    SetDefaultChannel(m_DefaultChannel);
                    break;

                case "renamed":
                    if (m_OnPlayerNameChanged != null)
                        m_OnPlayerNameChanged(message.Args["name"]);
                    break;
                case "rank_changed":
                    if (m_OnRankChanged != null)
                        m_OnRankChanged(message.Args);
                    break;
                case "new_guild":
                    if (m_OnNewGuild != null)
                        m_OnNewGuild(message.Args);
                    break;
                case "ask_new_guild":
                    {
                        ApplyRequest(message.Args, m_OnNewGuildRequest);
                        break;
                    }
                case "ach_completed":
                    if (m_OnAchievementCompleted != null)
                        m_OnAchievementCompleted(message.Args);
                    break;
                case "new_message":
                    if (m_OnNewMessageReceived != null)
                        m_OnNewMessageReceived(message.Args);
                    break;
                case "game_login_request":
                    if (m_OnGameLoginRequest != null)
                        m_OnGameLoginRequest();
                    break;
                case "request_reply":
                    RequestsManager.ReplyReceived(message.Args);
                    break;
            }
            //Debug.LogError(message.RawMessage);
        }

        void ApplyRequest(Dictionary<string, string> args, UnnyRequest request) {
            string error = null;
            if (request != null)
                error = request(args);

            string sysId = args["sys_id"];
            if (error != null)
                SendRequestFailed(sysId, error);
            else
                SendRequestSucceeded(sysId);
        }

        void SendRequestFailed(string sys_id, string err)
        {
            string code = string.Format(UnnynetCommand.GetCommand(UnnynetCommand.Command.RequestFailed), sys_id, err);
            EvaluateCodeInJavaScript(new CommandInfo(UnnynetCommand.Command.RequestFailed, code, false));
        }

        void SendRequestSucceeded(string sys_id)
        {
            string code = string.Format(UnnynetCommand.GetCommand(UnnynetCommand.Command.RequestSucceeded), sys_id);
            EvaluateCodeInJavaScript(new CommandInfo(UnnynetCommand.Command.RequestSucceeded, code, false));
        }

        void HideWebView()
        {
            m_WebViewVisible = false;
            m_WebView.Hide();
        }

        void ShowWebView()
        {
            m_WebViewVisible = true;
            m_WebView.Show(m_OpenWithFade, m_OpenAnimationDirection);
        }

        private void Update() {
            if (m_ActivateOnShake) {
                float time = Time.realtimeSinceStartup;
                if (m_LastShakeActivation + ShakeActivationDelay < time) {
                    Vector3 acc = Input.acceleration;
                    if (acc.sqrMagnitude >= 20) {
                        m_LastShakeActivation = time;
                        if (m_WebViewVisible)
                            HideWebView();
                        else
                            ShowWebView();
                    }
                }
            }
            if (m_LastWinWidth != Screen.width || m_LastWinHeight != Screen.height) {
                UpdateWebFrame();
                m_WebView.UpdateFrame();
            }

            m_Current += Time.deltaTime;
            if (m_Current > CHECK_PERIOD) {
                m_Current = 0;
                CheckQueue();
            }
        }

        private void SetDefaultChannel(string channelName) {
            string code = string.Format(UnnynetCommand.GetCommand(UnnynetCommand.Command.SetDefaultChannel), channelName);
            EvaluateCodeInJavaScript(new CommandInfo(UnnynetCommand.Command.SetDefaultChannel, code, false, payload => {
                if (!payload.resultCode.Equals("0"))
                    Debug.LogError("SetDefaultChannel Error: " + payload.data);
            }));
        }

        protected static bool CheckWebView(CommandInfo info = null) {
            bool q = (m_Instance != null && m_Instance.m_WebView != null);
            if (!q) {
                Debug.LogError("UnnyNet wasn't properly initialized");
                if (info != null)
                    info.EvaluateCallback(new ResponseData {Success = false, Error = new Error {Code = (int) Errors.NotInitialized, Message = Error_NotInitialized}});


                InitializeUnnyNet();
            }

            return q;
        }

        protected static ResponseData SendCallback(UniWebViewNativeResultPayload payload, CommandInfo info) {
            if (!payload.resultCode.Equals("0")) {
                if (info != null)
                    info.EvaluateCallback(new ResponseData {Success = false, Error = new Error {Code = (int) Errors.Unknown, Message = "Error occurred: " + payload.resultCode}});

                ResponseData unNotReadyError = new ResponseData();
                unNotReadyError.Success = false;
                unNotReadyError.Error = Error.GetUnnyNetNotReadyError();
                return unNotReadyError;
            }
            else {
                if (!payload.data.Equals("0")) {
                    try {
                        ResponseData data = string.IsNullOrEmpty(payload.data) ? new ResponseData {Success = false, Error = new Error {Code = (int) Errors.UnnynetNotReady}} : JsonUtility.FromJson<ResponseData>(payload.data);
                        if (!data.Success) {
                            if (info != null)
                                info.EvaluateCallback(new ResponseData {Success = false, Error = data.Error});
//                            switch ((Errors) data.Error.Code) {
//                                case Errors.UnnynetNotReady:
//                                    return data;
//                            }
                            return data;
                        }
                        else {
                            if (info != null)
                                info.EvaluateCallback(new ResponseData { Success = true });
                        }
                    }
                    catch (Exception ex) {
                        Debug.LogError(ex.StackTrace);

                        if (info != null)
                            info.EvaluateCallback(new ResponseData {Success = false, Error = new Error {Code = (int) Errors.Unknown, Message = payload.data}});
                    }
                }
                else {
                    if (info != null)
                        info.EvaluateCallback(new ResponseData { Success = true});
                }
            }

            return null;
        }

        public static void SetAttachmentsPermissionStatus(int status)
        {
            SetConfig("{\"attachmentsPermissionStatus\": " + status + "}");
        }

        public static void SetWebView(bool webView = true)
        {
            SetConfig("{\"webView\": " + (webView ? "true" : "false") + "}");
        }

        public static void SetIsAttachmentsEnabled(bool attachmentsEnabled)
        {
            SetConfig("{\"attachmentsEnabled\": " + (attachmentsEnabled ? "true" : "false") + "}");
        }

        public static void SetConfig(string config) {
            string code = string.Format(UnnynetCommand.GetCommand(UnnynetCommand.Command.SetConfig), config);
            EvaluateCodeInJavaScript(new CommandInfo(UnnynetCommand.Command.SetConfig, code, false));
        }

        protected static void EvaluateCommand(UnnynetCommand.Command command, bool openWindow, UnityAction<ResponseData> doneCallback)
        {
            EvaluateCodeInJavaScript(new CommandInfo(command, UnnynetCommand.GetCommand(command), openWindow, doneCallback));
        }

        protected static void EvaluateCodeInJavaScript(CommandInfo info, bool highPriority = false) {
            CheckWebView();

            int indexToReplace = -1;
            for (int i = 1; i < m_Instance.m_Queue.Count; i++) {
                if (m_Instance.m_Queue[i].CouldBeReplaced(info.Command)) {
                    indexToReplace = i;
                    break;
                }
            }

            if (indexToReplace == -1) {
                if (highPriority)
                    m_Instance.m_Queue.Insert(0, info);
                else
                    m_Instance.m_Queue.Add(info);

//                if (!CheckWebView(info.Callback))
//                    return;

//                CheckQueue();
            }
            else
                m_Instance.m_Queue[indexToReplace] = info;
        }

        private const string PrefsKey = "UN_commands";
        private void SaveQueue() {
            if (m_Queue.Count == 0)
                return;

            string commands = JsonUtility.ToJson(new CommandsInfo(m_Queue.FindAll(cmd => cmd.NeedToSave())));

            PlayerPrefs.SetString(PrefsKey, commands);
            PlayerPrefs.Save();
            Debug.LogError(commands);
        }

        private IEnumerator LoadQueue() {
            string commands = PlayerPrefs.GetString(PrefsKey, "");
            if (string.IsNullOrEmpty(commands))
                yield break;

            CommandsInfo cmds = JsonUtility.FromJson<CommandsInfo>(commands);
//            for (int i = 0; i < cmds.m_Commands.Length; i++) {
//                Debug.LogWarning("cmds: " + cmds.m_Commands[i].Command.ToString());
//            }

            while ((m_Instance == null || m_Instance.m_WebView == null)) {
                yield return new WaitForSeconds(0.1f);
            }

            m_Queue.AddRange(cmds.m_Commands);

            if (PlayerPrefs.HasKey(PrefsKey)) {
                PlayerPrefs.DeleteKey(PrefsKey);
                PlayerPrefs.Save();
            }
        }

        private void OnApplicationPause(bool isPaused) {
            if (isPaused)
                SaveQueue();
            else
                StartCoroutine(LoadQueue());
        }

        private static void CheckQueue() {
            if (m_Instance.m_Queue.Count == 0)
                return;

            CommandInfo info = m_Instance.m_Queue[0];
            if (!CheckWebView(info))
                return;
            if (info.StartedTime == null) {
                info.StartedTime = DateTime.UtcNow;
            }
            else {
                double secs = DateTime.UtcNow.Subtract(info.StartedTime.Value).TotalSeconds;
                if (secs > SEND_REQUEST_RETRY_DELAY) {
                    info.StartedTime = DateTime.UtcNow;
//                    info.Retries++;
//                    if (info.Retries > MAX_RETRIES) {
//                        UniWebViewLogger.Instance.Debug("Max retries: " + info.Command.ToString());
//                    }
                }
                else {
                    return;
                }
            }
            DateTime? original = info.StartedTime;

            m_Instance.m_WebView.EvaluateJavaScript(info.Code, (payload) => {
                info.EvaluateCallbackNative(payload);

                if (original == info.StartedTime) {
                    ResponseData resp = SendCallback(payload, info);
                    if (resp == null || resp.Success) {
                        m_Instance.m_Queue.RemoveAt(0);
                        if (info.OpenWindow)
                            OpenUnnyNet();
                    }
                    else {
                        switch (resp.Error.Code) {
                            case (int)Errors.UnnynetNotReady:
                                UniWebViewLogger.Instance.Debug("Not ready, w8");
                                break;
                            case (int)Errors.NotAuthorized:
                                UniWebViewLogger.Instance.Debug("Not authorized, w8");
                                break;
                            default:
                                m_Instance.m_Queue.RemoveAt(0);
                                break;
                        }
                        
                    }
                }
            });
        }

        ////In progress
        //public static void SendSystemMessageToChannel(string channelName, string message)
        //{
        //    if (!CheckWebView())
        //        return;
        //    string code = String.Format("window.globalReactFunctions.apiSendSystemMessage('{0}', '{1}')", channelName, message);
        //    m_Instance.m_WebView.EvaluateJavaScript(code, (payload) => {
        //        if (!payload.resultCode.Equals("0"))
        //            Debug.LogError("SendSystemMessage Error: " + payload.data);
        //    });
        //}
    }
}