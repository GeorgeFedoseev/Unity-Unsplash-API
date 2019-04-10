using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnsplashExplorerForUnity;

public class ExampleUnsplashExplorerScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // var req = new UnsplashSearchRequest();
        // req.GetSearchResultsAsync("lion").ContinueWith(t => {
        //     if(t.IsCanceled){
        //         Debug.Log("search request cancelled");
        //     }else if(t.IsFaulted){
        //         Debug.LogError($"search request failed: {t.Exception}");
        //     }else{                           
        //         Debug.Log($"Received results: {t.Result.total}");
        //     }
        // }, TaskScheduler.FromCurrentSynchronizationContext());

        UnsplashExplorer.Main.SearchPhotos("cucumber").ContinueWith(t => {            
            if(t.IsCanceled){
                Debug.Log("search cancelled");
            }else if(t.IsFaulted){
                Debug.LogError($"search failed: {t.Exception}");
            }else{
                Debug.Log($"Received photos: {t.Result.Count}");
                Debug.Log($"Photo url t.Result[0].urls == null: {t.Result[0].urls == null}");
            }
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
