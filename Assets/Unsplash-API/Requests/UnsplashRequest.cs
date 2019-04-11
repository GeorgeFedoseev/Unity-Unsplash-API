

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace UnsplashExplorerForUnity {

    public class UnsplashRequest {

        private int WARNING_API_LIMIT_THRESHOLD = 40;

        private TaskCompletionSource<string> _taskCompletionSource;        

        private Coroutine _requestCoroutine;

        protected UnsplashExplorer _driver;

        public UnsplashRequest(UnsplashExplorer driver){
            _driver = driver;
        }

        public Task<string> GetResponseStringAsync(string url){
            _taskCompletionSource = new TaskCompletionSource<string>();                  
            _requestCoroutine = _driver.StartCoroutine(RequestCoroutine(url));

            return _taskCompletionSource.Task;
        }

        public void CancelRequest(){
            if(_requestCoroutine != null){
                _driver.StopCoroutine(_requestCoroutine);
                _requestCoroutine = null;
                _taskCompletionSource.SetCanceled();
            }
        }

        private IEnumerator RequestCoroutine(string url){
            UnityWebRequest www = UnityWebRequest.Get(url);            

            www.SetRequestHeader("Authorization", $"Client-ID {_driver.AccessKey}");           

            yield return www.SendWebRequest();

            var responseHeaders = www.GetResponseHeaders();

            var responseHeadersStr = string.Join("; ", responseHeaders.Select(kv => $"{kv.Key} = {kv.Value}"));
            Debug.Log($"responseHeadersStr: {responseHeadersStr}");

            
            int remainingLimit = -1;
            int totalLimit = -1;
            try {                
                remainingLimit = GetLimitRemainingForThisHour(responseHeaders);
                totalLimit = GetTotalLimitForCurrentHour(responseHeaders);
            }catch{}

            if(www.isNetworkError || www.isHttpError) {
                if(www.isNetworkError){
                    _taskCompletionSource.SetException(new UnsplashRequestException($"Network error: {www.error}"));
                }else{
                    if(remainingLimit == 0){
                        _taskCompletionSource.SetException(new UnsplashRequestLimitExceededException($"Limit of {totalLimit} requests exceeded for current hour."));
                    }else{
                        _taskCompletionSource.SetException(new UnsplashRequestException(www.error));
                    }
                }                
            }
            else {
                var responseString = www.downloadHandler.text;
                _taskCompletionSource.SetResult(responseString);

                if(remainingLimit != -1 && remainingLimit <= WARNING_API_LIMIT_THRESHOLD){
                    Debug.LogWarning($"Unsplash: remaining requests for this hour: {remainingLimit}");
                }
            }

            if(remainingLimit != -1 && totalLimit != -1){
                _driver.OnAPIRequestLimitsReport(new UnsplashAPIRequestLimitsInfo(remainingLimit, totalLimit));
            }
        }

        private int GetTotalLimitForCurrentHour(Dictionary<string, string> responseHeaders){
            int result = -1;
            if(!DidReturnSameResultForThisAPIKey(responseHeaders) && responseHeaders.ContainsKey("X-Ratelimit-Limit")){
                try {
                    result = int.Parse(responseHeaders["X-Ratelimit-Limit"]);
                }catch{}                
            }

            return result;
        }

        private int GetLimitRemainingForThisHour(Dictionary<string, string> responseHeaders){
            int result = -1;
            if(!DidReturnSameResultForThisAPIKey(responseHeaders) && responseHeaders.ContainsKey("X-Ratelimit-Remaining")){
                try {
                    result = int.Parse(responseHeaders["X-Ratelimit-Remaining"]);
                }catch{}                
            }

            return result;
        }

        private bool DidReturnSameResultForThisAPIKey(Dictionary<string, string> responseHeaders){
            return responseHeaders.ContainsKey("Age") && int.Parse(responseHeaders["Age"]) > 0;
        }
    }

}