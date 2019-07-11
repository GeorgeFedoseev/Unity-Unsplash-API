using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace UnsplashExplorerForUnity {    

    public class UnsplashDownloader {

        private UnsplashPhoto _photo;
        private UnsplashExplorer _driver;

        private TaskCompletionSource<Texture2D> _taskCompletionSource;

        private IProgress<float> _progressReporter;

        private Coroutine _downloadCoroutine;

        public UnsplashDownloader(){            
            _driver = UnsplashExplorer.Main;            
        }

        public Task<Texture2D> DownloadPhotoAsync(UnsplashPhoto photo, IProgress<float> progressReporter, 
                                                    UnsplashPhotoSize size = UnsplashPhotoSize.Regular, bool incrementPhotoDownloadsCount = false){

            if(_downloadCoroutine != null){
                throw new InvalidOperationException("Download is already in progress");
            }
            
            _progressReporter = progressReporter;
            _photo = photo;

            _taskCompletionSource = new TaskCompletionSource<Texture2D>();

            var url = _photo.urls.GetUrlForSize(size);
            _downloadCoroutine = _driver.StartCoroutine(DownloadCoroutine(url));

            // log donwload to Unsplash analytics to make author happy
            if(incrementPhotoDownloadsCount){                
                SendIncrementDownloadsCountRequest(_photo);
            }
            

            return _taskCompletionSource.Task;
        }

        public void CancelDownload(){
            if(_downloadCoroutine != null){
                _driver.StopCoroutine(_downloadCoroutine);
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

            if(_progressReporter != null){
                    _progressReporter.Report(1f);
                }

            if(www.isNetworkError || www.isHttpError) {
                _taskCompletionSource.SetException(new UnsplashRequestException(www.error));
            }
            else {
                var texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                _taskCompletionSource.SetResult(texture);
            }

            _downloadCoroutine = null;
        }

        private async void SendIncrementDownloadsCountRequest(UnsplashPhoto photo){
            var request = new UnsplashRequest(_driver);
            await request.GetResponseStringAsync(photo.links.download_location);            
        }
    }

}