using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityWebGLSpeechSynthesis
{
    public class BaseSpeechSynthesisPlugin : MonoBehaviour
    {
        /// <summary>
        /// Synthesis event handlers need to provide this delegate
        /// </summary>
        /// <param name="speechSynthesisEvent"></param>
        public delegate void DelegateHandleSynthesisOnEnd(SpeechSynthesisEvent speechSynthesisEvent);

        /// <summary>
        /// The event handler that invokes when events are detected
        /// </summary>
        protected static List<DelegateHandleSynthesisOnEnd> _sOnSynthesisOnEnd = new List<DelegateHandleSynthesisOnEnd>();

        protected class IdEventArgs : EventArgs
        {
            public string _mId = null;
        }

        protected class SpeechSynthesisUtteranceEventArgs : IdEventArgs
        {
            public SpeechSynthesisUtterance _mUtterance = null;
        }

        protected EventHandler<SpeechSynthesisUtteranceEventArgs> _mOnCreateSpeechSynthesisUtterance;

        protected class VoiceResultArgs : IdEventArgs
        {
            public VoiceResult _mVoiceResult = null;
        }

        protected EventHandler<VoiceResultArgs> _mOnGetVoices;

        public void AddListenerSynthesisOnEnd(DelegateHandleSynthesisOnEnd listener)
        {
            if (!_sOnSynthesisOnEnd.Contains(listener))
            {
                //Debug.Log("AddListenerSynthesisOnEnd:");
                _sOnSynthesisOnEnd.Add(listener);
            }
        }

        public void RemoveListenerSynthesisOnEnd(DelegateHandleSynthesisOnEnd listener)
        {
            if (_sOnSynthesisOnEnd.Contains(listener))
            {
                //Debug.LogFormat("RemoveListenerOnDetectionResult: {0}", listener.Method);
                _sOnSynthesisOnEnd.Remove(listener);
            }
        }
    }
}
