using System;

namespace UnityWebGLSpeechDetection
{
    [Serializable]
    public class SpeechRecognitionResult
    {
        public bool isFinal = false;
        public int length = 0;
        public SpeechRecognitionAlternative[] alternatives;
    }
}
