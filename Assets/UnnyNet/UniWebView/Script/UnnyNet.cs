using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_ANDROID
using UnnyNet.Android;
#endif

namespace UnnyNet
{
    public class UnnyNet : UnnyNetBase
    {
#region OpenPage
        public static void OpenLeaderboards(UnityAction<ResponseData> doneCallback = null)
        {
            EvaluateCommand(UnnynetCommand.Command.OpenLeaderBoards, true, doneCallback);
        }

        public static void OpenAchievements(UnityAction<ResponseData> doneCallback = null)
        {
            EvaluateCommand(UnnynetCommand.Command.OpenAchievements, true, doneCallback);
        }

        public static void OpenFriends(UnityAction<ResponseData> doneCallback = null)
        {
            EvaluateCommand(UnnynetCommand.Command.OpenFriends, true, doneCallback);
        }

        public static void OpenChannel(string channelName, UnityAction<ResponseData> doneCallback = null)
        {
            EvaluateCodeInJavaScript(new CommandInfo(UnnynetCommand.Command.OpenChannel, string.Format(UnnynetCommand.GetCommand(UnnynetCommand.Command.OpenChannel), channelName), true, doneCallback));
        }

        public static void OpenGuilds(UnityAction<ResponseData> doneCallback = null)
        {
            EvaluateCommand(UnnynetCommand.Command.OpenGuilds, true, doneCallback);
        }

        public static void OpenMyGuild(UnityAction<ResponseData> doneCallback = null)
        {
            EvaluateCommand(UnnynetCommand.Command.OpenMyGuild, true, doneCallback);
        }

#endregion

#region Auth
        public static void AuthorizeWithCredentials(string login, string password, string displayName, UnityAction<ResponseData> doneCallback = null)
        {
            string code = string.Format(UnnynetCommand.GetCommand(UnnynetCommand.Command.AuthorizeWithCredentials), login, password, displayName);
            EvaluateCodeInJavaScript(new CommandInfo(UnnynetCommand.Command.AuthorizeWithCredentials, code, false, doneCallback), true);
        }

        public static void AuthorizeAsGuest(string displayName, UnityAction<ResponseData> doneCallback = null)
        {
            string code = string.Format(UnnynetCommand.GetCommand(UnnynetCommand.Command.AuthorizeAsGuest), displayName);
            EvaluateCodeInJavaScript(new CommandInfo(UnnynetCommand.Command.AuthorizeAsGuest, code, false, doneCallback), true);
        }

        public static void AuthorizeWithCustomId(string userName, string displayName, UnityAction<ResponseData> doneCallback = null)
        {
            string code = string.Format(UnnynetCommand.GetCommand(UnnynetCommand.Command.AuthorizeWithCustomId), userName, displayName);
            EvaluateCodeInJavaScript(new CommandInfo(UnnynetCommand.Command.AuthorizeWithCustomId, code, false, doneCallback), true);
        }

        public static void ForceLogout(UnityAction<ResponseData> doneCallback = null)
        {
            EvaluateCommand(UnnynetCommand.Command.ForceLogout, false, doneCallback);
        }

        public static void GetGuildInfo(bool fullInfo, UnityAction<ResponseData> doneCallback)
        {
            string code = string.Format(UnnynetCommand.GetCommand(UnnynetCommand.Command.GetGuildInfo), fullInfo ? 1 : 0);
            EvaluateCodeInJavaScript(new CommandInfo(UnnynetCommand.Command.GetGuildInfo, code, doneCallback));
        }
#endregion

#region API
        public static void SendMessageToChannel(string channelName, string message, UnityAction<ResponseData> doneCallback = null) {
            if (!string.IsNullOrEmpty(message)) {
                string code = string.Format(UnnynetCommand.GetCommand(UnnynetCommand.Command.SendMessage), channelName, message);
                EvaluateCodeInJavaScript(new CommandInfo(UnnynetCommand.Command.SendMessage, code, false, doneCallback));
            }
        }

        public static void ReportLeaderboards(string leaderboardsName, float newScore, UnityAction<ResponseData> doneCallback = null) {
            string code = string.Format(UnnynetCommand.GetCommand(UnnynetCommand.Command.ReportLeaderboardScores), leaderboardsName, newScore);
            EvaluateCodeInJavaScript(new CommandInfo(UnnynetCommand.Command.ReportLeaderboardScores, code, false, doneCallback));
        }

        public static void ReportAchievements(int achId, int progress, UnityAction<ResponseData> doneCallback = null) {
            string code = string.Format(UnnynetCommand.GetCommand(UnnynetCommand.Command.ReportAchievementProgress), achId, progress);
            EvaluateCodeInJavaScript(new CommandInfo(UnnynetCommand.Command.ReportAchievementProgress, code, false, doneCallback));
        }

        public static void AddGuildExperience(int experience, UnityAction<ResponseData> doneCallback = null) {
            string code = string.Format(UnnynetCommand.GetCommand(UnnynetCommand.Command.AddGuildExperience), experience);
            EvaluateCodeInJavaScript(new CommandInfo(UnnynetCommand.Command.AddGuildExperience, code, false, doneCallback));
        }
#endregion
    }
}