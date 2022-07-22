using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityWebGLSpeechSynthesis
{
    public class Example05PanelSynthesis : EditorWindow
    {
        /// <summary>
        /// Reference to the supported voices
        /// </summary>
        private VoiceResult _mVoiceResult = null;

        /// <summary>
        /// Reference to the utterance which holds the voice and text to speak
        /// </summary>
        private SpeechSynthesisUtterance _mSpeechSynthesisUtterance = null;

        /// <summary>
        /// The selected voice index
        /// </summary>
        private int _mVoice = 0;
        private string[] _mVoices = new string[] { "Voices" };

        /// <summary>
        /// The rate of words to speak
        /// </summary>
        private float _mRate = 0.5f;

        /// <summary>
        /// The pitch of words to speak
        /// </summary>
        private float _mPitch = 0.5f;

        /// <summary>
        /// The words to speak
        /// </summary>
        private string _mText = "Text to speech is great! Thumbs Up!";

        /// <summary>
        /// Is the panel initialized
        /// </summary>
        private bool _mInitialized = false;

        /// <summary>
        /// Toggle does speak on mouse up
        /// </summary>
        private bool _mSpeakOnMouseUp = false;

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

        [MenuItem("Window/WebGLSpeechSynthesis/Open Example05PanelSynthesis")]
        private static void OpenPanel()
        {
            Example05PanelSynthesis panel =
                (Example05PanelSynthesis)EditorWindow.GetWindow(typeof(Example05PanelSynthesis),
                false,
                "Example05PanelSynthesis");
            panel.Show();
        }

        /// <summary>
        /// Initially not initialized
        /// </summary>
        private void OnEnable()
        {
            _mInitialized = false;
        }

        /// <summary>
        /// Reinitialize the editor proxy when enabled and not playing and not compiling
        /// </summary>
        private void Update()
        {
            bool enabled = EditorProxySpeechSynthesisPlugin.IsEnabled();

            if (!enabled ||
                EditorApplication.isCompiling)
            {
                _mInitialized = false;
                return;
            }

            EditorProxySpeechSynthesisPlugin plugin = EditorProxySpeechSynthesisPlugin.GetInstance();
            if (null != plugin)
            {
                if (!_mInitialized)
                {
                    if (!plugin.IsAvailable())
                    {
                        return;
                    }

                    //Debug.LogFormat("Update: Plugin is available");

                    plugin.GetVoices((voiceResult) =>
                    {
                        _mVoiceResult = voiceResult;

                        // populate the voice options
                        SpeechSynthesisUtils.PopulateVoices(out _mVoices, out _mVoice, voiceResult);

                        // Set default voice if ready
                        SetIfReadyForDefaultVoice(plugin);

                        // Update UI
                        Repaint();
                    });

                    // Create an instance of SpeechSynthesisUtterance
                    plugin.CreateSpeechSynthesisUtterance((utterance) =>
                    {
                        SetTextStatus(string.Format("Utterance created: {0}", utterance._mReference));
                        _mSpeechSynthesisUtterance = utterance;

                        // Set default voice if ready
                        SetIfReadyForDefaultVoice(plugin);
                    });

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
        /// Set the default voice if voices and utterance are ready
        /// </summary>
        private void SetIfReadyForDefaultVoice(ISpeechSynthesisPlugin plugin)
        {
            if (null == plugin)
            {
                SetTextStatus("Plugin is not set!");
                return;
            }

            if (null != _mSpeechSynthesisUtterance &&
                null != _mVoices)
            {
                // Restore the default voice
                SpeechSynthesisUtils.RestoreVoice(_mVoices, out _mVoice);

                // Handle setting the default voice
                SpeechSynthesisUtils.PausePlayerPrefs(true);
                SpeechSynthesisUtils.HandleVoiceChanged(_mVoices,
                    _mVoice,
                    _mVoiceResult,
                    _mSpeechSynthesisUtterance,
                    plugin);
                SpeechSynthesisUtils.PausePlayerPrefs(false);

                // ready to handle voice changes
                Speak(plugin);
            }
        }

        /// <summary>
        /// Speak the utterance
        /// </summary>
        private void Speak(ISpeechSynthesisPlugin plugin)
        {
            if (null == plugin)
            {
                SetTextStatus("Plugin is not set!");
                return;
            }
            if (null == _mText)
            {
                SetTextStatus("Text is not set!");
                return;
            }
            if (null == _mSpeechSynthesisUtterance)
            {
                SetTextStatus("Utterance is not set!");
                return;
            }

            // Cancel if already speaking
            plugin.Cancel();

            // set the utterance pitch
            plugin.SetPitch(_mSpeechSynthesisUtterance, Mathf.Lerp(0.1f, 2f, _mPitch));

            // set the utterance rate
            plugin.SetRate(_mSpeechSynthesisUtterance, Mathf.Lerp(0.1f, 2f, _mRate));

            // Set the text that will be spoken
            plugin.SetText(_mSpeechSynthesisUtterance, _mText);

            // Use the plugin to speak the utterance
            plugin.Speak(_mSpeechSynthesisUtterance);
        }

        /// <summary>
        /// Display GUI in the editor panel
        /// </summary>
        private void OnGUI()
        {
            EditorProxySpeechSynthesisPlugin plugin = EditorProxySpeechSynthesisPlugin.GetInstance();

            if (_mSpeakOnMouseUp)
            {
                if (Event.current.rawType == EventType.MouseUp)
                {
                    _mSpeakOnMouseUp = false;
                    Speak(plugin);
                }
            }

            bool enabled = EditorProxySpeechSynthesisPlugin.IsEnabled();
            if (!enabled)
            {
                if (null != plugin)
                {
                    // unsubscribe to events
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
                EditorProxySpeechSynthesisPlugin.SetEnabled(newEnabled);
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

            int newVoice = EditorGUILayout.Popup(_mVoice, _mVoices);
            if (_mVoice != newVoice)
            {
                _mVoice = newVoice;
                if (null != plugin)
                {
                    if (_mVoice > 0)
                    {
                        SpeechSynthesisUtils.HandleVoiceChanged(_mVoices,
                        _mVoice,
                        _mVoiceResult,
                        _mSpeechSynthesisUtterance,
                        plugin);

                        Speak(plugin);
                    }
                }
            }

            bool oldWrap = EditorStyles.label.wordWrap;
            EditorStyles.label.wordWrap = true;
            EditorGUILayout.LabelField("Press the SPEAK button to play audio for the typed words.", GUILayout.Width(position.width), GUILayout.MaxHeight(40));
            EditorStyles.label.wordWrap = oldWrap;

            GUILayout.BeginHorizontal(GUILayout.Width(position.width));
            GUILayout.Label("Rate:");
            float newRate = EditorGUILayout.Slider(_mRate, 0f, 1f);
            if (newRate != _mRate)
            {
                _mRate = newRate;
                _mSpeakOnMouseUp = true;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(position.width));
            GUILayout.Label("Pitch:");
            float newPitch = EditorGUILayout.Slider(_mPitch, 0f, 1f);
            if (newPitch != _mPitch)
            {
                _mPitch = newPitch;
                _mSpeakOnMouseUp = true;
            }
            GUILayout.EndHorizontal();

            oldWrap = EditorStyles.textField.wordWrap;
            EditorStyles.textField.wordWrap = true;
            string newText = EditorGUILayout.TextArea(_mText, GUILayout.Width(position.width), GUILayout.Height(60));
            if (newText != _mText)
            {
                _mText = newText;
            }
            EditorStyles.textField.wordWrap = oldWrap;

            if (GUILayout.Button("Stop"))
            {
                if (null != plugin)
                {
                    SetTextStatus("Stop");
                    plugin.Cancel();
                }
            }

            if (GUILayout.Button("Speak"))
            {
                if (null != plugin)
                {
                    SetTextStatus("Speak");
                    Speak(plugin);
                }
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
