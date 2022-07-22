using System;

namespace UnityWebGLSpeechDetection
{
    [Serializable]
    public class Language
    {
        public string name = string.Empty;
        public string display = string.Empty;
        public Dialect[] dialects;
    }
}
