﻿using System;
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
    private float SCROLL_LOAD_THRESHOLD = 0.05f;
    private float SCROLL_LOAD_COOLDOWN_SEC = 1f;


    [SerializeField]
    private InputField _searchInput;

    [SerializeField]
    private ScrollRect _scrollRect;

    [SerializeField]
    private Transform _searchResultsContainer;
    
    [SerializeField]
    private GameObject _photoCellPrefab;

    [SerializeField]
    private GameObject _pageLoadingIndicator;

    private Coroutine _debounceInputCoroutine;

    private int _currentPage = -1;
    private long _totalPages = -1;
    private bool _isLoadingPage = false;
    
    
    private float _lastTimeAppendedCells = -999f;



    // Start is called before the first frame update
    void Start() {
        Application.targetFrameRate = 60;

        _searchInput.keyboardType = TouchScreenKeyboardType.Search;

        ClearContainer(_searchResultsContainer);
        ShowPageLoadingIndicator(false);

        _searchInput.Select();

       _searchInput.onValueChanged.AddListener((val) => {
           if(_debounceInputCoroutine != null){
               StopCoroutine(_debounceInputCoroutine);
           }
           _debounceInputCoroutine = StartCoroutine(DebounceInputCoroutine());
       });        
    }
    
    void LateUpdate()
    {
        if(_scrollRect.verticalNormalizedPosition < SCROLL_LOAD_THRESHOLD){
            if(!_isLoadingPage && _currentPage > 0 && _currentPage < _totalPages && Time.time - _lastTimeAppendedCells > SCROLL_LOAD_COOLDOWN_SEC){
                print("wanna load");
                LoadNextPage();
            }            
        }
    }

    // METHODS

    private void LoadNextPage(){
       _currentPage += 1;
        LoadPhotosForQuery(_searchInput.text, _currentPage);
    }

    IEnumerator DebounceInputCoroutine(){
        if(string.IsNullOrWhiteSpace(_searchInput.text)){
            ClearContainer(_searchResultsContainer);
            _currentPage = -1;
            yield break;
        }

        yield return new WaitForSeconds(0.5f);        

        _currentPage = 0;
        LoadNextPage();

        _debounceInputCoroutine = null;
    }

    private void LoadPhotosForQuery(string query, int page = 1){       

        OnStartLoadingPage();

        UnsplashExplorer.Main.SearchPhotos(query, page:page, perPage:30).ContinueWith(t => {
            if(t.IsCanceled){
                print("search query canceled");
            }else if(t.IsFaulted){
                Debug.LogError($"Search query failed: {t.Exception}");
            }else{
                _totalPages = t.Result.total_pages;
                AppendSearchResults(t.Result.results, page);
            }

            OnFinishedLoadingPage();
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

        _lastTimeAppendedCells = Time.time;
    }

    private void ShowPageLoadingIndicator(bool show){
        _pageLoadingIndicator.transform.SetAsLastSibling();
        _pageLoadingIndicator.SetActive(show);
    }
    

    // EVENTS

    private void OnStartLoadingPage(){
        ShowPageLoadingIndicator(true);
        _isLoadingPage = true;
    }

    private void OnFinishedLoadingPage(){
        ShowPageLoadingIndicator(false);
        _isLoadingPage = false;
    }



    // UTILS
    public void ClearContainer(Transform container){
        foreach(Transform t in container){
            if(t.gameObject == _pageLoadingIndicator){
                continue;
            }

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
