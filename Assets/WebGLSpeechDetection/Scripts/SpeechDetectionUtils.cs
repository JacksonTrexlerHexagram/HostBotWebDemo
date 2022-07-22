using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityWebGLSpeechDetection
{
    public class SpeechDetectionUtils
    {
        public const string KEY_WEBGL_SPEECH_DETECTION_PLUGIN_LANGUAGE = "WEBGL_SPEECH_DETECTION_PLUGIN_LANGUAGE";
        public const string KEY_WEBGL_SPEECH_DETECTION_PLUGIN_DIALECT = "WEBGL_SPEECH_DETECTION_PLUGIN_DIALECT";

        public static ISpeechDetectionPlugin GetInstance()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return WebGLSpeechDetectionPlugin.GetInstance();
#else
            return ProxySpeechDetectionPlugin.GetInstance();
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

        public static void SelectIndex(Dropdown dropdown, int val)
        {
            if (dropdown &&
                val < dropdown.options.Count)
            {
                dropdown.value = val;
            }
        }

        public static List<string> GetDefaultLanguageOptions()
        {
            List<string> options = new List<string>();
            options.Add("Languages");
            return options;
        }

        public static List<string> GetDefaultDialectOptions()
        {
            List<string> options = new List<string>();
            options.Add("Dialects");
            return options;
        }

        public static Language GetLanguage(LanguageResult languageResult,
            string display)
        {
            if (null == languageResult ||
                null == languageResult.languages)
            {
                Debug.LogError("Languages are not set!");
                return null;
            }

            foreach (Language language in languageResult.languages)
            {
                if (!string.IsNullOrEmpty(language.display) &&
                    language.display.Equals(display))
                {
                    return language;
                }
                else if (!string.IsNullOrEmpty(language.name) &&
                    language.name.Equals(display))
                {
                    return language;
                }
            }
            return null;
        }

        public static string GetLanguageDisplay(LanguageResult languageResult,
            string name)
        {
            if (null == languageResult ||
                null == languageResult.languages)
            {
                Debug.LogError("Languages are not set!");
                return null;
            }

            foreach (Language language in languageResult.languages)
            {
                if (string.IsNullOrEmpty(language.name))
                {
                    continue;
                }
                else if (language.name == name)
                {
                    if (!string.IsNullOrEmpty(language.display))
                    {
                        return language.display;
                    }
                    else if (!string.IsNullOrEmpty(language.name))
                    {
                        return language.name;
                    }
                }
            }
            return null;
        }

        public static Dialect GetDialect(LanguageResult languageResult, 
            string display)
        {
            if (null == languageResult ||
                null == languageResult.languages)
            {
                Debug.LogError("Languages are not set!");
                return null;
            }

            foreach (Language language in languageResult.languages)
            {
                if (null == language.dialects)
                {
                    continue;
                }
                foreach (Dialect dialect in language.dialects)
                {
                    if (!string.IsNullOrEmpty(dialect.display) &&
                        dialect.display.Equals(display))
                    {
                        return dialect;
                    }
                    else if (!string.IsNullOrEmpty(dialect.description) &&
                        dialect.description.Equals(display))
                    {
                        return dialect;
                    }
                    else if (!string.IsNullOrEmpty(dialect.name) &&
                        dialect.name.Equals(display))
                    {
                        return dialect;
                    }
                }
            }
            return null;
        }

        public static string GetDialectDisplay(LanguageResult languageResult,
            string name)
        {
            if (null == languageResult ||
                null == languageResult.languages)
            {
                Debug.LogError("Languages are not set!");
                return null;
            }

            foreach (Language language in languageResult.languages)
            {
                if (null == language.dialects)
                {
                    continue;
                }
                foreach (Dialect dialect in language.dialects)
                {
                    if (dialect.name == name)
                    {
                        if (!string.IsNullOrEmpty(dialect.display))
                        {
                            return dialect.display;
                        }
                        else if (!string.IsNullOrEmpty(dialect.description))
                        {
                            return dialect.description;
                        }
                        else if (!string.IsNullOrEmpty(dialect.name))
                        {
                            return dialect.name;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Disable the dialects UI when not available
        /// </summary>
        public static void DisableDialects(Dropdown dropdown)
        {
            SpeechDetectionUtils.PopulateDropdown(dropdown, SpeechDetectionUtils.GetDefaultDialectOptions());
            SpeechDetectionUtils.SetInteractable(false, dropdown);
        }

        public static void PopulateLanguages(out string[] languageOptions,
            out int languageIndex,
            LanguageResult languageResult)
        {
            List<string> options = GetDefaultLanguageOptions();
            languageIndex = 0;
            languageOptions = options.ToArray();

            // prepare the language drop down items
            if (null == languageResult)
            {
                Debug.LogError("Language result is not set!");
                return;
            }

            if (null != languageResult.languages)
            {
                foreach (Language language in languageResult.languages)
                {
                    if (!string.IsNullOrEmpty(language.display))
                    {
                        options.Add(language.display);
                    }
                    else if (!string.IsNullOrEmpty(language.name))
                    {
                        options.Add(language.name);
                    }
                }
            }

            languageOptions = options.ToArray();
        }

        public static void PopulateLanguagesDropdown(Dropdown dropdownLanguages, 
            LanguageResult languageResult)
        {
            if (null == dropdownLanguages)
            {
                Debug.LogError("The dropdown for languages is not set!");
                return;
            }

            if (null == languageResult)
            {
                Debug.LogError("The language result is not set!");
                return;
            }

            // prepare the language drop down items
            List<string> options = GetDefaultLanguageOptions();
            if (null != languageResult)
            {
                if (null != languageResult.languages)
                {
                    foreach (Language language in languageResult.languages)
                    {
                        if (!string.IsNullOrEmpty(language.display))
                        {
                            options.Add(language.display);
                        }
                        else if (!string.IsNullOrEmpty(language.name))
                        {
                            options.Add(language.name);
                        }
                    }
                }
            }
            SpeechDetectionUtils.PopulateDropdown(dropdownLanguages, options);
        }

        /// <summary>
        /// Handler for changing languages
        /// </summary>
        public static void HandleLanguageChanged(Dropdown dropdownLanguages,
            Dropdown dropdownDialects,
            LanguageResult languageResult,
            ISpeechDetectionPlugin plugin)
        {
            if (null == dropdownLanguages)
            {
                Debug.LogError("The dropdown for languages is not set!");
                return;
            }

            if (null == dropdownDialects)
            {
                Debug.LogError("The dropdown for dialects is not set!");
                return;
            }

            if (null == languageResult)
            {
                Debug.LogError("The language result is not set!");
                return;
            }

            string display = dropdownLanguages.options[dropdownLanguages.value].text;
            //Debug.Log(display);
            SetDefaultLanguage(display);

            Language language = null;
            if (dropdownLanguages.value > 0)
            {
                language = SpeechDetectionUtils.GetLanguage(languageResult, display);
                if (null == language)
                {
                    Debug.LogError("Did not find specified language!");
                }
                else
                {
                    LanguageChangedResult languageChangedResult = new LanguageChangedResult();
                    languageChangedResult._mLanguage = language.name;
                    plugin.Invoke(languageChangedResult);
                }
            }

            List<string> options = SpeechDetectionUtils.GetDefaultDialectOptions();
            if (dropdownLanguages.value > 0 &&
                null != language &&
                null != language.dialects &&
                language.dialects.Length > 0)
            {
                foreach (Dialect dialect in language.dialects)
                {
                    if (!string.IsNullOrEmpty(dialect.display))
                    {
                        options.Add(dialect.display);
                    }
                    else if (!string.IsNullOrEmpty(dialect.description))
                    {
                        options.Add(dialect.description);
                    }
                    else if (!string.IsNullOrEmpty(dialect.name))
                    {
                        options.Add(dialect.name);
                    }
                }
            }
            SpeechDetectionUtils.PopulateDropdown(dropdownDialects, options);
            SpeechDetectionUtils.SetInteractable(options.Count > 1, dropdownDialects);
            SpeechDetectionUtils.SelectIndex(dropdownDialects, 1);

            HandleDialectChanged(dropdownDialects, languageResult, plugin);
        }

        /// <summary>
        /// Handler for changing languages
        /// </summary>
        public static void HandleLanguageChanged(string[] languageOptions,
            int languageIndex,
            out string[] dialectOptions,
            ref int dialectIndex,
            LanguageResult languageResult,
            ISpeechDetectionPlugin plugin)
        {
            List<string> options = GetDefaultDialectOptions();
            dialectOptions = options.ToArray();
            dialectIndex = 0;

            if (null == languageOptions)
            {
                Debug.LogError("The language options are not set!");
                return;
            }

            if (null == dialectOptions)
            {
                Debug.LogError("The dialect options are not set!");
                return;
            }

            if (null == languageResult)
            {
                Debug.LogError("The language result is not set!");
                return;
            }

            string display = languageOptions[languageIndex];
            //Debug.Log(display);
            SetDefaultLanguage(display);

            Language language = null;
            if (languageIndex > 0)
            {
                language = SpeechDetectionUtils.GetLanguage(languageResult, display);
                if (null == language)
                {
                    Debug.LogError("Did not find specified language!");
                }
                else
                {
                    LanguageChangedResult languageChangedResult = new LanguageChangedResult();
                    languageChangedResult._mLanguage = language.name;
                    plugin.Invoke(languageChangedResult);
                }
            }

            if (languageIndex > 0 &&
                null != language &&
                null != language.dialects &&
                language.dialects.Length > 0)
            {
                foreach (Dialect dialect in language.dialects)
                {
                    if (!string.IsNullOrEmpty(dialect.display))
                    {
                        options.Add(dialect.display);
                    }
                    else if (!string.IsNullOrEmpty(dialect.description))
                    {
                        options.Add(dialect.description);
                    }
                    else if (!string.IsNullOrEmpty(dialect.name))
                    {
                        options.Add(dialect.name);
                    }
                }
            }
            dialectOptions = options.ToArray();
            if (dialectOptions.Length > 1)
            {
                dialectIndex = 1;
            }

            HandleDialectChanged(dialectOptions, dialectIndex, languageResult, plugin);
        }

        /// <summary>
        /// Handler for changing dialects
        /// </summary>
        public static void HandleDialectChanged(Dropdown dropdownDialects, 
            LanguageResult languageResult,
            ISpeechDetectionPlugin plugin)
        {
            if (null == dropdownDialects)
            {
                Debug.LogError("The dropdown for dialects is not set!");
                return;
            }

            if (null == languageResult)
            {
                Debug.LogError("The language result is not set!");
                return;
            }

            string display = dropdownDialects.options[dropdownDialects.value].text;

            Dialect dialect = null;
            if (dropdownDialects.value > 0)
            {
                //Debug.Log(display);
                SetDefaultDialect(display);

                dialect = SpeechDetectionUtils.GetDialect(languageResult, display);
                if (null == dialect)
                {
                    Debug.LogError("Did not find specified dialect!");
                    return;
                }

                //Debug.Log(dialect.name);
                if (null != plugin)
                {
                    plugin.Abort();
                    plugin.SetLanguage(dialect.name);

                    LanguageChangedResult languageChangedResult = new LanguageChangedResult();
                    languageChangedResult._mDialect = dialect.name;
                    plugin.Invoke(languageChangedResult);
                }
            }
        }

        /// <summary>
        /// Handler for changing dialects
        /// </summary>
        public static void HandleDialectChanged(string[] dialectOptions,
            int dialectIndex,
            LanguageResult languageResult,
            ISpeechDetectionPlugin plugin)
        {
            if (null == dialectOptions)
            {
                Debug.LogError("The dialect options are not set!");
                return;
            }

            if (null == languageResult)
            {
                Debug.LogError("The language result is not set!");
                return;
            }

            string display = dialectOptions[dialectIndex];

            Dialect dialect = null;
            if (dialectIndex > 0)
            {
                //Debug.Log(display);
                SetDefaultDialect(display);

                dialect = SpeechDetectionUtils.GetDialect(languageResult, display);
                if (null == dialect)
                {
                    Debug.LogError("Did not find specified dialect!");
                    return;
                }

                //Debug.Log(dialect.name);
                if (null != plugin)
                {
                    plugin.Abort();
                    plugin.SetLanguage(dialect.name);

                    LanguageChangedResult languageChangedResult = new LanguageChangedResult();
                    languageChangedResult._mDialect = dialect.name;
                    plugin.Invoke(languageChangedResult);
                }
            }
        }

        public static string GetDefaultLanguage()
        {
            if (!PlayerPrefs.HasKey(KEY_WEBGL_SPEECH_DETECTION_PLUGIN_LANGUAGE))
            {
                return string.Empty; //no default set yet
            }
            string language = PlayerPrefs.GetString(KEY_WEBGL_SPEECH_DETECTION_PLUGIN_LANGUAGE);
            if (string.IsNullOrEmpty(language))
            {
                return string.Empty; //no default set yet
            }
            //Debug.Log(string.Format("GetDefaultLanguage: {0}", language));
            return language;
        }

        public static void SetDefaultLanguage(string language)
        {
            if (_sPausePreferences)
            {
                return;
            }
            //Debug.Log(string.Format("SetDefaultLanguage: {0}", language));
            PlayerPrefs.SetString(KEY_WEBGL_SPEECH_DETECTION_PLUGIN_LANGUAGE, language);
        }

        public static string GetDefaultDialect()
        {
            if (!PlayerPrefs.HasKey(KEY_WEBGL_SPEECH_DETECTION_PLUGIN_DIALECT))
            {
                return string.Empty; //no default set yet
            }
            string dialect = PlayerPrefs.GetString(KEY_WEBGL_SPEECH_DETECTION_PLUGIN_DIALECT);
            if (string.IsNullOrEmpty(dialect))
            {
                return string.Empty; //no default set yet
            }
            //Debug.Log(string.Format("GetDefaultDialect: {0}", dialect));
            return dialect;
        }

        public static void SetDefaultDialect(string dialect)
        {
            if (_sPausePreferences)
            {
                return;
            }
            //Debug.Log(string.Format("SetDefaultDialect: {0}", dialect));
            PlayerPrefs.SetString(KEY_WEBGL_SPEECH_DETECTION_PLUGIN_DIALECT, dialect);
        }

        public static void RestoreLanguage(Dropdown dropdownLanguages)
        {
            try
            {
                // pause setting player prefs when changing values
                PausePlayerPrefs(true);

                if (null == dropdownLanguages)
                {
                    Debug.LogError("Dropdown Languages not set!");
                    return;
                }

                string display = GetDefaultLanguage();
                if (string.IsNullOrEmpty(display))
                {
                    return; //no default set yet
                }

                for (int index = 0; index < dropdownLanguages.options.Count; ++index)
                {
                    Dropdown.OptionData option = dropdownLanguages.options[index];
                    if (option.text == display)
                    {
                        dropdownLanguages.value = index;
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

        public static void RestoreLanguage(string[] languageOptions, out int languageIndex)
        {
            try
            {
                // pause setting player prefs when changing values
                PausePlayerPrefs(true);

                languageIndex = 0;

                if (null == languageOptions)
                {
                    Debug.LogError("Languages options are not set!");
                    return;
                }

                string display = GetDefaultLanguage();
                if (string.IsNullOrEmpty(display))
                {
                    return; //no default set yet
                }

                for (int index = 0; index < languageOptions.Length; ++index)
                {
                    string option = languageOptions[index];
                    if (option == display)
                    {
                        languageIndex = index;
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

        public static void RestoreDialect(Dropdown dropdownDialects)
        {
            try
            {
                // pause setting player prefs when changing values
                PausePlayerPrefs(true);

                if (null == dropdownDialects)
                {
                    Debug.LogError("Dropdown Dialects not set!");
                    return;
                }
                string display = GetDefaultDialect();
                if (string.IsNullOrEmpty(display))
                {
                    return; //no default set yet
                }

                for (int index = 0; index < dropdownDialects.options.Count; ++index)
                {
                    Dropdown.OptionData option = dropdownDialects.options[index];
                    if (option.text == display)
                    {
                        dropdownDialects.value = index;
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

        public static void RestoreDialect(string[] dialectOptions, out int dialectIndex)
        {
            try
            {
                // pause setting player prefs when changing values
                PausePlayerPrefs(true);

                dialectIndex = 0;

                if (null == dialectOptions)
                {
                    Debug.LogError("Dialects options are not set!");
                    return;
                }
                string display = GetDefaultDialect();
                if (string.IsNullOrEmpty(display))
                {
                    return; //no default set yet
                }

                for (int index = 0; index < dialectOptions.Length; ++index)
                {
                    string option = dialectOptions[index];
                    if (option == display)
                    {
                        dialectIndex = index;
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

        public static bool HandleOnLanguageChanged(
            ref int languageIndex,
            string[] languageOptions,
            ref int dialectIndex,
            string[] dialectOptions,
            LanguageResult languageResult,
            ISpeechDetectionPlugin plugin,
            LanguageChangedResult languageChangedResult)
        {
            if (_sPausePreferences)
            {
                return false;
            }

            if (null == languageResult)
            {
                Debug.LogError("LanguageResult is null!");
                return false;
            }

            if (null == languageChangedResult)
            {
                Debug.LogError("LanguageChangedResult is null!");
                return false;
            }

            bool changed = false;

            /*
            Debug.LogFormat("GetLanguageDisplay: language={0} dialect={1}",
                (languageChangedResult._mLanguage == null) ? "null" : languageChangedResult._mLanguage,
                (languageChangedResult._mDialect == null) ? "null" : languageChangedResult._mDialect);
            */

            if (null != languageChangedResult._mLanguage)
            {
                string language = GetLanguageDisplay(languageResult, languageChangedResult._mLanguage);
                for (int index = 0; index < languageOptions.Length; ++index)
                {
                    if (languageOptions[index] == language)
                    {
                        if (languageIndex != index)
                        {
                            languageIndex = index;
                            changed = true;
                            break;
                        }
                    }
                }
            }

            if (null != languageChangedResult._mDialect)
            {
                string dialect = GetDialectDisplay(languageResult, languageChangedResult._mDialect);
                for (int index = 0; index < dialectOptions.Length; ++index)
                {
                    if (dialectOptions[index] == dialect)
                    {
                        if (dialectIndex != index)
                        {
                            dialectIndex = index;
                            changed = true;
                        }
                    }
                }
            }

            return changed;
        }
    }
}
