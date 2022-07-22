using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityWebGLSpeechSynthesis
{
    public class SpeechSynthesisUtils
    {
        private const string KEY_WEBGL_SPEECH_SYNTHESIS_PLUGIN_VOICE = "WEBGL_SPEECH_SYNTHESIS_PLUGIN_VOICE";

        public static ISpeechSynthesisPlugin GetInstance()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return WebGLSpeechSynthesisPlugin.GetInstance();
#else
            return ProxySpeechSynthesisPlugin.GetInstance();
#endif
        }

        private static bool _sPausePreferences = false;

        /// <summary>
        /// When changing UI things, pause setting player prefs
        /// </summary>
        /// <param name="toggle"></param>
        public static void PausePlayerPrefs(bool toggle)
        {
            _sPausePreferences = toggle;
        }

        public static void SetActive(bool value, params Component[] args)
        {
            if (null == args)
            {
                return;
            }
            for (int i = 0; i < args.Length; ++i)
            {
                Component component = args[i];
                if (component &&
                    component.gameObject)
                {
                    component.gameObject.SetActive(value);
                }
            }
        }

        public static void SetInteractable(bool interactable, params Selectable[] args)
        {
            if (null == args)
            {
                return;
            }
            for (int i = 0; i < args.Length; ++i)
            {
                Selectable selectable = args[i];
                if (selectable)
                {
                    selectable.interactable = interactable;
                }
            }
        }

        public static void PopulateDropdown(Dropdown dropdown, List<string> strings)
        {
            if (dropdown)
            {
                dropdown.ClearOptions();
                List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
                foreach (string str in strings)
                {
                    Dropdown.OptionData data = new Dropdown.OptionData();
                    data.text = str;
                    options.Add(data);
                }
                dropdown.AddOptions(options);
            }
        }

        public static void PopulateVoicesDropdown(Dropdown dropdown, VoiceResult result)
        {
            if (dropdown)
            {
                List<string> options = new List<string>();
                options.Add("Voices");
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
                        if (!string.IsNullOrEmpty(voice.display))
                        {
                            options.Add(voice.display);
                        }
                        else if (!string.IsNullOrEmpty(voice.name))
                        {
                            options.Add(voice.name);
                        }
                    }
                }
                SpeechSynthesisUtils.PopulateDropdown(dropdown, options);
            }
        }

        public static void PopulateVoices(out string[] voiceOptions, out int voiceIndex, VoiceResult result)
        {
            voiceOptions = null;
            voiceIndex = 0;
            if (null == result)
            {
                Debug.LogError("VoiceResult is null!");
                return;
            }

            List<string> options = new List<string>();
            options.Add("Voices");
            if (null != result.voices)
            {
                for (int i = 0; i < result.voices.Length; ++i)
                {
                    Voice voice = result.voices[i];
                    if (null == voice)
                    {
                        continue;
                    }
                    if (!string.IsNullOrEmpty(voice.display))
                    {
                        options.Add(voice.display);
                    }
                    else if (!string.IsNullOrEmpty(voice.name))
                    {
                        options.Add(voice.name);
                    }
                }
            }
            voiceOptions = options.ToArray();
        }

        public static void SelectIndex(Dropdown dropdown, int val)
        {
            if (dropdown &&
                val < dropdown.options.Count)
            {
                dropdown.value = val;
            }
        }

        public static Voice GetVoice(
            VoiceResult voiceResult,
            string display)
        {
            if (null == voiceResult ||
                null == voiceResult.voices)
            {
                Debug.LogError("Voices are not set!");
                return null;
            }

            foreach (Voice voice in voiceResult.voices)
            {
                if (!string.IsNullOrEmpty(voice.display) &&
                    voice.display.Equals(display))
                {
                    return voice;
                }
                else if (!string.IsNullOrEmpty(voice.name) &&
                    voice.name.Equals(display))
                {
                    return voice;
                }
            }
            return null;
        }

        public static void HandleVoiceChangedDropdown(Dropdown dropdown,
            VoiceResult voiceResult,
            SpeechSynthesisUtterance utterance,
            ISpeechSynthesisPlugin plugin)
        {
            if (null == dropdown)
            {
                Debug.LogError("The dropdown for voices is not set!");
                return;
            }

            if (null == voiceResult)
            {
                Debug.LogError("The voice result is not set!");
                return;
            }

            if (null == utterance)
            {
                Debug.LogError("The utterance is not set!");
                return;
            }

            if (null == plugin)
            {
                Debug.LogError("The plugin is not set!");
                return;
            }

            string display = dropdown.options[dropdown.value].text;
            //Debug.Log(display);

            Voice voice = null;
            if (dropdown.value > 0)
            {
                SetDefaultVoice(display);

                voice = SpeechSynthesisUtils.GetVoice(voiceResult, display);
                if (null == voice)
                {
                    Debug.LogError("Did not find specified voice!");
                }
            }

            if (null == voice)
            {
                return;
            }

            //Debug.Log(voice.lang);
            plugin.SetVoice(utterance, voice);
        }

        public static void HandleVoiceChanged(string[] voiceOptions,
            int voiceIndex,
            VoiceResult voiceResult,
            SpeechSynthesisUtterance utterance,
            ISpeechSynthesisPlugin plugin)
        {
            if (null == voiceOptions)
            {
                Debug.LogError("Voice options are not set!");
                return;
            }

            if (null == voiceResult)
            {
                Debug.LogError("The voice result is not set!");
                return;
            }

            if (null == utterance)
            {
                Debug.LogError("The utterance is not set!");
                return;
            }

            if (null == plugin)
            {
                Debug.LogError("The plugin is not set!");
                return;
            }

            string display = voiceOptions[voiceIndex];
            //Debug.Log(display);

            Voice voice = null;
            if (voiceIndex > 0)
            {
                SetDefaultVoice(display);

                voice = SpeechSynthesisUtils.GetVoice(voiceResult, display);
                if (null == voice)
                {
                    Debug.LogError("Did not find specified voice!");
                }
            }

            if (null == voice)
            {
                return;
            }

            //Debug.Log(voice.lang);
            plugin.SetVoice(utterance, voice);
        }

        public static string GetDefaultVoice()
        {
            if (!PlayerPrefs.HasKey(KEY_WEBGL_SPEECH_SYNTHESIS_PLUGIN_VOICE))
            {
                return string.Empty; //no default set yet
            }
            string language = PlayerPrefs.GetString(KEY_WEBGL_SPEECH_SYNTHESIS_PLUGIN_VOICE);
            if (string.IsNullOrEmpty(language))
            {
                return string.Empty; //no default set yet
            }
            //Debug.Log(string.Format("GetDefaultVoice: {0}", language));
            return language;
        }

        public static void SetDefaultVoice(string voice)
        {
            if (_sPausePreferences)
            {
                return;
            }
            //Debug.Log(string.Format("SetDefaultVoice: {0}", voice));
            PlayerPrefs.SetString(KEY_WEBGL_SPEECH_SYNTHESIS_PLUGIN_VOICE, voice);
        }

        public static void RestoreVoice(Dropdown dropdownVoices)
        {
            try
            {
                // pause setting player prefs when changing values
                PausePlayerPrefs(true);

                if (null == dropdownVoices)
                {
                    Debug.LogError("Dropdown Voices not set!");
                    return;
                }
                string display = GetDefaultVoice();
                if (string.IsNullOrEmpty(display))
                {
                    return; //no default set yet
                }

                for (int index = 0; index < dropdownVoices.options.Count; ++index)
                {
                    Dropdown.OptionData option = dropdownVoices.options[index];
                    if (option.text == display)
                    {
                        dropdownVoices.value = index;
                        return;
                    }
                }
            }
            finally
            {
                // back to normal
                PausePlayerPrefs(false);
            }
        }

        public static void RestoreVoice(string[] voiceOptions, out int voiceIndex)
        {
            try
            {
                // pause setting player prefs when changing values
                PausePlayerPrefs(true);

                voiceIndex = 0;

                if (null == voiceOptions)
                {
                    Debug.LogError("Voices options are not set!");
                    return;
                }
                string display = GetDefaultVoice();
                if (string.IsNullOrEmpty(display))
                {
                    return; //no default set yet
                }

                for (int index = 0; index < voiceOptions.Length; ++index)
                {
                    string option = voiceOptions[index];
                    if (option == display)
                    {
                        voiceIndex = index;
                        return;
                    }
                }
            }
            finally
            {
                // back to normal
                PausePlayerPrefs(false);
            }
        }
    }
}
