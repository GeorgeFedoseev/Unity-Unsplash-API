using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnsplashExplorerForUnity;
using UnsplashExplorerForUnity.Model;

public class ExampleUnsplashExplorerScript : MonoBehaviour
{
    [SerializeField]
    private RawImage _rawImage;

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

        UnsplashExplorer.Main.GetRandomPhoto().ContinueWith(getPhotoTask => {            
            if(getPhotoTask.IsCanceled){
                Debug.Log("search cancelled");
            }else if(getPhotoTask.IsFaulted){
                Debug.LogError($"search failed: {getPhotoTask.Exception}");
            }else{
                var photo = getPhotoTask.Result;
                if(photo != null){
                    new UnsplashDownloader().DownloadPhotoAsync(photo, new Progress<float>((progress) => {
                        print($"Downloading picture: {progress}");
                    }), UnsplashPhotoSize.Thumb)

                    .ContinueWith(t => {
                        if(t.IsCanceled){
                            print("Download canceled");
                        }else if(t.IsFaulted){
                            Debug.LogError($"Failed to download image: {t.Exception}");
                        }else{
                            SetTexture(t.Result);
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext()).LogExceptions();
                }
            }
        }, TaskScheduler.FromCurrentSynchronizationContext()).LogExceptions();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // METHODS
    private void SetTexture(Texture2D texture){
        _rawImage.texture = texture;
    }
}
