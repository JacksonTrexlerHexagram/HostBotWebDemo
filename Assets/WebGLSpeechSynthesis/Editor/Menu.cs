using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UnityWebGLSpeechSynthesis
{
    public class Menu : MonoBehaviour
    {
        [MenuItem("GameObject/WebGLSpeechSynthesis/Create WebGLSpeechSynthesisPlugin")]
        private static void CreateWebGLSpeechSynthesisPlugin()
        {
            GameObject gameObject = new GameObject("WebGLSpeechSynthesisPlugin");
            gameObject.AddComponent<WebGLSpeechSynthesisPlugin>();
        }

        [MenuItem("GameObject/WebGLSpeechSynthesis/Create ProxySpeechSynthesisPlugin")]
        private static void CreateProxySpeechDetectionPlugin()
        {
            GameObject gameObject = new GameObject("ProxySpeechSynthesisPlugin");
            gameObject.AddComponent<ProxySpeechSynthesisPlugin>();
        }

        [MenuItem("GameObject/WebGLSpeechSynthesis/Create Voices Dropdown")]
        private static void CreateVoicesDropdown()
        {
            EditorApplication.ExecuteMenuItem("GameObject/UI/Dropdown");
            GameObject selection = Selection.activeGameObject;
            if (selection)
            {
                Dropdown dropdown = selection.GetComponent<Dropdown>();
                if (dropdown)
                {
                    selection.name = "DropdownVoices";
                    List<string> options = new List<string>();
                    options.Add("Voices");
                    SpeechSynthesisUtils.PopulateDropdown(dropdown, options);
                    RectTransform rectTransform = dropdown.GetComponent<RectTransform>();
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.localPosition = new Vector3(-201.5f, 151, 0);
                    rectTransform.sizeDelta = new Vector2(432, 50);
                }
            }
        }

        [MenuItem("GameObject/WebGLSpeechSynthesis/Online Documentation")]
        private static void OnlineDocumentation()
        {
            Application.OpenURL("https://github.com/tgraupmann/UnityWebGLSpeechSynthesis");
        }
    }
}
