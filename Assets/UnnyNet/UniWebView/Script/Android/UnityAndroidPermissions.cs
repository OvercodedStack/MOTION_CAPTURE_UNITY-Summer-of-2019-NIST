#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UnnyNet.Android {

    public class AndroidPermissionCallback : AndroidJavaProxy {
        private event Action<string, int> OnPermissionResultAction;

        public AndroidPermissionCallback(Action<string, int> onPermissionCallback) : base("com.unnynet.android.helper.UnityAndroidPermissions$IPermissionRequestResult") {
            if (onPermissionCallback != null) {
                OnPermissionResultAction += onPermissionCallback;
            }
        }

        // Handle permission granted
        public virtual void OnPermissionResult(string permissionName, int res) {
            if (OnPermissionResultAction != null) {
                OnPermissionResultAction(permissionName, res);
            }
        }
    }

    public class UnityAndroidPermissions {
        public const string WRITE_EXTERNAL_STORAGE = "android.permission.WRITE_EXTERNAL_STORAGE";

        private static AndroidJavaObject m_Activity;
        private static AndroidJavaObject m_PermissionService;

        private static AndroidJavaObject GetActivity() {
            if (m_Activity == null) {
                var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                m_Activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }
            return m_Activity;
        }

        private static AndroidJavaObject GetPermissionsService() {
            return m_PermissionService ??
                (m_PermissionService = new AndroidJavaObject("com.unnynet.android.helper.UnityAndroidPermissions"));
        }

        public static bool IsPermissionGranted(string permissionName) {
            return GetPermissionsService().Call<bool>("isPermissionGranted", GetActivity(), permissionName);
        }

        public static void RequestPermissions(string[] permissionNames, AndroidPermissionCallback callback) {
            GetPermissionsService().Call("requestPermissionsAsync", GetActivity(), permissionNames, callback);
        }

        public static void RequestPermission(string permissionName, AndroidPermissionCallback callback) {
            GetPermissionsService().Call("requestPermissionAsync", GetActivity(), permissionName, callback);
        }
    }
}

#endif