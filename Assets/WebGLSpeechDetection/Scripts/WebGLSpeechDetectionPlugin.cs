using System;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using UnityEngine;

namespace UnityWebGLSpeechDetection
{
    public class WebGLSpeechDetectionPlugin : BaseSpeechDetectionPlugin, ISpeechDetectionPlugin
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static WebGLSpeechDetectionPlugin _sInstance = null;

        /// <summary>
        /// Get singleton instance
        /// </summary>
        public static WebGLSpeechDetectionPlugin GetInstance()
        {
            return _sInstance;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private class OnlyWebGL
        {
            [DllImport("__Internal")]
            public static extern bool WebGLSpeechDetectionPluginIsAvailable();

            [DllImport("__Internal")]
            public static extern void WebGLSpeechDetectionPluginInit();

            [DllImport("__Internal")]
            public static extern void WebGLSpeechDetectionPluginAbort();

            [DllImport("__Internal")]
            public static extern string WebGLSpeechDetectionPluginGetLanguages();

            [DllImport("__Internal")]
            public static extern int WebGLSpeechDetectionPluginGetNumberOfResults();

            [DllImport("__Internal")]
            public static extern string WebGLSpeechDetectionPluginGetResult();

            [DllImport("__Internal")]
            public static extern void WebGLSpeechDetectionPluginSetLanguage(string dialect);

            [DllImport("__Internal")]
            public static extern void WebGLSpeechDetectionPluginStart();

            [DllImport("__Internal")]
            public static extern void WebGLSpeechDetectionPluginStop();
        }
#endif

        public bool IsAvailable()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return OnlyWebGL.WebGLSpeechDetectionPluginIsAvailable();
#else
            return false;
#endif
        }

        public void Abort()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            OnlyWebGL.WebGLSpeechDetectionPluginAbort();
#endif
        }

        public void StartRecognition()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            OnlyWebGL.WebGLSpeechDetectionPluginStart();
#endif
        }

        public void StopRecognition()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            OnlyWebGL.WebGLSpeechDetectionPluginStop();
#endif
        }

        public void GetLanguages(Action<LanguageResult> callback)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            string jsonData = OnlyWebGL.WebGLSpeechDetectionPluginGetLanguages();
            if (string.IsNullOrEmpty(jsonData))
            {
                Debug.LogError("GetLanguages: Languages are empty!");
                callback.Invoke(null);
                return;
            }
            //Debug.Log("GetLanguages=" + jsonData);
            LanguageResult languageResult = JsonUtility.FromJson<LanguageResult>(jsonData);
            callback.Invoke(languageResult);
            return;
#else
            callback.Invoke(null);
#endif
        }

        public void SetLanguage(string dialect)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            OnlyWebGL.WebGLSpeechDetectionPluginSetLanguage(dialect);
#endif
        }

        /// <summary>
        /// Set the singleton instance
        /// </summary>
        private void Awake()
        {
            _sInstance = this;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        void Start()
        {

            if (OnlyWebGL.WebGLSpeechDetectionPluginIsAvailable())
            {
                OnlyWebGL.WebGLSpeechDetectionPluginInit();
            }
        }

        void FixedUpdate()
        {
            if (OnlyWebGL.WebGLSpeechDetectionPluginGetNumberOfResults() > 0)
            {
                string jsonData = OnlyWebGL.WebGLSpeechDetectionPluginGetResult();
                if (string.IsNullOrEmpty(jsonData))
                {
                    return;
                }
                DetectionResult detectionResult = JsonUtility.FromJson<DetectionResult>(jsonData);
                if (null != detectionResult)
                {
                    Invoke(detectionResult);
                }
            }
        }
#endif

        public void ManagementCloseBrowserTab()
        {
            //not used
        }

        public void ManagementCloseProxy()
        {
            //not used
        }

        public void ManagementLaunchProxy()
        {
            //not used
        }

        public void ManagementOpenBrowserTab()
        {
            //not used
        }

        public void ManagementSetProxyPort(int port)
        {
            //not used
        }
    }
}
