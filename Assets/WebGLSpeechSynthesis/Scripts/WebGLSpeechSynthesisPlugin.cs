using System;
using System.Collections;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using UnityEngine;

namespace UnityWebGLSpeechSynthesis
{
    public class WebGLSpeechSynthesisPlugin : BaseSpeechSynthesisPlugin, ISpeechSynthesisPlugin
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static WebGLSpeechSynthesisPlugin _sInstance = null;

        /// <summary>
        /// Get singleton instance
        /// </summary>
        public static WebGLSpeechSynthesisPlugin GetInstance()
        {
            return _sInstance;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private class OnlyWebGL
        {
            [DllImport("__Internal")]
            public static extern bool WebGLSpeechSynthesisPluginIsAvailable();

            [DllImport("__Internal")]
            public static extern void WebGLSpeechSynthesisPluginInit();

            [DllImport("__Internal")]
            public static extern string WebGLSpeechSynthesisPluginGetVoices();

            [DllImport("__Internal")]
            public static extern int WebGLSpeechSynthesisPluginCreateSpeechSynthesisUtterance();

            [DllImport("__Internal")]
            public static extern void WebGLSpeechSynthesisPluginSetUtterancePitch(int reference, string pitch);

            [DllImport("__Internal")]
            public static extern void WebGLSpeechSynthesisPluginSetUtteranceRate(int reference, string rate);

            [DllImport("__Internal")]
            public static extern void WebGLSpeechSynthesisPluginSetUtteranceText(int reference, string text);

            [DllImport("__Internal")]
            public static extern void WebGLSpeechSynthesisPluginSetUtteranceVoice(int reference, string voiceURI);

            [DllImport("__Internal")]
            public static extern void WebGLSpeechSynthesisPluginSetUtteranceVolume(int reference, float volume);

            [DllImport("__Internal")]
            public static extern void WebGLSpeechSynthesisPluginSpeak(int reference);

            [DllImport("__Internal")]
            public static extern bool WebGLSpeechSynthesisPluginHasOnEnd();

            [DllImport("__Internal")]
            public static extern string WebGLSpeechSynthesisPluginGetOnEnd();

            [DllImport("__Internal")]
            public static extern void WebGLSpeechSynthesisPluginCancel();
        }
#endif

        /// <summary>
        /// Set the singleton instance
        /// </summary>
        private void Awake()
        {
            _sInstance = this;
        }

        /// <summary>
        /// Get the proxy on end events and then fire callback
        /// </summary>
        /// <returns></returns>
        IEnumerator ProxyOnEnd()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            //Debug.Log("ProxyOnEnd:");
            while (true)
            {
                if (OnlyWebGL.WebGLSpeechSynthesisPluginHasOnEnd())
                {
                    string jsonData = OnlyWebGL.WebGLSpeechSynthesisPluginGetOnEnd();
                    if (string.IsNullOrEmpty(jsonData))
                    {
                        yield return new WaitForSeconds(1f);
                        continue;
                    }

                    //Debug.Log("ProxyOnEnd: "+jsonData);
                    SpeechSynthesisEvent speechSynthesisEvent =
                        JsonUtility.FromJson<SpeechSynthesisEvent>(jsonData);
                    if (null != speechSynthesisEvent)
                    {
                        //Debug.LogFormat("ProxyOnEnd: listener count={0}", _sOnSynthesisOnEnd.Count);
                        for (int i = 0; i < _sOnSynthesisOnEnd.Count; ++i)
                        {
                            DelegateHandleSynthesisOnEnd listener = _sOnSynthesisOnEnd[i];
                            if (null != listener)
                            {
                                listener.Invoke(speechSynthesisEvent);
                            }
                        }
                    }
                }
                yield return new WaitForFixedUpdate();
            }
#else
            yield break;
#endif
        }

        public bool IsAvailable()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return OnlyWebGL.WebGLSpeechSynthesisPluginIsAvailable();
#else
            return false;
#endif
        }

        public void CreateSpeechSynthesisUtterance(Action<SpeechSynthesisUtterance> callback)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            int index = OnlyWebGL.WebGLSpeechSynthesisPluginCreateSpeechSynthesisUtterance();
            if (index < 0)
            {
                callback.Invoke(null);
                return;
            }
            SpeechSynthesisUtterance instance = new SpeechSynthesisUtterance();
            instance._mReference = index;
            callback.Invoke(instance);
            return;
#else
            callback.Invoke(null);
            return;
#endif
        }

        public void Speak(SpeechSynthesisUtterance speechSynthesisUtterance)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            OnlyWebGL.WebGLSpeechSynthesisPluginSpeak(speechSynthesisUtterance._mReference);
#endif
        }

        public void Cancel()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            OnlyWebGL.WebGLSpeechSynthesisPluginCancel();
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        void Start()
        {
            if (OnlyWebGL.WebGLSpeechSynthesisPluginIsAvailable())
            {
                OnlyWebGL.WebGLSpeechSynthesisPluginInit();
                StartCoroutine(ProxyOnEnd());
            }
        }
#endif

        public void GetVoices(Action<VoiceResult> callback)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            string jsonData = OnlyWebGL.WebGLSpeechSynthesisPluginGetVoices();
            if (jsonData == "")
            {
                callback.Invoke(null);
                return;
            }
            //Debug.Log("GetVoices: "+jsonData);
            VoiceResult result = JsonUtility.FromJson<VoiceResult>(jsonData);
            if (null != result &&
                null != result.voices)
            {
                for (int i = 0; i < result.voices.Length; ++i)
                {
                    Voice voice = result.voices[i];
                    if (null == voice)
                    {
                        continue;
                    }
                    if (null == voice.lang)
                    {
                        continue;
                    }
                    string display = null;
                    switch (voice.lang)
                    {
                        case "de-DE":
                            display = "German (Germany)";
                            break;
                        case "es-ES":
                            display = "Spanish (Spain)";
                            break;
                        case "es-US":
                            display = "Spanish (United States)";
                            break;
                        case "fr-FR":
                            display = "French (France)";
                            break;
                        case "hi-IN":
                            display = "Hindi (India)";
                            break;
                        case "id-ID":
                            display = "Indonesian (Indonesia)";
                            break;
                        case "it-IT":
                            display = "Italian (Italy)";
                            break;
                        case "ja-JP":
                            display = "Japanese (Japan)";
                            break;
                        case "ko-KR":
                            display = "Korean (South Korea)";
                            break;
                        case "nl-NL":
                            display = "Dutch (Netherlands)";
                            break;
                        case "pl-PL":
                            display = "Polish (Poland)";
                            break;
                        case "pt-BR":
                            display = "Portuguese (Brazil)";
                            break;
                        case "ru-RU":
                            display = "Russian (Russia)";
                            break;
                        case "zh-CN":
                            display = "Mandarin (China)";
                            break;
                        case "zh-HK":
                            display = "Cantonese (China)";
                            break;
                        case "zh-TW":
                            display = "Mandarin (Taiwan)";
                            break;
                    }
                    if (null != display)
                    {
                        voice.display = display;
                    }
                }
            }
            callback.Invoke(result);
            return;
#else
            callback.Invoke(null);
#endif
        }

        public void SetPitch(SpeechSynthesisUtterance utterance, float pitch)
        {
            if (null == utterance)
            {
                Debug.LogError("Utterance not set!");
                return;
            }
#if UNITY_WEBGL && !UNITY_EDITOR
            OnlyWebGL.WebGLSpeechSynthesisPluginSetUtterancePitch(utterance._mReference, pitch.ToString());
#endif
        }
        public void SetRate(SpeechSynthesisUtterance utterance, float rate)
        {
            if (null == utterance)
            {
                Debug.LogError("Utterance not set!");
                return;
            }
#if UNITY_WEBGL && !UNITY_EDITOR
            OnlyWebGL.WebGLSpeechSynthesisPluginSetUtteranceRate(utterance._mReference, rate.ToString());
#endif
        }
        public void SetText(SpeechSynthesisUtterance utterance, string text)
        {
            if (null == utterance)
            {
                Debug.LogError("Utterance not set!");
                return;
            }
#if UNITY_WEBGL && !UNITY_EDITOR
            OnlyWebGL.WebGLSpeechSynthesisPluginSetUtteranceText(utterance._mReference, text);
#endif
        }
        public void SetVoice(SpeechSynthesisUtterance utterance, Voice voice)
        {
            if (null == utterance)
            {
                Debug.LogError("Utterance not set!");
                return;
            }
            if (null == voice)
            {
                Debug.LogError("Voice not set!");
                return;
            }
#if UNITY_WEBGL && !UNITY_EDITOR
            OnlyWebGL.WebGLSpeechSynthesisPluginSetUtteranceVoice(utterance._mReference, voice.voiceURI);
#endif
        }

        public void SetVolume(SpeechSynthesisUtterance utterance, float volume)
        {
            if (null == utterance)
            {
                Debug.LogError("Utterance not set!");
                return;
            }
#if UNITY_WEBGL && !UNITY_EDITOR
            OnlyWebGL.WebGLSpeechSynthesisPluginSetUtteranceVolume(utterance._mReference, volume);
#endif
        }

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
