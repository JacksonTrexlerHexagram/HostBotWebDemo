using System;
using System.IO;
using System.Net;
#if UNITY_2018_1_OR_NEWER
using UnityEngine.Networking;
#endif

namespace UnityWebGLSpeechDetection
{
    public class WWWEditMode : IWWW
    {
        private string _mError = null;
        private bool _mIsDone = false;
        private string _mText = null;

        public WWWEditMode(string url)
        {
            //Debug.LogFormat("WWWEditMode: url={0}", url);
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Timeout = 3000;
            // Start the asynchronous request.
            request.BeginGetResponse(new AsyncCallback(HandleBeginGetResponse), request);
        }

        private void HandleBeginGetResponse(IAsyncResult asyncResult)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)asyncResult.AsyncState;

                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asyncResult);

                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        _mText = sr.ReadToEnd();
                        _mIsDone = true;
                        //Debug.LogFormat("HandleBeginGetResponse: text={0}", _mText);
                    }
                }

                response.Close();
            }
            catch (WebException e)
            {
                _mError = string.Format("Request exception={0}", e);
                //Debug.LogErrorFormat("HandleBeginGetResponse: error={0}", _mError);
            }
        }

        public bool IsDone()
        {
            return _mIsDone;
        }

        public string GetError()
        {
            return _mError;
        }

        public string GetText()
        {
            return _mText;
        }

        public void Dispose()
        {
        }

#if UNITY_2018_1_OR_NEWER
        public UnityWebRequestAsyncOperation SendWebRequest()
        {
            return null;
        }
#endif
    }
}
