using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

namespace UnnyNet
{
    public class UnnyNet_Editor : EditorWindow
    {
        const string UnnyNetDefine = "UNNYNET";

        public static void AddUnnyNetSymbols()
        {
            foreach (BuildTarget target in System.Enum.GetValues(typeof(BuildTarget)))
            {
                BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);

                if (group == BuildTargetGroup.Unknown)
                {
                    continue;
                }

                string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);

                var defineSymbols = defines
                    .Split(';')
                    .Select(d => d.Trim())
                    .ToList();

                bool found = false;
                foreach (var symbol in defineSymbols)
                {
                    if (UnnyNetDefine.Equals(symbol))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    try
                    {
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Format("{0};{1}", defines, UnnyNetDefine));
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogErrorFormat("Could not add UnnyNet define symbols for build target: {0} group: {1}, {2}", target, group, e);
                    }
                }
            }
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            AddUnnyNetSymbols();
        }

        const string jsonPath = "Assets/UnnyNet/Resources/unnynet.data.json";

        static void SaveUnnyJson(Dictionary<string, object> json)
        {
            string finalString = UnnyNetMiniJSON.Json.Serialize(json);

            StreamWriter writer = new StreamWriter(jsonPath, false);
            writer.WriteLine(finalString);
            writer.Close();
            AssetDatabase.ImportAsset(jsonPath);
        }

        static Dictionary<string, object> CreateUnnyJson(bool load)
        {
            TextAsset textAsset = (TextAsset)AssetDatabase.LoadAssetAtPath(jsonPath, typeof(TextAsset));
            if (textAsset == null)
            {
                Dictionary<string, object> json = new Dictionary<string, object>();

                json.Add("gameId", string.Empty);
                json.Add("guests", true);
                json.Add("game_login", false);
                json.Add("channel", "general");
                json.Add("public_key", string.Empty);

                SaveUnnyJson(json);

                return json;
            }
            else
            {
                if (load)
                    return UnnyNetMiniJSON.Json.Deserialize(textAsset.text) as Dictionary<string, object>;
            }

            return null;
        }

        #region Settings Window
        [MenuItem("UnnyNet/Settings")]

        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(UnnyNet_Editor));
        }

        Dictionary<string, object> m_UnnyJson;
        string m_GameId;
        bool m_GameLogin;
        string m_DefaultChannel;
        string m_PublicKey;

        bool m_OpenWithFade;
        int m_OpenWithAnimation;

        string[] m_AnimationNames;

        private void Awake()
        {
            this.minSize = new Vector2(450, 200);

            m_AnimationNames = new string[5];
            for (int i = 0; i < m_AnimationNames.Length; i++)
                m_AnimationNames[i] = ((UniWebViewTransitionEdge)i).ToString();
        }

        private T GetJsonValue<T>(string key, T def) {
            if (m_UnnyJson.ContainsKey(key))
                return (T)m_UnnyJson[key];
            return def;
        }

        void OnEnable()
        {
            m_UnnyJson = CreateUnnyJson(true);
            m_GameId = GetJsonValue<string>(UnnyNetBase.JSON_GAME_ID, string.Empty);
            m_GameLogin = GetJsonValue<bool>(UnnyNetBase.JSON_GAME_LOGIN, false);
            m_DefaultChannel = GetJsonValue<string>(UnnyNetBase.JSON_DEFAULT_CHANNEL, "general");
            m_PublicKey = GetJsonValue<string>(UnnyNetBase.JSON_PUBLIC_KEY, string.Empty);
            m_OpenWithFade = GetJsonValue<bool>(UnnyNetBase.JSON_OPEN_WITH_FADE, false);
            m_OpenWithAnimation = System.Convert.ToInt32(GetJsonValue<object>(UnnyNetBase.JSON_OPEN_WITH_ANIMATION, (int)UnnyNetBase.DEFAULT_ANIMATION));
        }

        void OpenHash(string hash)
        {
            Application.OpenURL("https://docs.developers.unnynet.com/" + hash);
        }

        bool m_AnyChanges;

        void SetColor(bool anyChanges) {
            GUI.color = anyChanges ? Color.green : Color.white;
            if (anyChanges)
                m_AnyChanges = true;
        }

        void OnGUI()
        {
            m_AnyChanges = false;
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("UnnyNet Settings");
            SetColor(m_GameId != GetJsonValue<string>(UnnyNetBase.JSON_GAME_ID, string.Empty));
            m_GameId = EditorGUILayout.TextField("Game ID", m_GameId);

            SetColor(m_PublicKey != GetJsonValue<string>(UnnyNetBase.JSON_PUBLIC_KEY, string.Empty));
            m_PublicKey = EditorGUILayout.TextField("Public key", m_PublicKey);

            SetColor(m_GameLogin != GetJsonValue<bool>(UnnyNetBase.JSON_GAME_LOGIN, false));
            m_GameLogin = EditorGUILayout.Toggle("Login With Credentials", m_GameLogin);

            SetColor(m_DefaultChannel != GetJsonValue<string>(UnnyNetBase.JSON_DEFAULT_CHANNEL, "general"));
            m_DefaultChannel = EditorGUILayout.TextField("Default Channel", m_DefaultChannel);


            SetColor(m_OpenWithFade != GetJsonValue<bool>(UnnyNetBase.JSON_OPEN_WITH_FADE, false));
            m_OpenWithFade = EditorGUILayout.Toggle("Open with Fade", m_OpenWithFade);

            SetColor(m_OpenWithAnimation != System.Convert.ToInt32(GetJsonValue<object>(UnnyNetBase.JSON_OPEN_WITH_ANIMATION, (int)UnnyNetBase.DEFAULT_ANIMATION)));
            m_OpenWithAnimation = EditorGUILayout.Popup("Open with Animation", m_OpenWithAnimation, m_AnimationNames);

            SetColor(m_AnyChanges);
            GUI.enabled = m_AnyChanges;
            if (GUILayout.Button("Save"))
             {
                m_UnnyJson[UnnyNetBase.JSON_GAME_ID] = m_GameId;
                m_UnnyJson[UnnyNetBase.JSON_GAME_LOGIN] = m_GameLogin;
                m_UnnyJson[UnnyNetBase.JSON_DEFAULT_CHANNEL] = m_DefaultChannel;
                m_UnnyJson[UnnyNetBase.JSON_PUBLIC_KEY] = m_PublicKey;
                m_UnnyJson[UnnyNetBase.JSON_OPEN_WITH_FADE] = m_OpenWithFade;
                m_UnnyJson[UnnyNetBase.JSON_OPEN_WITH_ANIMATION] = m_OpenWithAnimation;
                SaveUnnyJson(m_UnnyJson);
            }

            GUILayout.EndVertical();

            SetColor(false);
            GUI.enabled = true;
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Documentation");
            if (GUILayout.Button("Basic Integration"))
                OpenHash("basic/integration_unity3d/");
            if (GUILayout.Button("Leaderboards"))
                OpenHash("advanced/leaderboards/");
            if (GUILayout.Button("Achievements"))
                OpenHash("advanced/achievements/");
            if (GUILayout.Button("Guilds"))
                OpenHash("advanced/guilds/guilds/");
            GUILayout.EndVertical();
        }
        #endregion
    }
}