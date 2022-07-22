#if UNITY_2018_1_OR_NEWER
using UnityEngine.Networking;
#endif

namespace UnityWebGLSpeechDetection
{
    public interface IWWW
    {
        bool IsDone();

        string GetError();

        string GetText();

        void Dispose();

#if UNITY_2018_1_OR_NEWER
        UnityWebRequestAsyncOperation SendWebRequest();
#endif
    }
}
