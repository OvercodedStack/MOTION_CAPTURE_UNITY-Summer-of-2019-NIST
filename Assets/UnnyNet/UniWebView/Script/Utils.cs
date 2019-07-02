using UnityEngine;

namespace UnnyNet {
    public class Utils {
        public static string GetUniqId() {
            var id = SystemInfo.deviceUniqueIdentifier;

            return SystemInfo.unsupportedIdentifier != SystemInfo.deviceUniqueIdentifier ? id : null;
        }
    }
}
