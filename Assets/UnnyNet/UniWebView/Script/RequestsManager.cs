using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnyNet
{
    public class RequestsManager
    {
        static int m_UniqueIds;
        static Dictionary<int, CommandInfo> m_AllRequests = new Dictionary<int, CommandInfo>();

        static int RequestId(){
            return m_UniqueIds++;
        }

        public static void AddRequest(CommandInfo cmd) {
            int id = RequestId();
            if (m_AllRequests.ContainsKey(id))
                m_AllRequests[id] = cmd;
            else
                m_AllRequests.Add(id, cmd);

            string code = cmd.Code.Replace("<*id*>", "{0}");
            cmd.Code = string.Format(code, id);
        }

        public static void ReplyReceived(Dictionary<string, string> reply) {
            int id = int.Parse(reply["id"]);
            CommandInfo info;
            if (m_AllRequests.TryGetValue(id, out info)) {
                m_AllRequests.Remove(id);
                info.EvaluateDelayedCallback(reply["data"]);
            }
        }
    }
}