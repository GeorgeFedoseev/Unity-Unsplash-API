using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnsplashExplorerForUnity.Model;

namespace UnsplashExplorerForUnity {    

    public class UnsplashDownloader {

        private UnsplashPhoto _photo;
        private MonoBehaviour _coroutineRunner;

        private TaskCompletionSource<Texture2D> _taskCompletionSource;

        private IProgress<float> _progressReporter;

        private Coroutine _downloadCoroutine;

        public UnsplashDownloader(UnsplashPhoto photo, IProgress<float> progressReporter){
            _photo = photo;
            _coroutineRunner = UnsplashExplorer.Main;
            _progressReporter = progressReporter;
        }

        public Task<Texture2D> DownloadImageAsync(UnsplashPhotoSize size){
            _taskCompletionSource = new TaskCompletionSource<Texture2D>();

            var url = _photo.urls[size.ToString().ToLower()];
            _downloadCoroutine = _coroutineRunner.StartCoroutine(DownloadCoroutine(url));

            return _taskCompletionSource.Task;
        }

        public void CancelDownload(){
            if(_downloadCoroutine != null){
                _coroutineRunner.StopCoroutine(_downloadCoroutine);
                _downloadCoroutine = null;
                _taskCompletionSource.SetCanceled();
            }
        }

        private IEnumerator DownloadCoroutine(string url){
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            www.SendWebRequest();            

            while(!www.isDone){
                if(_progressReporter != null){
                    _progressReporter.Report(www.downloadProgress);
                }                
                yield return null;
            }

            if(www.isNetworkError || www.isHttpError) {
                _taskCompletionSource.SetException(new UnsplashRequestException(www.error));
            }
            else {
                var texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                _taskCompletionSource.SetResult(texture);
            }
        }
    }

}