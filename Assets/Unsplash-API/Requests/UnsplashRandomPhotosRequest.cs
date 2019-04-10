using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnsplashExplorerForUnity.Model;

namespace UnsplashExplorerForUnity {

    public class UnsplashRandomPhotosRequest : UnsplashRequest {


        // https://unsplash.com/documentation#get-a-random-photo
        public Task<UnsplashPhoto> GetRandomPhotoAsync(bool only_featured, string query,
                                              string user, string collections, UnsplashPhotoOrientation orientation)
        {
            
            var orientation_param = orientation == UnsplashPhotoOrientation.Any ? "" : $"&orientation={orientation.ToString().ToLower()}";
            var query_param = string.IsNullOrWhiteSpace(query) ? "" : $"&query={query}";
            var user_param = string.IsNullOrWhiteSpace(user) ? "" : $"&user={user}";
            var featured_param = $"&featured={(only_featured?1:0)}";
            var collections_param = string.IsNullOrWhiteSpace(collections) ? "" : $"&collections={collections}";

            var url = $"https://api.unsplash.com/photos/random?{featured_param}{query_param}{user_param}{collections_param}{orientation_param}";
            
            var completionSource = new TaskCompletionSource<UnsplashPhoto>();

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
                        var result = JsonUtility.FromJson<UnsplashPhoto>(jsonResultString);

                        Debug.Log("parsed json string");                       
                        
                        completionSource.SetResult(result);
                                                
                    }catch(Exception ex){
                        completionSource.SetException(ex);
                    }
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());

            return completionSource.Task;
        }

    }

}