using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnsplashExplorerForUnity;

public class SimpleSearchExampleScript : MonoBehaviour
{
    public Transform resultsContainer;

    public GameObject photoPrefab;

    // Start is called before the first frame update
    void Start()
    {
        UnsplashExplorer.Main.SearchPhotosAsync("watermelon", perPage: 9, orientation: UnsplashPhotoOrientation.Landscape)
            .ContinueWith(t => {
                if(t.IsCanceled){
                    print("query canceled");
                }else if(t.IsFaulted){
                    Debug.LogError($"Failed to load search results: {t.Exception}");
                }else{
                    DisplayResults(t.Result.results);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext()) // to run on main thread
            .LogExceptions(); // log any exceptions in last task to Unity console
        
    }

    private void DisplayResults(List<UnsplashPhoto> photos){
        ClearContainer(resultsContainer);

        foreach(var photo in photos){
            var cell = InstantiateIntoContainer<MonoBehaviour>(photoPrefab, resultsContainer);
            new UnsplashDownloader().DownloadPhotoAsync(photo, null, UnsplashPhotoSize.Thumb)
                .ContinueWith(t => {
                    if(t.IsCanceled){
                        print("Photo download canceled");
                    }else if(t.IsFaulted){
                        Debug.LogError($"Failed to download photo: {t.Exception}");
                    }else{
                        var tex = t.Result;
                        cell.GetComponentInChildren<RawImage>().texture = tex;
                        cell.GetComponentInChildren<AspectRatioFitter>().aspectRatio = (float)tex.width/tex.height;
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext()) // to run on main thread
                .LogExceptions(); // log any exceptions in last task to Unity console
        }
    }

    // UTILS
    private void ClearContainer(Transform container){
        // destroy allocated textures
        foreach(var rawImage in container.GetComponentsInChildren<RawImage>()){
            if(rawImage.texture != null){
                Destroy(rawImage.texture);
            }
        }

        // destroy gameobjects
        foreach(Transform t in container){

            if(Application.isPlaying){
                GameObject.Destroy(t.gameObject);
            }else{
                GameObject.DestroyImmediate(t.gameObject);
            }
            
        }
    }   

    private static T InstantiateIntoContainer<T>(UnityEngine.Object prefab, Transform container) where T: MonoBehaviour {
        var a = (GameObject.Instantiate(prefab) as GameObject).GetComponent<T>();
        a.transform.SetParent(container);
        a.transform.localScale = Vector3.one;
        a.transform.localPosition = Vector3.zero;

        return a;
    }
}
