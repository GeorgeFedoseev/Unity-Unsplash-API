using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnsplashExplorerForUnity;
using UnsplashExplorerForUnity.Model;

public class UnsplashExplorerGalleryExampleScript : MonoBehaviour
{   

    [SerializeField]
    private InputField _searchInput;

    [SerializeField]
    private Transform _searchResultsContainer;
    
    [SerializeField]
    private GameObject _photoCellPrefab;

    private Coroutine _debounceCoroutine;

    // Start is called before the first frame update
    void Start() {
        Application.targetFrameRate = 60;


        ClearContainer(_searchResultsContainer);

        _searchInput.Select();

       _searchInput.onValueChanged.AddListener((val) => {
           if(_debounceCoroutine != null){
               StopCoroutine(_debounceCoroutine);
           }
           _debounceCoroutine = StartCoroutine(DebounceCoroutine());
       });
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // METHODS
    IEnumerator DebounceCoroutine(){
        yield return new WaitForSeconds(0.5f);
        LoadPhotosForQuery(_searchInput.text);

        _debounceCoroutine = null;
    }

    private void LoadPhotosForQuery(string query, int page = 1){
        if(string.IsNullOrWhiteSpace(query)){
            ClearContainer(_searchResultsContainer);
            return;
        }

        UnsplashExplorer.Main.SearchPhotos(query).ContinueWith(t => {
            if(t.IsCanceled){
                print("search query canceled");
            }else if(t.IsFaulted){
                Debug.LogError($"Search query failed: {t.Exception}");
            }else{
                AppendSearchResults(t.Result, page);
            }
        }, TaskScheduler.FromCurrentSynchronizationContext()).LogExceptions();
    }

    private void AppendSearchResults(List<UnsplashPhoto> photos, int page){
        if(page == 1){
            ClearContainer(_searchResultsContainer);
        }

        foreach(var photo in photos){
            var cell = InstantiateIntoContainer<PhotoCellScript>(_photoCellPrefab, _searchResultsContainer);
            cell.InitWith(photo);
        }
    }




    // STATIC
    public static void ClearContainer(Transform container){
        foreach(Transform t in container){
            if(Application.isPlaying){
                GameObject.Destroy(t.gameObject);
            }else{
                GameObject.DestroyImmediate(t.gameObject);
            }
            
        }
    }   

    public static T InstantiateIntoContainer<T>(UnityEngine.Object prefab, Transform container) where T: MonoBehaviour {
        var a = (GameObject.Instantiate(prefab) as GameObject).GetComponent<T>();
        a.transform.SetParent(container);
        a.transform.localScale = Vector3.one;
        a.transform.localPosition = Vector3.zero;

        return a;
    }
}
