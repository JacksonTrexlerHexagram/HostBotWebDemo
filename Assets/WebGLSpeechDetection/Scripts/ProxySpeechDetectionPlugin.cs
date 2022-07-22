using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR || UNITY_STANDALONE
using System.Diagnostics;
using System.IO;
using System.Text;
#endif
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UnityWebGLSpeechDetection
{
    public class ProxySpeechDetectionPlugin : BaseSpeechDetectionPlugin, ISpeechDetectionPlugin
    {
		const string KEY_CHROME_SPEECH_PROXY = "CHROME_SPEECH_PROXY";
		
        /// <summary>
        /// Default proxy port
        /// </summary>
        public int _mPort = 5000;

        /// <summary>
        /// Singleton reference
        /// </summary>
        private static ProxySpeechDetectionPlugin _sInstance = null;

        /// <summary>
        /// Available after proxy responds
        /// </summary>
        protected bool _mIsAvailable = false;

        /// <summary>
        /// Change to the next port after proxy is set
        /// </summary>
        private int _mNextPort = 0;

        /// <summary>
        /// Pending commands
        /// </summary>
        private List<string> _mPendingCommands = new List<string>();

        /// <summary>
        /// Get singleton instance
        /// </summary>
        public static ProxySpeechDetectionPlugin GetInstance()
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
        void Start()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            SafeStartCoroutine("Init", Init());
#else
            gameObject.SetActive(false);
#endif
        }

        protected IEnumerator Init()
        {
            //Debug.Log("ProxySpeechDetectionPlugin: Init:");
            while (true)
            {
                string url = string.Format("http://localhost:{0}/SpeechDetectionInit", _mPort);
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
					//Debug.LogErrorFormat("Init: error={0}", error); //switch to status message
					DateTime wait = DateTime.Now + TimeSpan.FromSeconds(1);
					while (wait > DateTime.Now)
					{
						yield return null;
					}
                    continue;
                }
                else
                {
                    _mIsAvailable = true;
                    //Debug.LogFormat("Init: available={0}", _mIsAvailable);
                    SafeStartCoroutine("RunPendingCommands", RunPendingCommands());
                    SafeStartCoroutine("GetResult", GetResult());
                    yield break;
                }
            }
        }

        protected virtual IWWW CreateWWW(string url)
        {
            return new WWWPlayMode(url);
        }

        /// <summary>
        /// Run commands in order
        /// </summary>
        /// <returns></returns>
        protected IEnumerator RunPendingCommands()
        {
            //Debug.Log("ProxySpeechDetectionPlugin: RunPendingCommands:");
            while (true)
            {
                if (_mPendingCommands.Count == 0)
                {
                    yield return null;
                    continue;
                }
                string command = _mPendingCommands[0];
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
                    //Debug.LogError(error); //use a status message event
					DateTime wait = DateTime.Now + TimeSpan.FromSeconds(1);
					while (wait > DateTime.Now)
					{
						yield return null;
					}
                    continue;
                }
                else
                {
                    if (_mPendingCommands.Count > 0)
                    {
                        _mPendingCommands.RemoveAt(0); //command complete
                    }
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Proxy speech detection results
        /// </summary>
        /// <returns></returns>
        protected IEnumerator GetResult()
        {
			DateTime wait;
            //Debug.Log("ProxySpeechDetectionPlugin: GetResult:");
            while (true)
            {
                string url = string.Format("http://localhost:{0}/SpeechDetectionGetResult", _mPort);
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
                if (!hasError)
                {
                    string jsonData = www.GetText();
                    if (!string.IsNullOrEmpty(jsonData))
                    {
                        //Debug.LogFormat("ProxySpeechDetectionPlugin: GetResult: jsondata={0}", jsonData);
                        DetectionResult detectionResult = JsonUtility.FromJson<DetectionResult>(jsonData);
                        Invoke(detectionResult);
                    }
                }
                www.Dispose();
                if (hasError)
                {
					//Debug.LogError(error); // change to status message
					wait = DateTime.Now + TimeSpan.FromSeconds(1);
					while (wait > DateTime.Now)
					{
						yield return null;
					}
                    continue;
                }
				wait = DateTime.Now + TimeSpan.FromSeconds(0.1f);
				while (wait > DateTime.Now)
				{
					yield return null;
				}
            }
        }

        /// <summary>
        /// Start recognition
        /// </summary>
        public void StartRecognition()
        {
            AddCommand("SpeechDetectionStartRecognition");
        }

        /// <summary>
        /// Stop recognition
        /// </summary>
        public void StopRecognition()
        {
            AddCommand("SpeechDetectionStopRecognition");
        }

        /// <summary>
        /// Abort detection of the current word
        /// </summary>
        public void Abort()
        {
            AddCommand("SpeechDetectionAbort");
        }

        /// <summary>
        /// Check if the proxy is available
        /// </summary>
        /// <returns></returns>
        public bool IsAvailable()
        {
            return _mIsAvailable;
        }

        private void DebugCommands()
        {
            for (int i = 0; i < _mPendingCommands.Count; ++i)
            {
                Debug.LogFormat("{0} - Command - {1}", i, _mPendingCommands[i]);
            }
        }

        /// <summary>
        /// Pass a command to the proxy
        /// </summary>
        /// <returns></returns>
        private void AddCommand(string command)
        {
            //Debug.LogFormat("AddCommand: {0}", command);
            _mPendingCommands.Add(command);
            //DebugCommands();
        }

        /// <summary>
        /// Set detection language
        /// </summary>
        /// <param name="lang"></param>
        public void SetLanguage(string lang)
        {
            string msg = string.Format("SpeechDetectionSetLanguage?lang={0}", lang);
            AddCommand(msg);
        }

        /// <summary>
        /// Proxy languages
        /// </summary>
        /// <returns></returns>
        private IEnumerator ProxyLanguages(Action<LanguageResult> callback)
        {
			DateTime wait;
            while (true)
            {
                string url = string.Format("http://localhost:{0}/SpeechDetectionGetLanguages", _mPort);
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
                    //Debug.LogError(error); //switch to status message event
					wait = DateTime.Now + TimeSpan.FromSeconds(1);
					while (wait > DateTime.Now)
					{
						yield return null;
					}
                    continue;
                }
                if (!string.IsNullOrEmpty(jsonData))
                {
                    //Debug.Log(jsonData);
                    LanguageResult languageResult = null;
                    try
                    {
                        languageResult = JsonUtility.FromJson<LanguageResult>(jsonData);
                        //Debug.Log(jsonData);
                    }
                    catch (Exception)
                    {
                        Debug.LogError(string.Format("Failed to decode json: {0}", jsonData));
                        hasError = true;
                    }
                    if (hasError)
                    {
                        wait = DateTime.Now + TimeSpan.FromSeconds(1);
                        while (wait > DateTime.Now)
                        {
                            yield return null;
                        }
                        continue;
                    }
                    callback.Invoke(languageResult);
                    yield break;
                }
				wait = DateTime.Now + TimeSpan.FromSeconds(1);
				while (wait > DateTime.Now)
				{
					yield return null;
				}
            }
        }

        public void GetLanguages(Action<LanguageResult> callback)
        {
            SafeStartCoroutine("ProxyLanguages", ProxyLanguages(callback));
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
    }
}
