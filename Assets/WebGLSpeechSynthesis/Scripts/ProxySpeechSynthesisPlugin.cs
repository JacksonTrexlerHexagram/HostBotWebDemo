using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
#if UNITY_EDITOR || UNITY_STANDALONE
using System.Diagnostics;
using System.IO;
#endif
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UnityWebGLSpeechSynthesis
{
    public class ProxySpeechSynthesisPlugin : BaseSpeechSynthesisPlugin, ISpeechSynthesisPlugin
    {
        const string KEY_CHROME_SPEECH_PROXY = "CHROME_SPEECH_PROXY";

        /// <summary>
        /// Default proxy port
        /// </summary>
        public int _mPort = 5000;

        /// <summary>
        /// Singleton reference
        /// </summary>
        private static ProxySpeechSynthesisPlugin _sInstance = null;

        /// <summary>
        /// Available after proxy responds
        /// </summary>
        private bool _mIsAvailable = false;

        /// <summary>
        /// Change to the next port after proxy is set
        /// </summary>
        private int _mNextPort = 0;

        /// <summary>
        /// Invoke Utterance callbacks when results are ready
        /// </summary>
        private List<Action<SpeechSynthesisUtterance>> _mUtteranceCallbacks =
            new List<Action<SpeechSynthesisUtterance>>();

        /// <summary>
        /// Invoke GetVoices callbacks when results are ready
        /// </summary>
        private List<Action<VoiceResult>> _mGetVoicesCallbacks =
            new List<Action<VoiceResult>>();

        /// <summary>
        /// Pending commands
        /// </summary>
        private List<string> _mPendingCommands = new List<string>();

        /// <summary>
        /// Get singleton instance
        /// </summary>
        public static ProxySpeechSynthesisPlugin GetInstance()
        {
            return _sInstance;
        }

        protected virtual void SafeStartCoroutine(string routineName, IEnumerator routine)
        {
            //Debug.LogFormat("ProxySpeechDetectionPlugin: SafeStartCoroutine: {0}", routineName);
            StartCoroutine(routine);
        }

        /// <summary>
        /// Set the singleton instance
        /// </summary>
        private void Awake()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            _sInstance = this;
#else
            gameObject.SetActive(false);
#endif
        }

        // Use this for initialization
        protected virtual void Start()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            SafeStartCoroutine("Init", Init());
#else
            gameObject.SetActive(false);
#endif
        }

        // Use this for initialization
        protected IEnumerator Init()
        {
			DateTime wait;

            //Debug.Log("Init:");
            while (true)
            {
                string url = string.Format("http://localhost:{0}/SpeechSynthesisConnect", _mPort);
                IWWW www = CreateWWW(url);
                while (null == www.GetError() &&
                    !www.IsDone())
                {
#if UNITY_2018_1_OR_NEWER
                    yield return www.SendWebRequest();
#else
                    yield return null;
#endif
                }

                string error = www.GetError();
                bool hasError = !string.IsNullOrEmpty(error);
                www.Dispose();
                if (hasError)
                {
                    //Debug.LogError(error); //update status label instead
					wait = DateTime.Now + TimeSpan.FromSeconds(1);
					while (DateTime.Now < wait)
					{
						yield return null;
					}
                    continue;
                }
                _mIsAvailable = true;
                SafeStartCoroutine("RunPendingCommands", RunPendingCommands());
                SafeStartCoroutine("ProxyOnEnd", ProxyOnEnd());
                yield break;
            }
        }

        /// <summary>
        /// Run commands in order
        /// </summary>
        /// <returns></returns>
        private IEnumerator RunPendingCommands()
        {
			DateTime wait;

            //Debug.Log("RunPendingCommands:");
            while (true)
            {
                if (_mPendingCommands.Count == 0)
                {
                    yield return null;
                    continue;
                }
                string command = _mPendingCommands[0];
                //Debug.LogFormat("RunPendingCommands: command={0}", command);
                if (command == "ClearPendingCommands")
                {
                    _mPendingCommands.Clear();
                    yield return null;
                    continue;
                }
                else if (command == "SetProxyPort")
                {
                    _mPendingCommands.Clear();
                    _mPort = _mNextPort;
                    yield return null;
                    continue;
                }
                string url = string.Format("http://localhost:{0}/{1}", _mPort, command);
                IWWW www = CreateWWW(url);
                while (null == www.GetError() &&
                    !www.IsDone())
                {
#if UNITY_2018_1_OR_NEWER
                    yield return www.SendWebRequest();
#else
                    yield return null;
#endif
                }

                string error = www.GetError();
                bool hasError = !string.IsNullOrEmpty(error);
                www.Dispose();
                if (hasError)
                {
                    Debug.LogError(error);
					wait = DateTime.Now + TimeSpan.FromSeconds(1);
					while (DateTime.Now < wait)
					{
						yield return null;
					}
                    continue;
                }
                else
                {
                    _mPendingCommands.RemoveAt(0); //command complete
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Get the proxy utterances and then fire callback
        /// </summary>
        /// <returns></returns>
        IEnumerator ProxyUtterance()
        {
			DateTime wait;

            //Debug.Log("ProxyUtterance:");
            while (true)
            {
                string url = string.Format("http://localhost:{0}/SpeechSynthesisProxyUtterance", _mPort);
                IWWW www = CreateWWW(url);
                while (null == www.GetError() &&
                    !www.IsDone())
                {
#if UNITY_2018_1_OR_NEWER
                    yield return www.SendWebRequest();
#else
                    yield return null;
#endif
                }

                string error = www.GetError();
                bool hasError = !string.IsNullOrEmpty(error);
                string text = null;
                if (!hasError)
                {
                    text = www.GetText();
                }
                www.Dispose();
                if (hasError)
                {
                    //Debug.LogError(error); //switch to status messages
					wait = DateTime.Now + TimeSpan.FromSeconds(1);
					while (DateTime.Now < wait)
					{
						yield return null;
					}
                    continue;
                }
                /*
                if (null != text)
                {
                    Debug.LogFormat("ProxyUtterance: text={0}", text);
                }
                */
                int index;
                if (int.TryParse(text, out index))
                {
                    //Debug.LogFormat("ProxyUtterance: text={0}", text);
                    if (_mUtteranceCallbacks.Count > 0)
                    {
                        Action<SpeechSynthesisUtterance> callback = _mUtteranceCallbacks[0];
                        _mUtteranceCallbacks.RemoveAt(0);
                        if (null != callback)
                        {
                            SpeechSynthesisUtterance utterance = new SpeechSynthesisUtterance();
                            utterance._mReference = index;
                            callback.Invoke(utterance);
                        }
                    }
                    yield break;
                }
                else
                {
					wait = DateTime.Now + TimeSpan.FromSeconds(1);
					while (DateTime.Now < wait)
					{
						yield return null;
					}
                    continue;
                }
            }
        }

        /// <summary>
        /// Get the proxy voices and then fire callback
        /// </summary>
        /// <returns></returns>
        IEnumerator ProxyVoices()
        {
			DateTime wait;

            //Debug.Log("ProxyVoices:");
            while (true)
            {
                string url = string.Format("http://localhost:{0}/SpeechSynthesisProxyVoices", _mPort);
                IWWW www = CreateWWW(url);
                while (null == www.GetError() &&
                    !www.IsDone())
                {
#if UNITY_2018_1_OR_NEWER
                    yield return www.SendWebRequest();
#else
                    yield return null;
#endif
                }

                string error = www.GetError();
                bool hasError = !string.IsNullOrEmpty(error);
                string jsonData = null;
                if (!hasError)
                {
                    jsonData = www.GetText();
                }
                www.Dispose();
                if (hasError)
                {
                    // Debug.LogError(error); //switch to status messages
					wait = DateTime.Now + TimeSpan.FromSeconds(1);
					while (DateTime.Now < wait)
					{
						yield return null;
					}
                    continue;
                }
                if (string.IsNullOrEmpty(jsonData))
                {
					wait = DateTime.Now + TimeSpan.FromSeconds(1);
					while (DateTime.Now < wait)
					{
						yield return null;
					}
                    continue;
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
                
                if (_mGetVoicesCallbacks.Count > 0)
                {
                    Action<VoiceResult> callback = _mGetVoicesCallbacks[0];
                    _mGetVoicesCallbacks.RemoveAt(0);
                    if (null != callback)
                    {
                        callback.Invoke(result);
                    }
                }
                yield break;
            }
        }

        /// <summary>
        /// Get the proxy on end events and then fire callback
        /// </summary>
        /// <returns></returns>
        IEnumerator ProxyOnEnd()
        {
			DateTime wait;

            //Debug.Log("ProxyOnEnd:");
            while (true)
            {
                string url = string.Format("http://localhost:{0}/SpeechSynthesisProxyOnEnd", _mPort);
                IWWW www = CreateWWW(url);
                while (null == www.GetError() &&
                    !www.IsDone())
                {
#if UNITY_2018_1_OR_NEWER
                    yield return www.SendWebRequest();
#else
                    yield return null;
#endif
                }

                string error = www.GetError();
                bool hasError = !string.IsNullOrEmpty(error);
                string jsonData = null;
                if (!hasError)
                {
                    jsonData = www.GetText();
                }
                www.Dispose();
                if (hasError)
                {
                    // Debug.LogError(error); //switch to status messages
					wait = DateTime.Now + TimeSpan.FromSeconds(1);
					while (DateTime.Now < wait)
					{
						yield return null;
					}
                    continue;
                }
                if (string.IsNullOrEmpty(jsonData))
                {
					wait = DateTime.Now + TimeSpan.FromSeconds(1);
					while (DateTime.Now < wait)
					{
						yield return null;
					}
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
                yield return new WaitForFixedUpdate();
            }
        }

        /// <summary>
        /// Check if the proxy is available
        /// </summary>
        /// <returns></returns>
        public bool IsAvailable()
        {
            return _mIsAvailable;
        }

        /// <summary>
        /// Pass a command to the proxy
        /// </summary>
        /// <returns></returns>
        private void AddCommand(string command)
        {
            //Debug.LogFormat("AddCommand: {0}", command);
            _mPendingCommands.Add(command);
        }

        /// <summary>
        /// Create an utterance and pass to the callback
        /// </summary>
        /// <param name="callback"></param>
        public void CreateSpeechSynthesisUtterance(Action<SpeechSynthesisUtterance> callback)
        {
            if (null == callback)
            {
                Debug.LogError("Callback was not set!");
                return;
            }
            _mUtteranceCallbacks.Add(callback);
            AddCommand("SpeechSynthesisCreateSpeechSynthesisUtterance");
            SafeStartCoroutine("ProxyUtterance", ProxyUtterance());
        }

        public void GetVoices(Action<VoiceResult> callback)
        {
            if (null == callback)
            {
                Debug.LogError("Callback was not set!");
                return;
            }
            AddCommand("SpeechSynthesisGetVoices");
            _mGetVoicesCallbacks.Add(callback);
            SafeStartCoroutine("ProxyVoices", ProxyVoices());
        }

        public void SetPitch(SpeechSynthesisUtterance utterance, float pitch)
        {
            if (null == utterance)
            {
                Debug.LogError("Utterance was not set!");
                return;
            }
            string msg = string.Format("SpeechSynthesisSetPitch?utterance={0}&pitch={1}", utterance._mReference, pitch);
            AddCommand(msg);
        }

        public void SetRate(SpeechSynthesisUtterance utterance, float rate)
        {
            if (null == utterance)
            {
                Debug.LogError("Utterance was not set!");
                return;
            }
            string msg = string.Format("SpeechSynthesisSetRate?utterance={0}&rate={1}", utterance._mReference, rate);
            AddCommand(msg);
        }

        public void SetText(SpeechSynthesisUtterance utterance, string text)
        {
            if (null == utterance)
            {
                Debug.LogError("Utterance was not set!");
                return;
            }
            string encoded = string.Empty;
            if (!string.IsNullOrEmpty(text))
            {
                byte[] bytes = UTF8Encoding.UTF8.GetBytes(text);
                encoded = Convert.ToBase64String(bytes);
            }
            string msg = string.Format("SpeechSynthesisSetText?utterance={0}&text={1}", utterance._mReference, encoded);
            AddCommand(msg);
        }

        public void SetVolume(SpeechSynthesisUtterance utterance, float volume)
        {
            if (null == utterance)
            {
                Debug.LogError("Utterance was not set!");
                return;
            }
            string msg = string.Format("SpeechSynthesisSetVolume?utterance={0}&volume={1}", utterance._mReference, volume);
            AddCommand(msg);
        }

        public void Speak(SpeechSynthesisUtterance utterance)
        {
            if (null == utterance)
            {
                Debug.LogError("Utterance was not set!");
                return;
            }
            string msg = string.Format("SpeechSynthesisSpeak?utterance={0}", utterance._mReference);
            AddCommand(msg);
        }

        public void Cancel()
        {
            AddCommand("SpeechSynthesisCancel");
        }

        public void SetVoice(SpeechSynthesisUtterance utterance, Voice voice)
        {
            if (null == utterance)
            {
                Debug.LogError("Utterance was not set!");
                return;
            }
            string encoded = string.Empty;
            string voiceURI = voice.voiceURI;
            if (!string.IsNullOrEmpty(voiceURI))
            {
                byte[] bytes = UTF8Encoding.UTF8.GetBytes(voiceURI);
                encoded = Convert.ToBase64String(bytes);
            }
            string msg = string.Format("SpeechSynthesisSetVoice?utterance={0}&voice={1}", utterance._mReference, encoded);
            AddCommand(msg);
        }

        public void ManagementCloseBrowserTab()
        {
            AddCommand("CloseBrowserTab");
        }

        public void ManagementCloseProxy()
        {
            AddCommand("CloseProxy");
            _mPendingCommands.Add("ClearPendingCommands");
        }

#if UNITY_EDITOR || UNITY_STANDALONE

        private static string GetAppDataFolder()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), KEY_CHROME_SPEECH_PROXY);
            return path;
        }

        private static string GetAppConfig()
        {
            string path = Path.Combine(GetAppDataFolder(), "app.config");
            return path;
        }

        private static void SetupAppDataFolder()
        {
            string path = GetAppDataFolder();
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception)
            {

            }
        }

        private static SpeechProxyConfig GetSpeechProxyConfig()
        {
            SetupAppDataFolder();
            string path = GetAppConfig();
            SpeechProxyConfig speechProxyConfig = null;
            try
            {
                if (File.Exists(path))
                {
                    using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            string jsonData = sr.ReadToEnd();
                            //Debug.Log(jsonData);
                            speechProxyConfig = JsonUtility.FromJson<SpeechProxyConfig>(jsonData);
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
            if (null == speechProxyConfig)
            {
                speechProxyConfig = new SpeechProxyConfig();
            }
            return speechProxyConfig;
        }

#endif

        public void ManagementLaunchProxy()
        {
            _mPendingCommands.Insert(0, "ClearPendingCommands");

#if (UNITY_EDITOR || UNITY_STANDALONE) && !UNITY_WEBPLAYER
            string path = GetAppDataFolder();
            if (!Directory.Exists(path))
            {
                Debug.LogError("The Speech Proxy needs to run once to set the install path!");
                return;
            }

            //Debug.LogFormat("Data folder: {0}", path);

            SpeechProxyConfig speechProxyConfig = GetSpeechProxyConfig();
            //Debug.LogFormat("AppName: {0}", speechProxyConfig.appName);
            //Debug.LogFormat("InstallDirectory: {0}", speechProxyConfig.installDirectory);
            //Debug.LogFormat("ProxyPort: {0}", speechProxyConfig.proxyPort);
#endif


#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN) && !UNITY_WEBPLAYER
            const string APP_CMD = @"C:\Windows\System32\cmd.exe";
            try
            {
                Process process = new Process();
                StringBuilder args = new StringBuilder();
                args.AppendFormat("/c start \"\" \"{0}\\{1}\" {2}",
                    speechProxyConfig.installDirectory,
                    speechProxyConfig.appName,
                    _mPort);
                //Debug.Log(args.ToString());
                process.StartInfo = new ProcessStartInfo(APP_CMD, args.ToString());
                process.StartInfo.WorkingDirectory = speechProxyConfig.installDirectory;
                process.Exited += (sender, e) =>
                {
                    process.Dispose();
                };
                process.Start();
            }
            catch (Exception)
            {

            }
#elif (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX) && !UNITY_WEBPLAYER
            const string APP_MONO_PATH = "/Library/Frameworks/Mono.framework/Versions/Current/Commands/mono";
            try
            {
                Process process = new Process();
                string launch = string.Format("{0}/{1}",
                                              speechProxyConfig.installDirectory,
                                              speechProxyConfig.appName);
                ProcessStartInfo psi = new ProcessStartInfo(APP_MONO_PATH, launch);
                psi.WorkingDirectory = speechProxyConfig.installDirectory;
                process.StartInfo = psi;
                process.Exited += (sender, e) =>
                {
                    //Debug.LogFormat("Process exited.");
                    process.Dispose();
                };
                //Debug.LogFormat("Launch: {0} {1}", APP_BASH_MAC, args);
                process.Start();
            }
            catch (Exception)
            {

            }
#endif
        }

        public void ManagementOpenBrowserTab()
        {
            AddCommand("OpenBrowserTab");
        }

        public void ManagementSetProxyPort(int port)
        {
            _mNextPort = port;
            string msg = string.Format("SetProxyPort?port={0}", port);
            AddCommand(msg);
            AddCommand("SetProxyPort");
        }

        protected virtual IWWW CreateWWW(string url)
        {
            return new WWWPlayMode(url);
        }
    }
}
