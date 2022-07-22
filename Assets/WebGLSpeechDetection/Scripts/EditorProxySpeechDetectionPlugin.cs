using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace UnityWebGLSpeechDetection
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public class EditorProxySpeechDetectionPlugin : ProxySpeechDetectionPlugin
    {
        const string KEY_SPEECH_DETECTION_ENABLED = "ProxySpeechDetectionEnabled";

#if UNITY_EDITOR
        /// <summary>
        /// Singleton reference
        /// </summary>
        private static EditorProxySpeechDetectionPlugin _sInstance = null;
#endif

        /// <summary>
        /// Keep track of initialization
        /// </summary>
        private bool _mHasEditorUpdates = false;

        /// <summary>
        /// Use for co-routine simulation
        /// </summary>
        private List<IEnumerator> _mPendingRoutines = new List<IEnumerator>();

        protected override void SafeStartCoroutine(string routineName, IEnumerator routine)
        {
            //Debug.LogFormat("EditorProxySpeechDetectionPlugin: SafeStartCoroutine: {0}", routineName);
            _mPendingRoutines.Add(routine);
        }

        protected override IWWW CreateWWW(string url)
        {
            return new WWWEditMode(url);
        }

        public static bool IsEnabled()
        {
#if UNITY_EDITOR
            if (!EditorPrefs.HasKey(KEY_SPEECH_DETECTION_ENABLED))
            {
                return false;
            }

            return (1 == EditorPrefs.GetInt(KEY_SPEECH_DETECTION_ENABLED));
#else
            return false;
#endif
        }

        private void CleanUp()
        {
#if UNITY_EDITOR
            //Debug.Log("CleanUp:");
            GameObject go = GameObject.Find("EditorProxySpeechDetectionPlugin");
            if (go)
            {
                DestroyImmediate(go);
                _sInstance = null;
            }
#endif
        }

        public static void SetEnabled(bool toggle)
        {
#if UNITY_EDITOR
            if (toggle)
            {
                EditorPrefs.SetInt(KEY_SPEECH_DETECTION_ENABLED, 1);
            }
            else
            {
                EditorPrefs.SetInt(KEY_SPEECH_DETECTION_ENABLED, 0);
                if (null != _sInstance)
                {
                    _sInstance.StopEditorUpdates();
                    _sInstance.CleanUp();
                }
            }
#endif
        }

        /// <summary>
        /// Get singleton instance
        /// </summary>
        public new static EditorProxySpeechDetectionPlugin GetInstance()
        {
#if UNITY_EDITOR
            // stop editor updates, if compiling or not enabled
            if (EditorApplication.isCompiling ||
                !IsEnabled())
            {
                //Debug.Log("EditorProxySpeechDetectionPlugin:  GetInstance: cleaning...");
                if (null != _sInstance)
                {
                    _sInstance.StopEditorUpdates();
                    _sInstance.CleanUp();
                    _sInstance = null;
                }
                return null;
            }
            if (null == _sInstance)
            {
                GameObject go = GameObject.Find("EditorProxySpeechDetectionPlugin");
                if (null == go)
                {
                    go = new GameObject("EditorProxySpeechDetectionPlugin");
                    _sInstance = go.AddComponent<EditorProxySpeechDetectionPlugin>();
                }
                else
                {
                    _sInstance = go.GetComponent<EditorProxySpeechDetectionPlugin>();
                }
            }
            if (null != _sInstance)
            {
                _sInstance.StartEditorUpdates();
            }
            return _sInstance;
#else
            return null;
#endif
        }

#if UNITY_EDITOR

        public void EditorUpdate()
        {
            //Debug.LogFormat("EditorUpdate: {0}", _mPendingRoutines.Count);

            //Debug.LogFormat("OnGUI: IsCompiling: {0}", EditorApplication.isCompiling);
            if (!IsEnabled() ||
                EditorApplication.isCompiling)
            {
                StopEditorUpdates();
                CleanUp();
            }

            int i = 0;
            while (i < _mPendingRoutines.Count)
            {
                //Debug.LogFormat("EditorUpdate: {0}", _mPendingRoutines.Count);
                IEnumerator routine = _mPendingRoutines[i];
                try
                {
                    if (null != routine &&
                        routine.MoveNext())
                    {
                        //more work
                        ++i;
                        continue;
                    }
                }
                catch (Exception)
                {
                    // something bad happened,
                    // stop coroutine
                }
                //done
                _mPendingRoutines.RemoveAt(i);
                continue;
            }
        }

#endif

        /// <summary>
        /// Start editor updates
        /// </summary>
        public void StartEditorUpdates()
        {
            if (!_mHasEditorUpdates)
            {
                _mHasEditorUpdates = true;
                //Debug.Log("StartEditorUpdates:");
                _mPendingRoutines.Clear();
#if UNITY_EDITOR
                EditorApplication.update += EditorUpdate;
#endif
                SafeStartCoroutine("Init", Init());
            }
        }

        //Stop editor updates
        public void StopEditorUpdates()
        {
            if (_mHasEditorUpdates)
            {
                //Debug.Log("StopEditorUpdates:");
                _mHasEditorUpdates = false;
                _mPendingRoutines.Clear();
#if UNITY_EDITOR
                EditorApplication.update -= EditorUpdate;
#endif
            }
        }
    }
}
