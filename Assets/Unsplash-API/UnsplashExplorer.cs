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
    /// Get a single page of photo results for a query.
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="collections">Collection ID(‘s) to narrow search. If multiple, comma-separated.</param>
    /// <param name="page">Page number to retrieve.</param>
    /// <param name="per_page">Number of items per page. Default: 10, Min: 1, Max: 30</param>
    /// <param name="orientation">Filter search results by photo orientation.</param>
    /// <returns></returns>
    public Task<List<UnsplashPhoto>> SearchPhotos(string query, string collections = null, int page = 1, int per_page = 10,
                 UnsplashPhotoOrientation orientation = UnsplashPhotoOrientation.Any)
    {

        var completionSource = new TaskCompletionSource<List<UnsplashPhoto>>();

        var req = new UnsplashSearchRequest();
        req.GetSearchResultsAsync(query, collections, page, per_page, orientation).ContinueWith(t => {            
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

    /// <summary>
    /// Retrieve random photos, given optional filters.
    /// </summary>
    /// <param name="count">The number of photos to return. (Default: 1; max: 30)</param>
    /// <param name="only_featured">Limit selection to featured photos.</param>
    /// <param name="query">Limit selection to photos matching a search term.</param>
    /// <param name="collections">Public collection ID(‘s) to filter selection. If multiple, comma-separated</param>
    /// <param name="user">Limit selection to a single user.</param>
    /// <param name="orientation">Filter search results by photo orientation.</param>
    /// <returns></returns>
    public Task<List<UnsplashPhoto>> GetRandomPhotos(int count = 1, bool only_featured = false, string query = null, 
                 string collections = null, string user = null, UnsplashPhotoOrientation orientation = UnsplashPhotoOrientation.Any)
    {

        var completionSource = new TaskCompletionSource<List<UnsplashPhoto>>();

        var req = new UnsplashRandomPhotosRequest();
        req.GetRandomPhotosAsync(count, only_featured, query, user, collections, orientation).ContinueWith(t => {            
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

    /// <summary>
    /// Retrieve a random photo, given optional filters.
    /// </summary>
    /// <param name="only_featured">Limit selection to featured photos.</param>
    /// <param name="query">Limit selection to photos matching a search term.</param>
    /// <param name="collections">Public collection ID(‘s) to filter selection. If multiple, comma-separated</param>
    /// <param name="user">Limit selection to a single user.</param>
    /// <param name="orientation">Filter search results by photo orientation.</param>
    /// <returns></returns>
    public Task<UnsplashPhoto> GetRandomPhoto(bool only_featured = false, string query = null, 
                 string collections = null, string user = null, UnsplashPhotoOrientation orientation = UnsplashPhotoOrientation.Any)
    {
        var completionSource = new TaskCompletionSource<UnsplashPhoto>();
        GetRandomPhotos(1, only_featured, query, collections, user, orientation).ContinueWith(t => {
            if(t.IsCanceled){
                completionSource.SetCanceled();
            }else if(t.IsFaulted){
                completionSource.SetException(t.Exception);
            }else{
                completionSource.SetResult(t.Result[0]);
            }
        });

        return completionSource.Task;
    }

    
    
}
