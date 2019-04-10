using System;
using System.Threading.Tasks;
using UnityEngine;
using UnsplashExplorerForUnity.Model;

namespace UnsplashExplorerForUnity {

    public class UnsplashSearchRequest : UnsplashRequest {

        // public UnsplashSearchRequest() : base() {
                        
        // }

        
        public Task<UnsplashSearchRequestResult> GetSearchResultsAsync(string query, int page, int per_page, UnsplashPhotoOrientation orientation){
            
            var orientation_param = orientation == UnsplashPhotoOrientation.Any ? "" : $"&orientation={orientation.ToString().ToLower()}";

            var url = $"https://api.unsplash.com/search/photos?page={page}&per_page={per_page}&query={query}{orientation_param}";
            
            var completionSource = new TaskCompletionSource<UnsplashSearchRequestResult>();

            GetResponseStringAsync(url).ContinueWith(t => {
                if(t.IsCanceled){
                    completionSource.SetCanceled();
                }else if(t.IsFaulted){
                    completionSource.SetException(t.Exception);
                }else{

                    var jsonResultString = t.Result;

                    Debug.Log($"Received json string: {jsonResultString}");
                    
                    // parse json
                    try {
                        var result = JsonUtility.FromJson<UnsplashSearchRequestResult>(jsonResultString);

                        Debug.Log("parsed json string");

                        if(result.HasErrors){
                            var apiException = new UnsplashAPIException($"{result.errors.Count} error(-s): {string.Join("; ", result.errors)}");
                            completionSource.SetException(apiException);
                        }else{
                            completionSource.SetResult(result);
                        }                        
                    }catch(Exception ex){
                        completionSource.SetException(ex);
                    }
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());

            return completionSource.Task;
        }

    }

}