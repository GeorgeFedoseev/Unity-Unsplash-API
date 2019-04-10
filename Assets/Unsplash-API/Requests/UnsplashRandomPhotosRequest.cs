using System;
using System.Threading.Tasks;
using UnityEngine;
using UnsplashExplorerForUnity.Model;

namespace UnsplashExplorerForUnity {

    public class UnsplashRandomPhotosRequest : UnsplashRequest {


        // https://unsplash.com/documentation#get-a-random-photo
        public Task<UnsplashMultiplePhotosRequestResult> GetRandomPhotosAsync(int count, bool only_featured, string query,
                                              string user, string collections, UnsplashPhotoOrientation orientation)
        {
            
            var orientation_param = orientation == UnsplashPhotoOrientation.Any ? "" : $"&orientation={orientation.ToString().ToLower()}";
            var query_param = query == null ? "" : $"&query={query}";
            var user_param = user == null ? "" : $"&user={user}";
            var featured_param = $"&featured={(only_featured?1:0)}";
            var collections_param = collections == null ? "" : $"&collections={collections}";

            var url = $"https://api.unsplash.com/photos/random?count={count}{featured_param}{query_param}{user_param}{collections_param}{orientation_param}";
            
            var completionSource = new TaskCompletionSource<UnsplashMultiplePhotosRequestResult>();

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
                        var result = JsonUtility.FromJson<UnsplashMultiplePhotosRequestResult>(jsonResultString);

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