

using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace UnsplashExplorerForUnity {

    public class UnsplashRequest {
        
        private MonoBehaviour _coroutineRunner;

        private TaskCompletionSource<string> _taskCompletionSource;        

        private Coroutine _requestCoroutine;

        public UnsplashRequest(){
            _coroutineRunner = UnsplashExplorer.Main;
        }

        public Task<string> GetResponseStringAsync(string url){
            _taskCompletionSource = new TaskCompletionSource<string>();                  
            _requestCoroutine = _coroutineRunner.StartCoroutine(RequestCoroutine(url));

            return _taskCompletionSource.Task;
        }

        public void CancelRequest(){
            if(_requestCoroutine != null){
                _coroutineRunner.StopCoroutine(_requestCoroutine);
                _requestCoroutine = null;
                _taskCompletionSource.SetCanceled();
            }
        }

        private IEnumerator RequestCoroutine(string url){
            UnityWebRequest www = UnityWebRequest.Get(url);

            www.SetRequestHeader("Authorization", $"Client-ID {UnsplashExplorer.Main.AccessKey}");

            yield return www.SendWebRequest();            

            if(www.isNetworkError || www.isHttpError) {
                _taskCompletionSource.SetException(new UnsplashRequestException(www.error));
            }
            else {
                var responseString = www.downloadHandler.text;
                _taskCompletionSource.SetResult(responseString);
            }
        }
    }

}