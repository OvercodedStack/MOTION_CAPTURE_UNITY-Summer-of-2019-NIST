#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnnyNet.Android {
    public class WebViewResult {
        /// <summary>
        /// The identifier bound to this payload. It would be used internally to identify the callback.
        /// </summary>
        public string identifier;

        /// <summary>
        /// The result code contained in this payload. Generally, "0" means the operation finished without
        /// problem, while a non-zero value means somethings goes wrong.
        /// </summary>
        public string resultCode;

        /// <summary>
        /// Return value or data from native. You should look at 
        /// corresponding APIs to know what exactly contained in this.
        /// </summary>
        public string data;
    }
}

#endif