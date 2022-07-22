using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace UnityWebGLSpeechDetection
{
    public class Menu : MonoBehaviour
    {
        [MenuItem("GameObject/WebGLSpeechDetection/Create WebGLSpeechDetectionPlugin")]
        private static void CreateWebGLSpeechDetectionPlugin()
        {
            GameObject gameObject = new GameObject("WebGLSpeechDetectionPlugin");
            gameObject.AddComponent<WebGLSpeechDetectionPlugin>();
        }

        [MenuItem("GameObject/WebGLSpeechDetection/Create ProxySpeechDetectionPlugin")]
        private static void CreateProxySpeechDetectionPlugin()
        {
            GameObject gameObject = new GameObject("ProxySpeechDetectionPlugin");
            gameObject.AddComponent<ProxySpeechDetectionPlugin>();
        }

        [MenuItem("GameObject/WebGLSpeechDetection/Create EditorProxySpeechDetectionPlugin")]
        private static void CreateEditorProxySpeechDetectionPlugin()
        {
            GameObject gameObject = new GameObject("EditorProxySpeechDetectionPlugin");
            gameObject.AddComponent<EditorProxySpeechDetectionPlugin>();
        }

        [MenuItem("GameObject/WebGLSpeechDetection/Create Languages Dropdown")]
        private static void CreateLanguagesDropdown()
        {
            EditorApplication.ExecuteMenuItem("GameObject/UI/Dropdown");
            GameObject selection = Selection.activeGameObject;
            if (selection)
            {
                Dropdown dropdown = selection.GetComponent<Dropdown>();
                if (dropdown)
                {
                    selection.name = "DropdownLanguages";
                    List<string> options = new List<string>();
                    options.Add("Language");
                    SpeechDetectionUtils.PopulateDropdown(dropdown, options);
                    RectTransform rectTransform = dropdown.GetComponent<RectTransform>();
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.localPosition = new Vector3(-265, 231, 0);
                    rectTransform.sizeDelta = new Vector2(267, 44);
                }
            }
        }

        [MenuItem("GameObject/WebGLSpeechDetection/Create Dialects Dropdown")]
        private static void CreateDialectsDropdown()
        {
            EditorApplication.ExecuteMenuItem("GameObject/UI/Dropdown");
            GameObject selection = Selection.activeGameObject;
            if (selection)
            {
                Dropdown dropdown = selection.GetComponent<Dropdown>();
                if (dropdown)
                {
                    selection.name = "DropdownDialects";
                    List<string> options = new List<string>();
                    options.Add("Dialect");
                    SpeechDetectionUtils.PopulateDropdown(dropdown, options);
                    RectTransform rectTransform = dropdown.GetComponent<RectTransform>();
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.localPosition = new Vector3(272, 231, 0);
                    rectTransform.sizeDelta = new Vector2(267, 44);
                }
            }
        }

        [MenuItem("GameObject/WebGLSpeechDetection/Online Documentation")]
        private static void OnlineDocumentation()
        {
            Application.OpenURL("https://github.com/tgraupmann/UnityWebGLSpeechDetection");
        }

        [MenuItem("Window/WebGLSpeechDetection/Open Example06PanelDictation")]
        private static void OpenExample06PanelDictation()
        {
            Example06PanelDictation panel =
                (Example06PanelDictation)EditorWindow.GetWindow(typeof(Example06PanelDictation),
                false,
                "Example06PanelDictation");
            panel.Show();
        }

        [MenuItem("Window/WebGLSpeechDetection/Open Example07PanelCommands")]
        private static void OpenExample07PanelCommandsPanel()
        {
            Example07PanelCommands panel =
                (Example07PanelCommands)EditorWindow.GetWindow(typeof(Example07PanelCommands),
                false,
                "Example07PanelCommands");
            panel.Show();
        }

        [SpeechDetectionAttribute(spokenPhrase: "new scene")]
        public static void FileNewScene()
        {
            if (EditorUtility.DisplayDialog("Are you sure?", "Have you saved your changes? Your changes will be lost if you don't save them!", "YES", "NO"))
            {
                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
            }
        }

        [SpeechDetectionAttribute(spokenPhrase: "undo")]
        public static void EditUndo()
        {
            Undo.PerformUndo();
        }

        [SpeechDetectionAttribute(spokenPhrase: "redo")]
        public static void EditRedo()
        {
            Undo.PerformRedo();
        }

        [SpeechDetectionAttribute(spokenPhrase:"create cube")]
        public static void CreateCube()
        {
            EditorApplication.ExecuteMenuItem("GameObject/3D Object/Cube");
        }

        [SpeechDetectionAttribute(spokenPhrase: "create sphere")]
        public static void CreateSphere()
        {
            EditorApplication.ExecuteMenuItem("GameObject/3D Object/Sphere");
        }

        [SpeechDetectionAttribute(spokenPhrase: "create capsule")]
        public static void CreateCapsule()
        {
            EditorApplication.ExecuteMenuItem("GameObject/3D Object/Capsule");
        }

        [SpeechDetectionAttribute(spokenPhrase: "create cylinder")]
        public static void CreateCylinder()
        {
            EditorApplication.ExecuteMenuItem("GameObject/3D Object/Cylinder");
        }

        [SpeechDetectionAttribute(spokenPhrase: "create plane")]
        public static void CreatePlane()
        {
            EditorApplication.ExecuteMenuItem("GameObject/3D Object/Plane");
        }

        [SpeechDetectionAttribute(spokenPhrase: "create quad")]
        public static void CreateQuad()
        {
            EditorApplication.ExecuteMenuItem("GameObject/3D Object/Quad");
        }

        [SpeechDetectionAttribute(spokenPhrase: "create point light")]
        public static void CreatePointLight()
        {
            EditorApplication.ExecuteMenuItem("GameObject/Light/Point Light");
        }

        [SpeechDetectionAttribute(spokenPhrase: "delete")]
        public static void EditDelete()
        {
            if (Selection.activeGameObject)
            {
                if (EditorUtility.DisplayDialog("Are you sure?", "Delete this object? This cannot be undone!", "YES", "NO"))
                {
                    DestroyImmediate(Selection.activeGameObject);
                }
            }
        }

        [SpeechDetectionAttribute(spokenPhrase: "duplicate")]
        public static void EditDuplicate()
        {
            GameObject[] gos = Selection.gameObjects;
            if (null != gos &&
                gos.Length > 1)
            {
                foreach (GameObject go in gos)
                {
                    Instantiate(go);
                }
            }
            else if (Selection.activeGameObject)
            {
                Instantiate(Selection.activeGameObject);
            }
        }

        [SpeechDetectionAttribute(spokenPhrase: "focus")]
        public static void EditFrameSelected()
        {
            SceneView.FrameLastActiveSceneView();
        }

        [SpeechDetectionAttribute(spokenPhrase: "lockview")]
        public static void EditLockView()
        {
            SceneView.FrameLastActiveSceneViewWithLock();
        }

        [SpeechDetectionAttribute(spokenPhrase: "hand")]
        public static void Hand()
        {
            Tools.current = Tool.View;
        }

        [SpeechDetectionAttribute(spokenPhrase: "move")]
        public static void Move()
        {
            Tools.current = Tool.Move;
        }

        [SpeechDetectionAttribute(spokenPhrase: "rotate")]
        public static void Rotate()
        {
            Tools.current = Tool.Rotate;
        }

        [SpeechDetectionAttribute(spokenPhrase: "scale")]
        public static void Scale()
        {
            Tools.current = Tool.Scale;
        }

        [SpeechDetectionAttribute(spokenPhrase: "rectangle")]
        public static void Rect()
        {
            Tools.current = Tool.Rect;
        }
    }
}
