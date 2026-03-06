using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine;

namespace SandboxPlatform.SAMANDA
{
    public class WebRequester
    {
        public delegate void SuccessCallback(JToken body);
        public delegate void FailCallback();

        protected static WebRequester _instance = null;

        public static WebRequester Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WebRequester();
                }
                return _instance;
            }
        }

        public void SendPost(string uri,
            WWWForm param,
            SuccessCallback onSuccess = null,
            FailCallback onFail = null)
        {
            if (SAMANDA.Instance == false)
            {
                Debug.LogError("[Error] SAMANDA is Missing..");
            }

            SAMANDA.Instance.StartCoroutine(SendPostCorutine(uri, param, onSuccess, onFail));
        }

        public IEnumerator SendPostCorutine(string uri,
            WWWForm param,
            SuccessCallback onSuccess,
            FailCallback onFail)
        {
            using (UnityWebRequest req = UnityWebRequest.Post(uri, param))
            {
                req.timeout = 10;
                yield return req.SendWebRequest();

                if (!req.isNetworkError && !req.isHttpError)
                {
                    if (onSuccess != null)
                    {
                        try
                        {
                            string body = req.downloadHandler.text;
                            onSuccess(JToken.Parse(body));
                        }
                        catch
                        {
                            Debug.LogError("====== Response Error : " + uri + req.downloadHandler.text);
                        };
                    }
                }
                else
                {
                    onFail?.Invoke();
                }
            }
        }
    }
}