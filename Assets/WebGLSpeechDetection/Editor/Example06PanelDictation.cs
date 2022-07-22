using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityWebGLSpeechDetection
{
    public class Example06PanelDictation : EditorWindow
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
        /// The list of detected words
        /// </summary>
        private List<string> _mWords = new List<string>();

        /// <summary>
        /// Is the detection event initialized
        /// </summary>
        private bool _mInitialized = false;

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
            _mInitialized = false;
        }

        /// <summary>
        /// Handler for speech detection events
        /// </summary>
        /// <param name="detectionResult"></param>
        /// <returns></returns>
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
            foreach (SpeechRecognitionResult result in results)
            {
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
                    if (result.isFinal)
                    {
                        _mWords.Add(string.Format("[FINAL] \"{0}\" Confidence={1}",
                            alternative.transcript,
                            alternative.confidence));
                    }
                    else
                    {
                        _mWords.Add(string.Format("\"{0}\" Confidence={1}",
                            alternative.transcript,
                            alternative.confidence));
                    }
                }
            }
            while (_mWords.Count > 15)
            {
                _mWords.RemoveAt(0);
            }
            Repaint();

            // dictation doesn't need to handle the event
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

                    // Reset the detection list
                    _mWords.Clear();
                    _mWords.Add("Say something...");

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

            for (int index = 0; index < _mWords.Count; ++index)
            {
                GUILayout.Label(_mWords[index]);
            }

			GUILayout.Label(_sTextStatus, GUILayout.Width(position.width), GUILayout.Height(40));

            if (isCompiling ||
                !newEnabled)
            {
                GUI.enabled = true;
            }
        }
    }
}
