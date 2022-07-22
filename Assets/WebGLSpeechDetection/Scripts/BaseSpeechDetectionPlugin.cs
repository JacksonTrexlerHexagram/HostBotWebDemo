using System.Collections.Generic;
using UnityEngine;

namespace UnityWebGLSpeechDetection
{
    public class BaseSpeechDetectionPlugin : MonoBehaviour
    {
        /// <summary>
        /// Detection result handlers need to provide this delegate
        /// </summary>
        /// <param name="detectionResult"></param>
        /// <returns></returns>
        public delegate bool DelegateHandleDetectionResult(DetectionResult detectionResult);

        /// <summary>
        /// Language changed handlers need to provide this delegate
        /// </summary>
        /// <param name="languageChangedResult"></param>
        public delegate void DelegateHandleLanguageChanged(LanguageChangedResult languageChangedResult);

        /// <summary>
        /// The event handler that invokes when words are detected
        /// </summary>
        protected static List<DelegateHandleDetectionResult> _sOnDetectionResults = new List<DelegateHandleDetectionResult>();

        /// <summary>
        /// The event handler that invokes when words are detected
        /// </summary>
        protected static List<DelegateHandleLanguageChanged> _sOnLanguageChanged = new List<DelegateHandleLanguageChanged>();

        public void AddListenerOnDetectionResult(DelegateHandleDetectionResult listener)
        {
            if (!_sOnDetectionResults.Contains(listener))
            {
                //Debug.LogFormat("AddListenerOnDetectionResult: {0}", listener.Method);
                _sOnDetectionResults.Add(listener);
            }
        }

        public void RemoveListenerOnDetectionResult(DelegateHandleDetectionResult listener)
        {
            if (_sOnDetectionResults.Contains(listener))
            {
                //Debug.LogFormat("RemoveListenerOnDetectionResult: {0}", listener.Method);
                _sOnDetectionResults.Remove(listener);
            }
        }

        public void AddListenerOnLanguageChanged(DelegateHandleLanguageChanged listener)
        {
            if (!_sOnLanguageChanged.Contains(listener))
            {
                //Debug.LogFormat("AddListenerOnDetectionResult: {0}", listener.Method);
                _sOnLanguageChanged.Add(listener);
            }
        }

        public void RemoveListenerOnLanguageChanged(DelegateHandleLanguageChanged listener)
        {
            if (_sOnLanguageChanged.Contains(listener))
            {
                //Debug.LogFormat("RemoveListenerOnLanguageChanged: {0}", listener.Method);
                _sOnLanguageChanged.Remove(listener);
            }
        }

        /// <summary>
        /// Invoke all the listeners until event is handled
        /// </summary>
        /// <param name="detectionResult"></param>
        public void Invoke(DetectionResult detectionResult)
        {
            foreach (DelegateHandleDetectionResult listener in _sOnDetectionResults)
            {
                if (listener.Invoke(detectionResult))
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Invoke all the listeners
        /// </summary>
        /// <param name="detectionResult"></param>
        public void Invoke(LanguageChangedResult languageChangedResult)
        {
            foreach (DelegateHandleLanguageChanged listener in _sOnLanguageChanged)
            {
                listener.Invoke(languageChangedResult);
            }
        }
    }
}
