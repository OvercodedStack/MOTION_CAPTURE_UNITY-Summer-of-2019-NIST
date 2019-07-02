using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Internal;
using UnityEngine.Serialization;

namespace UnnyNet {
    public enum Errors {
        NotInitialized = -1,
        Unknown = 1,
        NotAuthorized = 2,
        NoMessage = 3,
        NoChannel = 4,
        UnnynetNotReady = 5,
        NoGameId = 6,
        NoSuchLeaderboard = 7,
        NoLeaderboardsForTheGame = 8,
        NoAchievementsForTheGame = 9,
        NoGuildsForTheGame = 10,
        NotInGuild = 11,
        NoSuchAchievement = 12,
        WrongAchievementType = 13
    };

    [Serializable]
    public class ResponseData {
        [SerializeField, FormerlySerializedAs("success")]
        private bool success;

        [SerializeField, FormerlySerializedAs("error")]
        private Error error;

        [SerializeField, FormerlySerializedAs("data")]
        private string data;

        public bool Success {
            get { return success; }
            set { success = value; }
        }

        public string Data {
            get { return data; }
            set { data = value; }
        }

        public Error Error {
            get { return error; }
            set { error = value; }
        }
    }

    [Serializable]
    public class Error {
        [SerializeField, FormerlySerializedAs("code")]
        private int code;

        [SerializeField, FormerlySerializedAs("message")]
        private string message;

        public int Code {
            get { return code; }
            set { code = value; }
        }

        public string Message {
            get { return message; }
            set { message = value; }
        }

        public static Error GetUnnyNetNotReadyError(){
            Error error = new Error();
            error.Code = (int)Errors.UnnynetNotReady;
            return error;
        }
    }
}