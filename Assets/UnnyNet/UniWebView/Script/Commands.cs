using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace UnnyNet {
    public class UnnynetCommand {
        private static readonly Dictionary<int, string> m_Commands = new Dictionary<int, string> {
            {(int) Command.OpenLeaderBoards, "window.globalReactFunctions.apiOpenLeaderboards()"},
            {(int) Command.OpenAchievements, "window.globalReactFunctions.apiOpenAchievements()"},
            {(int) Command.OpenFriends, "window.globalReactFunctions.apiOpenFriends()"},
            {(int) Command.OpenChannel, "window.globalReactFunctions.apiOpenChannel('{0}')"},
            {(int) Command.OpenGuilds, "window.globalReactFunctions.apiOpenGuilds()"},
            {(int) Command.OpenMyGuild, "window.globalReactFunctions.apiOpenMyGuild()"},
            {(int) Command.SendMessage, "window.globalReactFunctions.apiSendMessage('{0}', '{1}')"},
            {(int) Command.ReportLeaderboardScores, "window.globalReactFunctions.apiReportLeaderboardScores('{0}', '{1}')"},
            {(int) Command.ReportAchievementProgress, "window.globalReactFunctions.apiReportAchievementProgress({0}, {1})"},
            {(int) Command.AddGuildExperience, "window.globalReactFunctions.apiAddGuildExperience('{0}')"},
            {(int) Command.RequestFailed, "window.globalReactFunctions.requestFailed('{0}', \"{1}\")"},
            {(int) Command.RequestSucceeded, "window.globalReactFunctions.requestSucceeded('{0}')"},
            {(int) Command.SetKeyboardOffset, "window.globalReactFunctions.apiSetKeyboardOffset('{0}')"},
            {(int) Command.SetConfig, "window.globalReactFunctions.apiSetConfig('{0}')"},
            {(int) Command.SetDefaultChannel, "window.globalReactFunctions.apiSetDefaultChannel('{0}')"},         
            {(int) Command.AuthorizeWithCredentials, "window.globalReactFunctions.apiAuthWithCredentials('{0}', '{1}', '{2}')"}, 
            {(int) Command.AuthorizeAsGuest, "window.globalReactFunctions.apiAuthAsGuest('{0}')"}, 
            {(int) Command.AuthorizeWithCustomId, "window.globalReactFunctions.apiAuthWithCustomId('{0}', '{1}')"}, 
            {(int) Command.ForceLogout, "window.globalReactFunctions.apiForceLogout()"},
            {(int) Command.GetGuildInfo, "window.globalReactFunctions.apiGetGuildInfo(<*id*>, {0})"}
        };

        public enum Command {
            OpenLeaderBoards = 1,
            OpenAchievements = 2,
            OpenFriends = 3,
            OpenChannel = 4,
            OpenGuilds = 5,
            OpenMyGuild = 6,
            SendMessage = 20,
            ReportLeaderboardScores = 40,
            ReportAchievementProgress = 41,
            AddGuildExperience = 60,
            RequestFailed = 70,
            RequestSucceeded = 71,
            SetKeyboardOffset = 80,
            SetConfig = 81,
            SetDefaultChannel = 82,
            AuthorizeWithCredentials = 100,
            AuthorizeAsGuest = 101,
            AuthorizeWithCustomId = 102,
            ForceLogout = 110,
            GetGuildInfo = 120
        };

        public static string GetCommand(Command cmd) {
            string val;

            if (!m_Commands.TryGetValue((int)cmd, out val))
                throw new ArgumentOutOfRangeException("cmd", cmd, null);
            
            return m_Commands[(int)cmd];
        }
    }

    [Serializable]
    public class CommandInfo {
        [SerializeField]
        private bool m_OpenWindow;

        [SerializeField]
        private string m_Code;

        [SerializeField]
        private UnnynetCommand.Command m_Command;

        private UnityAction<ResponseData> m_Callback;
        private UnityAction<UniWebViewNativeResultPayload> m_CallbackNative;
        private int m_Retries = 0;

        public bool OpenWindow {
            get { return m_OpenWindow; }
            set { m_OpenWindow = value; }
        }

        public string Code {
            get { return m_Code; }
            set { m_Code = value; }
        }

        public UnnynetCommand.Command Command {
            get { return m_Command; }
            set { m_Command = value; }
        }

        public UnityAction<ResponseData> m_DelayedRequestCallback;

        public DateTime? StartedTime { get; set; }

        public int Retries {
            get { return m_Retries; }
            set { m_Retries = value; }
        }

        public virtual void EvaluateCallback(ResponseData response) {
            if (m_Callback != null)
                m_Callback(response);
        }

        public virtual void EvaluateCallbackNative(UniWebViewNativeResultPayload response)
        {
            if (m_CallbackNative != null)
                m_CallbackNative(response);
        }

        public virtual void EvaluateDelayedCallback(string response) {
            if (m_DelayedRequestCallback != null)
                m_DelayedRequestCallback(new ResponseData {Success = true, Error = null, Data = response});
        }

        public CommandInfo(UnnynetCommand.Command cmd, string code, bool openWindow, UnityAction<ResponseData> callback) : this(cmd, code, openWindow) {
            m_Callback = callback;
        }

        public CommandInfo(UnnynetCommand.Command cmd, string code, bool openWindow, UnityAction<UniWebViewNativeResultPayload> callback) : this(cmd, code, openWindow) {
            m_CallbackNative = callback;
        }

        public CommandInfo(UnnynetCommand.Command cmd, string code, bool openWindow) {
            Command = cmd;
            Code = code;
            m_Callback = null;
            OpenWindow = openWindow;
            StartedTime = null;
            Retries = 0;
        }

        //Delayed replies
        public CommandInfo(UnnynetCommand.Command cmd, string code, UnityAction<ResponseData> callback) : this(cmd, code, false) {
            m_DelayedRequestCallback = callback;

            RequestsManager.AddRequest(this);
        }

        public bool NeedToSave() {
            switch (Command) {
                case UnnynetCommand.Command.SendMessage:
                case UnnynetCommand.Command.ReportLeaderboardScores:
                case UnnynetCommand.Command.ReportAchievementProgress:
                case UnnynetCommand.Command.AddGuildExperience:
                    return true;
                default:
                    return false;
            }
        }

        public bool CouldBeReplaced(UnnynetCommand.Command cmd) {
            switch (cmd) {
                case UnnynetCommand.Command.OpenLeaderBoards:
                case UnnynetCommand.Command.OpenAchievements:
                case UnnynetCommand.Command.OpenFriends:
                case UnnynetCommand.Command.OpenChannel:
                case UnnynetCommand.Command.OpenGuilds:
                case UnnynetCommand.Command.OpenMyGuild:
                    return Command == UnnynetCommand.Command.OpenLeaderBoards || Command == UnnynetCommand.Command.OpenAchievements || Command == UnnynetCommand.Command.OpenFriends || Command == UnnynetCommand.Command.OpenChannel || Command == UnnynetCommand.Command.OpenGuilds || Command == UnnynetCommand.Command.OpenMyGuild;
                case UnnynetCommand.Command.AuthorizeWithCredentials:
                case UnnynetCommand.Command.AuthorizeAsGuest:
                case UnnynetCommand.Command.AuthorizeWithCustomId:
                case UnnynetCommand.Command.ForceLogout:
                case UnnynetCommand.Command.GetGuildInfo:
                    return Command == cmd;
                default:
                    return false;
            }
        }

        public bool SameType(UnnynetCommand.Command cmd) {
            switch (cmd) {
                case UnnynetCommand.Command.OpenLeaderBoards:
                case UnnynetCommand.Command.OpenAchievements:
                case UnnynetCommand.Command.OpenFriends:
                case UnnynetCommand.Command.OpenChannel:
                case UnnynetCommand.Command.OpenGuilds:
                case UnnynetCommand.Command.OpenMyGuild:
                    return Command == UnnynetCommand.Command.OpenLeaderBoards || Command == UnnynetCommand.Command.OpenAchievements || Command == UnnynetCommand.Command.OpenFriends || Command == UnnynetCommand.Command.OpenChannel || Command == UnnynetCommand.Command.OpenGuilds || Command == UnnynetCommand.Command.OpenMyGuild;
                default:
                    return Command == cmd;
            }
        }
    }

    [Serializable]
    public class CommandsInfo {
        [SerializeField]
        public CommandInfo[] m_Commands;

        public CommandsInfo(List<CommandInfo> data) {
            m_Commands = data.ToArray();
        }
    }
}