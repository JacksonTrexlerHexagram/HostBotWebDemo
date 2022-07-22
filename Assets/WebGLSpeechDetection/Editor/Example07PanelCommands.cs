using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityWebGLSpeechDetection
{
    public class Example07PanelCommands : EditorWindow
    {
        /// <summary>
        /// Holds the languages and dialects
        /// </summary>
        private LanguageResult _mLanguageResult = null;

        /// <summary>
        /// The selected language index
        /// </summary>
        private int _mLanguage = 0;
        private string[] _mLanguages = new string[] { "Languages" };

        /// <summary>
        /// The selected dialog index
        /// </summary>
        private int _mDialect = 0;
        private string[] _mDialects = new string[] { "Dialects" };

        /// <summary>
        /// Is the detection event initialized
        /// </summary>
        private bool _mInitialized = false;

        private Dictionary<string, MethodInfo> _mEditorCommands = new Dictionary<string, MethodInfo>();

        /// <summary>
        /// Dictionary of commands
        /// </summary>
        private Dictionary<string, DateTime> _mTimers = new Dictionary<string, DateTime>();

        /// <summary>
        /// Run UI actions on the main thread
        /// </summary>
        private List<Action> _mPendingActions = new List<Action>();

		/// <summary>
		/// Display status text
		/// </summary>
		private static string _sTextStatus = string.Empty;

		/// <summary>
		/// Set the status text and repaint
		/// </summary>
		/// <param name="text">Text.</param>
		private void SetTextStatus(string text)
		{
			//Debug.Log(text);

			_sTextStatus = text;

			//Repaint in the update event
			Action action = () =>
			{
				Repaint();
			};
			_mPendingActions.Add(action);
		}

        /// <summary>
        /// Initially not initialized
        /// </summary>
        private void OnEnable()
        {
			SetTextStatus("OnEnable");

            _mEditorCommands.Clear();

            Assembly assembly = Assembly.GetExecutingAssembly();
            if (null != assembly)
            {
                foreach (var type in assembly.GetTypes())
                {
                    foreach (var methodInfo in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
                    {
                        foreach (var attribute in methodInfo.GetCustomAttributes(false))
                        {
                            if (attribute is SpeechDetectionAttribute)
                            {
                                SpeechDetectionAttribute attr = attribute as SpeechDetectionAttribute;
                                //Debug.Log(attr.GetSpokenPhrase());
                                _mEditorCommands[attr.GetSpokenPhrase()] = methodInfo;
                                break;
                            }
                        }
                    }
                }
            }


            _mInitialized = false;
            _mTimers.Clear();
            foreach (KeyValuePair<string, MethodInfo> kvp in _mEditorCommands)
            {
                _mTimers[kvp.Key] = DateTime.MinValue;
            }
        }

        bool RunEditorCommands(string word)
        {
            foreach (KeyValuePair<string, MethodInfo> kvp in _mEditorCommands)
            {
                if (word.Contains(kvp.Key))
                {
                    if (!_mTimers.ContainsKey(kvp.Key) ||
                        _mTimers[kvp.Key] < DateTime.Now)
                    {
                        kvp.Value.Invoke(null, null);
                        _mTimers[kvp.Key] = DateTime.Now + TimeSpan.FromSeconds(1);

						SetTextStatus(kvp.Key);

                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Handler for speech detection events
        /// </summary>
        /// <param name="args"></param>
        bool HandleDetectionResult(DetectionResult detectionResult)
        {
            bool enabled = EditorProxySpeechDetectionPlugin.IsEnabled();
            if (!enabled)
            {
                return false;
            }

            //Debug.Log("Example06PanelDictation: HandleDetectionResult:");
            if (null == detectionResult)
            {
                return false;
            }
            SpeechRecognitionResult[] results = detectionResult.results;
            if (null == results)
            {
                return false;
            }
            bool doAbort = false;
            foreach (SpeechRecognitionResult result in results)
            {
                if (doAbort)
                {
                    break;
                }
                SpeechRecognitionAlternative[] alternatives = result.alternatives;
                if (null == alternatives)
                {
                    continue;
                }
                foreach (SpeechRecognitionAlternative alternative in alternatives)
                {
                    if (string.IsNullOrEmpty(alternative.transcript))
                    {
                        continue;
                    }
                    string lower = alternative.transcript.ToLower();
					SetTextStatus(lower);
                    if (RunEditorCommands(lower))
                    {
                        doAbort = true;
                        break;
                    }
                }
            }

            if (doAbort)
            {
                EditorProxySpeechDetectionPlugin plugin = EditorProxySpeechDetectionPlugin.GetInstance();
                if (null != plugin)
                {
                    plugin.Abort();
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Handle multiple panels open and changing languages
        /// </summary>
        /// <param name="languageChangedResult"></param>
        void HandleOnLanguageChanged(LanguageChangedResult languageChangedResult)
        {
            ISpeechDetectionPlugin plugin = EditorProxySpeechDetectionPlugin.GetInstance();
            int languageIndex = _mLanguage;
            if (SpeechDetectionUtils.HandleOnLanguageChanged(
                    ref _mLanguage,
                    _mLanguages,
                    ref _mDialect,
                    _mDialects,
                    _mLanguageResult,
                    plugin,
                    languageChangedResult))
            {
                if (languageIndex != _mLanguage)
                {
                    SpeechDetectionUtils.HandleLanguageChanged(_mLanguages,
                                    _mLanguage,
                                    out _mDialects,
                                    ref _mDialect,
                                    _mLanguageResult,
                                    plugin);
                }
                Repaint();
            }
        }

        /// <summary>
        /// Reinitialize the editor proxy when enabled and not compiling
        /// </summary>
        private void Update()
        {
            bool enabled = EditorProxySpeechDetectionPlugin.IsEnabled();

            if (!enabled ||
                EditorApplication.isCompiling)
            {
                _mInitialized = false;
                return;
            }

            // handle init
            EditorProxySpeechDetectionPlugin plugin = EditorProxySpeechDetectionPlugin.GetInstance();
            if (null != plugin)
            {
                if (!_mInitialized)
                {
                    if (!plugin.IsAvailable())
                    {
                        return;
                    }

                    //Debug.LogFormat("Update: Plugin is available");

                    plugin.GetLanguages((languageResult) =>
                    {
                        _mLanguageResult = languageResult;

                        // populate the language options
                        SpeechDetectionUtils.PopulateLanguages(out _mLanguages, out _mLanguage, languageResult);

                        // Restore the default language
                        SpeechDetectionUtils.RestoreLanguage(_mLanguages, out _mLanguage);

                        // Handle setting the default language
                        SpeechDetectionUtils.PausePlayerPrefs(true);
                        SpeechDetectionUtils.HandleLanguageChanged(_mLanguages,
                            _mLanguage,
                            out _mDialects,
                            ref _mDialect,
                            _mLanguageResult,
                            plugin);
                        SpeechDetectionUtils.PausePlayerPrefs(false);

                        // Restore the default dialect
                        SpeechDetectionUtils.RestoreDialect(_mDialects, out _mDialect);

                        // Update UI
                        Repaint();
                    });

                    // subscribe to events
                    plugin.AddListenerOnDetectionResult(HandleDetectionResult);

                    // get on language changed events
                    plugin.AddListenerOnLanguageChanged(HandleOnLanguageChanged);

                    //Debug.Log("Update: HandleDetectionResult subscribed");

                    _mInitialized = true;

                    Repaint();
                }
            }

            // Check timers to deactivate button after delay
            bool doRepaint = false;
            foreach (KeyValuePair<string, MethodInfo> kvp in _mEditorCommands)
            {
                DateTime timer = _mTimers[kvp.Key];
                if (timer != DateTime.MinValue &&
                    timer < DateTime.Now)
                {
                    _mTimers[kvp.Key] = DateTime.MinValue;
                    doRepaint = true;
                }
            }

            if (doRepaint)
            {
                Repaint();
            }

            // run pending UI actions
            while (_mPendingActions.Count > 0)
            {
                Action action = _mPendingActions[0];
                _mPendingActions.RemoveAt(0);
                action.Invoke();
            }
        }

        /// <summary>
        /// Display GUI in the editor panel
        /// </summary>
        private void OnGUI()
        {
            EditorProxySpeechDetectionPlugin plugin = EditorProxySpeechDetectionPlugin.GetInstance();

            bool enabled = EditorProxySpeechDetectionPlugin.IsEnabled();
            if (!enabled)
            {
                if (null != plugin)
                {
                    // unsubscribe to events
                    plugin.RemoveListenerOnDetectionResult(HandleDetectionResult);
                }
            }

            bool isCompiling = EditorApplication.isCompiling;
            if (isCompiling)
            {
                GUI.enabled = false;
            }
            bool newEnabled = GUILayout.Toggle(enabled, "Enabled");
            if (newEnabled != enabled)
            {
                EditorProxySpeechDetectionPlugin.SetEnabled(newEnabled);
            }
            if (isCompiling)
            {
                GUI.enabled = true;
            }

            if (null != plugin && plugin.IsAvailable())
            {
                GUILayout.Label("IsAvailable: true");
            }
            else
            {
                GUILayout.Label("IsAvailable: false");
            }

            if (isCompiling ||
                !newEnabled)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Open Browser Tab"))
            {
                if (null != plugin)
                {
					SetTextStatus("Open Browser Tab");
                    plugin.ManagementOpenBrowserTab();
                }
            }
            if (GUILayout.Button("Close Browser Tab"))
            {
                if (null != plugin)
                {
					SetTextStatus("Close Browser Tab");
                    plugin.ManagementCloseBrowserTab();
                }
            }
            GUILayout.BeginHorizontal(GUILayout.Width(position.width));
            int port = 5000;
            if (null != plugin)
            {
                port = plugin._mPort;
            }
            string strPort = GUILayout.TextField(string.Format("{0}", port));
            if (int.TryParse(strPort, out port))
            {
                if (null != plugin)
                {
                    plugin._mPort = port;
                }
            }
            if (GUILayout.Button("Set Proxy Port"))
            {
                if (null != plugin)
                {
					SetTextStatus(string.Format("Set Proxy Tab: port={0}", plugin._mPort));
                    plugin.ManagementSetProxyPort(plugin._mPort);
                }
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Launch Proxy"))
            {
                if (null != plugin)
                {
					SetTextStatus("Launch Proxy");
                    plugin.ManagementLaunchProxy();
                }
            }
            if (GUILayout.Button("Close Proxy"))
            {
                if (null != plugin)
                {
					SetTextStatus("Close Proxy");
                    plugin.ManagementCloseProxy();
                }
            }

            int newLanguage = EditorGUILayout.Popup(_mLanguage, _mLanguages);
            if (_mLanguage != newLanguage)
            {
                _mLanguage = newLanguage;
                if (null != plugin)
                {
                    SpeechDetectionUtils.HandleLanguageChanged(_mLanguages,
                        _mLanguage,
                        out _mDialects,
                        ref _mDialect,
                        _mLanguageResult,
                        plugin);
                }
            }

            if (!isCompiling &&
                newEnabled)
            {
                GUI.enabled = _mLanguage != 0;
            }
            int newDialect = EditorGUILayout.Popup(_mDialect, _mDialects);
            if (_mDialect != newDialect)
            {
                _mDialect = newDialect;
                if (null != plugin)
                {
                    SpeechDetectionUtils.HandleDialectChanged(_mDialects,
                        _mDialect,
                        _mLanguageResult,
                        plugin);
                }
            }
            if (!isCompiling &&
                newEnabled)
            {
                GUI.enabled = true;
            }

            GUI.skin.label.wordWrap = true;
            GUILayout.Label("Say any of the following words which will highlight when detected.", GUILayout.Width(position.width), GUILayout.Height(40));
            GUI.skin.label.wordWrap = false;

            Color oldColor = GUI.backgroundColor;
            foreach (KeyValuePair<string, MethodInfo> kvp in _mEditorCommands)
            {
                DateTime timer = _mTimers[kvp.Key];
                if (timer > DateTime.Now)
                {
                    GUI.backgroundColor = Color.red;
                }
                else
                {
                    GUI.backgroundColor = oldColor;
                }
                GUILayout.Button(kvp.Key);
            }
            GUI.backgroundColor = oldColor;

			GUILayout.Label(_sTextStatus, GUILayout.Width(position.width), GUILayout.Height(40));

            if (isCompiling ||
                !newEnabled)
            {
                GUI.enabled = true;
            }
        }
    }
}
