using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnsplashExplorerForUnity;

public class SimpleRandomExampleScript : MonoBehaviour
{

    public RawImage rawImage;

    // Start is called before the first frame update
    void Start()
    {
        UnsplashExplorer.Main.GetRandomPhotoAsync(onlyFeatured:true)
            .ContinueWith(t => {
                if(t.IsCanceled){
                    return;
                }else if(t.IsFaulted){
                    Debug.LogError($"Failed to get random photo: {t.Exception}");
                }else{
                    LoadPhoto(t.Result);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext()).LogExceptions();
    }

    void LoadPhoto(UnsplashPhoto photo){
        new UnsplashDownloader().DownloadPhotoAsync(photo, null).ContinueWith(t => {
            if(t.IsCanceled){
                print("Photo download canceled");
            }else if(t.IsFaulted){
                Debug.LogError($"Failed to download photo: {t.Exception}");
            }else{
                var tex = t.Result;
                rawImage.texture = tex;
                rawImage.GetComponent<AspectRatioFitter>().aspectRatio = (float)tex.width/tex.height;
            }
        }, TaskScheduler.FromCurrentSynchronizationContext()).LogExceptions();
    }

   
}
