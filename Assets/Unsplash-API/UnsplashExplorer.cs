using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using UnsplashExplorerForUnity;
using UnsplashExplorerForUnity.Model;



public class UnsplashExplorer : MonoBehaviour {

    public Action<UnsplashAPIRequestLimitsInfo> OnAPIRequestLimitsReport = (_) => {};

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
    /// <param name="perPage">Number of items per page. Default: 10, Min: 1, Max: 30</param>
    /// <param name="orientation">Filter search results by photo orientation.</param>
    /// <returns></returns>
    public Task<UnsplashMultiplePhotosRequestResult> SearchPhotos(string query, string collections = null, int page = 1, int perPage = 10,
                 UnsplashPhotoOrientation orientation = UnsplashPhotoOrientation.Any)
    {
        var req = new UnsplashSearchRequest(this);
        return req.GetSearchResultsAsync(query, collections, page, perPage, orientation);
    }

    /// <summary>
    /// Retrieve a random photo, given optional filters.
    /// </summary>    
    /// <param name="onlyFeatured">Limit selection to featured photos.</param>
    /// <param name="query">Limit selection to photos matching a search term.</param>
    /// <param name="collections">Public collection ID(‘s) to filter selection. If multiple, comma-separated</param>
    /// <param name="user">Limit selection to a single user.</param>
    /// <param name="orientation">Filter search results by photo orientation.</param>
    /// <returns></returns>
    public Task<UnsplashPhoto> GetRandomPhoto(bool onlyFeatured = false, string query = null, 
                 string collections = null, string user = null, UnsplashPhotoOrientation orientation = UnsplashPhotoOrientation.Any)
    {

        var completionSource = new TaskCompletionSource<UnsplashPhoto>();

        var req = new UnsplashRandomPhotosRequest(this);
        req.GetRandomPhotoAsync(onlyFeatured, query, user, collections, orientation).ContinueWith(t => {            
            if(t.IsCanceled){
                completionSource.SetCanceled();
            }else if(t.IsFaulted){
                completionSource.SetException(t.Exception);
            }else{
                completionSource.SetResult(t.Result);
            }
        }, TaskScheduler.FromCurrentSynchronizationContext());

        return completionSource.Task;
    }

   

    
    
}
