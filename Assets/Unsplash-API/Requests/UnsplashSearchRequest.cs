using System;
using System.Threading.Tasks;
using UnityEngine;
using UnsplashExplorerForUnity.Model;

namespace UnsplashExplorerForUnity {

    public class UnsplashSearchRequest : UnsplashRequest {

        public UnsplashSearchRequest(UnsplashExplorer driver) : base(driver){}
        

        // https://unsplash.com/documentation#search-photos
        public Task<UnsplashMultiplePhotosRequestResult> GetSearchResultsAsync(string query, string collections, int page, int per_page, UnsplashPhotoOrientation orientation){
            
            var orientation_param = orientation == UnsplashPhotoOrientation.Any ? "" : $"&orientation={orientation.ToString().ToLower()}";
            var collections_param = string.IsNullOrWhiteSpace(collections) ? "" : $"&collections={collections}";

            var url = $"https://api.unsplash.com/search/photos?page={page}&per_page={per_page}&query={query}{collections_param}{orientation_param}";
            
            var completionSource = new TaskCompletionSource<UnsplashMultiplePhotosRequestResult>();

            GetResponseStringAsync(url).ContinueWith(t => {
                if(t.IsCanceled){
                    completionSource.SetCanceled();
                }else if(t.IsFaulted){
                    completionSource.SetException(t.Exception);
                }else{
                    
                    var jsonResultString = t.Result;
                    
                    // parse json
                    try {
                        var result = JsonUtility.FromJson<UnsplashMultiplePhotosRequestResult>(jsonResultString);

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