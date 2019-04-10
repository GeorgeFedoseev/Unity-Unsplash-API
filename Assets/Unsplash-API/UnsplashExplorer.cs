using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using UnsplashExplorerForUnity;
using UnsplashExplorerForUnity.Model;

public class UnsplashExplorer : MonoBehaviour {
    private static UnsplashExplorer _instance;
    public static UnsplashExplorer Main {
        get {
            if(_instance == null){
                var found = FindObjectOfType<UnsplashExplorer>();
                if(found == null){
                    throw new UnsplashExplorerScriptNotFoundException("Please add UnsplashExplorer script to your scene.");                    
                }
                
                return found;
            }
            return _instance;
        }
    }

    [SerializeField]
    private string _unsplashAccessKey = "";

    public string AccessKey {
        get {
            return _unsplashAccessKey;
        }
    }

    // METHODS

    /// <summary>
    /// Get Unsplash photos by query.
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="page"></param>
    /// <param name="per_page">Default: 10, Min: 1, Max: 30</param>
    /// <returns></returns>
    public Task<List<UnsplashPhoto>> SearchPhotos(string query, int page = 1, int per_page = 10,
                 UnsplashPhotoOrientation orientation = UnsplashPhotoOrientation.Any)
    {

        var completionSource = new TaskCompletionSource<List<UnsplashPhoto>>();

        var req = new UnsplashSearchRequest();
        req.GetSearchResultsAsync(query, page, per_page, orientation).ContinueWith(t => {            
            if(t.IsCanceled){
                completionSource.SetCanceled();
            }else if(t.IsFaulted){
                completionSource.SetException(t.Exception);
            }else{
                completionSource.SetResult(t.Result.results);
            }
        }, TaskScheduler.FromCurrentSynchronizationContext());

        return completionSource.Task;
    }

    
    
}
