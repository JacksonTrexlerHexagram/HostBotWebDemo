using System;

namespace UnityWebGLSpeechDetection
{
    [Serializable]
    public class SpeechRecognitionAlternative
    {
        public float confidence = 0f;
        public string transcript = string.Empty;
    }
}
