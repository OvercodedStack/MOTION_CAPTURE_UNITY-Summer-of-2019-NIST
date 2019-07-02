using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnnyNet;
using Error = UnnyNet.Error;

public class UnnyNetExample : MonoBehaviour
{
    public Button m_OpenUnnyNetBtn;

    public InputField m_ChannelName;
    public InputField m_ChannelMessage;
    public Button m_ChannelOpen;
    public Button m_ChannelSend;
    public Text m_ChannelStatus;

    public InputField m_LeadersName;
    public InputField m_LeadersScores;
    public Button m_LeadersOpen;
    public Button m_LeadersReport;
    public Text m_LeadersStatus;

    public InputField m_AchID;
    public InputField m_AchProgress;
    public Button m_AchOpen;
    public Button m_AchReport;
    public Text m_AchStatus;

    public InputField m_GuildExperience;
    public Button m_ExpDonate;
    public Text m_GuildStatus;
    public Button m_GuildsWindow;
    public Button m_MyGuildWindow;

    public Button m_FriendsOpen;
    public Text m_FriendsStatus;

    public Button m_GameLogin;
    public Button m_GuestLogin;
    public Button m_CustomLogin;
    public Button m_LogOut;

    public Button m_GetGuildInfo;

    private void Start()
    {
        m_OpenUnnyNetBtn.onClick.AddListener(OpenUnnyNetClicked);
        m_ChannelOpen.onClick.AddListener(OpenChannel);
        m_ChannelSend.onClick.AddListener(SendMessage);

        m_LeadersOpen.onClick.AddListener(OpenLeaders);
        m_LeadersReport.onClick.AddListener(ReportLeaders);

        m_AchOpen.onClick.AddListener(OpenAchievements);
        m_AchReport.onClick.AddListener(ReportAchievement);

        m_ExpDonate.onClick.AddListener(DonateGuildExperience);
        m_GuildsWindow.onClick.AddListener(OpenGuilds);
        m_MyGuildWindow.onClick.AddListener(OpenMyGuild);

        m_FriendsOpen.onClick.AddListener(OpenFriends);

        UnnyNet.UnnyNetBase.m_OnPlayerAuthorized = (prms) =>
        {
            string unnyId;
            prms.TryGetValue("unny_id", out unnyId);
            string playerName;
            prms.TryGetValue("name", out playerName);
            Debug.LogFormat("Player autorized: id = {0}; name = {1};", unnyId, playerName);
        };

        UnnyNet.UnnyNetBase.m_OnPlayerNameChanged = (newName) => {
            Debug.Log("Player changed name to " + newName);
        };

        UnnyNet.UnnyNetBase.m_OnNewGuildRequest = (prms) => {
            string guildName;
            prms.TryGetValue("name", out guildName);
            string description;
            prms.TryGetValue("description", out description);
            string guildType;
            prms.TryGetValue("type", out guildType);
            Debug.Log("Guilds are allowed, so we return null. If you want to prevent guild from the creation - just return any string error");
            return null;
        };

        UnnyNet.UnnyNetBase.m_OnNewGuild = (prms) => {
            string guildName;
            prms.TryGetValue("name", out guildName);
            string description;
            prms.TryGetValue("description", out description);
            string guildId;
            prms.TryGetValue("guild_id", out guildId);
            Debug.LogFormat("New Guild was created: id = {0}; name = {1}; description= {2}", guildId, guildName, description);
        };

        UnnyNet.UnnyNetBase.m_OnRankChanged = (prms) => {
            string previousRankIndex;
            prms.TryGetValue("prev_index", out previousRankIndex);
            string previousRankName;
            prms.TryGetValue("prev_rank", out previousRankName);
            string currentRankIndex;
            prms.TryGetValue("curr_index", out currentRankIndex);
            string currentRankName;
            prms.TryGetValue("curr_rank", out currentRankName);
            Debug.LogFormat("Player's rank changed from {0} ({1}) to {2} ({3})", previousRankName, previousRankIndex, currentRankName, currentRankIndex);
        };

        UnnyNet.UnnyNetBase.m_OnAchievementCompleted = (Dictionary<string, string> prms) => {
            Debug.Log("On Achievement Completed by id " + prms["ach_id"]);
        };

        UnnyNet.UnnyNetBase.m_OnNewMessageReceived = (Dictionary<string, string> prms) => {
            string sender_id;
            prms.TryGetValue("sender_id", out sender_id);
            string sender_name;
            prms.TryGetValue("sender_name", out sender_name);
            string channel_type;
            prms.TryGetValue("type", out channel_type);
            string channel_name;
            prms.TryGetValue("channel_name", out channel_name);

            UnnyNet.ChannelType type = (UnnyNet.ChannelType)int.Parse(channel_type);

            Debug.LogFormat("New Message received from user {0} ({1}); ChannelType = {2}", sender_name, sender_id, type);
        };

        UnnyNet.UnnyNetBase.m_OnGameLoginRequest = AuthWithGameCredentials;
        UnnyNet.UnnyNetBase.InitializeUnnyNet();

        m_GameLogin.onClick.AddListener(AuthWithGameCredentials);
        m_GuestLogin.onClick.AddListener(AuthAsGuest);
        m_CustomLogin.onClick.AddListener(AuthWithCustomId);
        m_LogOut.onClick.AddListener(ForceLogOut);

        m_GetGuildInfo.onClick.AddListener(GetGuildInfo);
    }

    void OnGuildInfo(ResponseData data) {
        if (data.Success) {
            Dictionary<string, object> json = UnnyNetMiniJSON.Json.Deserialize(data.Data) as Dictionary<string, object>;

            if (json.ContainsKey("error"))
            {
                Debug.LogError("Couldn't get Guild Info: " + json["error"]);
            }
            else
            {
                Debug.Log("Guild Info Loaded: " + data.Data);
            }
        }
    }

    void GetGuildInfo() {
        UnnyNet.UnnyNet.GetGuildInfo(true, OnGuildInfo);
    }

    void AuthWithGameCredentials(){
        UnnyNet.UnnyNet.AuthorizeWithCredentials("username", "password", "display_name");
    }

    void AuthAsGuest()
    {
        UnnyNet.UnnyNet.AuthorizeAsGuest("display_name");
    }

    void AuthWithCustomId()
    {
        UnnyNet.UnnyNet.AuthorizeWithCustomId("custom_id", "display_custom");
    }

    void ForceLogOut() {
        UnnyNet.UnnyNet.ForceLogout();
    }

    void OpenUnnyNetClicked()
    {
        UnnyNet.UnnyNetBase.OpenUnnyNet();
    }

    private void DisplayMessage(string error, Text text, string successText)
    {
        if (error != null)
        {
            text.text = error;
            text.color = Color.red;
        }
        else
        {
            text.text = successText;
            text.color = Color.green;
        }
    }


    private void DisplayMessage(ResponseData response, Text text, string successText) {
        DisplayMessage(response != null && !response.Success ? response.Error.Message : null, text, successText);
    }

    #region Game Messages

    private void MessageWasSent(ResponseData response)
    {
        DisplayMessage(response, m_ChannelStatus, "Message Was Sent");
    }

    private void ChannelWasOpened(ResponseData response)
    {
        DisplayMessage(response, m_ChannelStatus, "Channel Was Opened");
    }

    void SendMessage()
    {
        m_ChannelStatus.text = null;
        UnnyNet.UnnyNet.SendMessageToChannel(m_ChannelName.text, m_ChannelMessage.text, MessageWasSent);
        m_ChannelMessage.text = string.Empty;
    }

    void OpenChannel()
    {
        m_ChannelStatus.text = null;
        UnnyNet.UnnyNet.OpenChannel(m_ChannelName.text, ChannelWasOpened);
    }
    #endregion

    #region Leaderboards
    private void LeadersWereReported(ResponseData response)
    {
        DisplayMessage(response, m_LeadersStatus, "Leaders Were Reported");
    }

    private void LeadersWereReportedStr(string error)
    {
        DisplayMessage(error, m_LeadersStatus, "Leaders Were Reported");
    }

    private void LeadersWereOpened(ResponseData response)
    {
        DisplayMessage(response, m_LeadersStatus, "Leaders Were Opened");
    }

    void ReportLeaders()
    {
        m_LeadersStatus.text = null;
        float scores;
        if (float.TryParse(m_LeadersScores.text, out scores))
            UnnyNet.UnnyNet.ReportLeaderboards(m_LeadersName.text, scores, LeadersWereReported);
        else
            LeadersWereReportedStr("Scores must be float");
    }

    void OpenLeaders()
    {
        m_LeadersStatus.text = null;
        UnnyNet.UnnyNet.OpenLeaderboards(LeadersWereOpened);
    }
    #endregion

    #region Achievements
    private void AchWereReported(ResponseData response)
    {
        DisplayMessage(response, m_AchStatus, "Achievement Was Reported");
    }

    private void AchWereReportedStr(string error)
    {
        DisplayMessage(error, m_AchStatus, "Achievement Was Reported");
    }

    private void AchWereOpened(ResponseData response)
    {
        DisplayMessage(response, m_AchStatus, "Achievements Were Opened");
    }

    void ReportAchievement()
    {
        m_AchStatus.text = null;
        int progress;
        if (int.TryParse(m_AchProgress.text, out progress))
        {
            int id;
            if (int.TryParse(m_AchID.text, out id))
                UnnyNet.UnnyNet.ReportAchievements(id, progress, AchWereReported);
            else
                AchWereReportedStr("ID must be integer");
        }
        else
            AchWereReportedStr("Progress must be integer");
    }

    void OpenAchievements()
    {
        m_AchStatus.text = null;
        UnnyNet.UnnyNet.OpenAchievements(AchWereOpened);
    }
    #endregion

    #region Guilds
    private void GuildExperienceReported(ResponseData response)
    {
        DisplayMessage(response, m_GuildStatus, "Experience Was Added");
    }

    private void GuildExperienceReportedStr(string error)
    {
        DisplayMessage(error, m_GuildStatus, "Experience Was Added");
    }

    private void GuildsWereOpened(ResponseData response)
    {
        DisplayMessage(response, m_GuildStatus, "Guilds Were Opened");
    }

    private void MyGuildWasOpened(ResponseData response)
    {
        DisplayMessage(response, m_GuildStatus, "My Guild Was Opened");
    }

    void DonateGuildExperience()
    {
        m_GuildStatus.text = null;
        int experience;
        if (int.TryParse(m_GuildExperience.text, out experience))
        {
            UnnyNet.UnnyNet.AddGuildExperience(experience, GuildExperienceReported);
        }
        else
            GuildExperienceReportedStr("Experience must be integer");
    }

    void OpenGuilds()
    {
        m_GuildStatus.text = null;
        UnnyNet.UnnyNet.OpenGuilds(GuildsWereOpened);
    }

    void OpenMyGuild()
    {
        m_GuildStatus.text = null;
        UnnyNet.UnnyNet.OpenMyGuild(MyGuildWasOpened);
    }
    #endregion

    #region Friends
    private void FriendsWereOpened(ResponseData response)
    {
        DisplayMessage(response, m_FriendsStatus, "Friends Were Opened");
    }

    void OpenFriends()
    {
        m_FriendsStatus.text = null;
        UnnyNet.UnnyNet.OpenFriends(FriendsWereOpened);
    }
    #endregion
}
