using System;

namespace UnityWebGLSpeechDetection
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class SpeechDetectionAttribute : Attribute
    {
        private string _mSpokenPhrase = null;

        public SpeechDetectionAttribute(string spokenPhrase)
        {
            _mSpokenPhrase = spokenPhrase;
        }

        public string GetSpokenPhrase()
        {
            return _mSpokenPhrase;
        }
    }
}
