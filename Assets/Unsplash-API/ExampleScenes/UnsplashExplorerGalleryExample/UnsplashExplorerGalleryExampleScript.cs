using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnsplashExplorerForUnity;

public class UnsplashExplorerGalleryExampleScript : MonoBehaviour
{   
    private float SCROLL_LOAD_THRESHOLD = 0.05f;
    private float SCROLL_LOAD_COOLDOWN_SEC = 1f;
    private float SCROLL_LOAD_ERROR_COOLDOWN_SEC = 5f;

    [SerializeField]
    private Text _apiStatusText;

    [SerializeField]
    private InputField _searchInput;

    [SerializeField]
    private Button _clearInputButton;

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
    private float _lastTimeFailedToLoadPage = -999f;

    // PREVIEW
    [SerializeField]
    private GameObject _preview;

    [SerializeField]
    private Button _previewOverlayButton;

    [SerializeField]
    private PhotoCellScript _previewPhotoCell;

    // EMPTY
    [SerializeField]
    private GameObject _emptyMessageContainer;

    [SerializeField]
    private Text _emptyMessageText;

    [SerializeField]
    private Button _surpriseMeButton;




    // Start is called before the first frame update
    void Start() {
        Application.targetFrameRate = 60;

        _searchInput.keyboardType = TouchScreenKeyboardType.Search;

        ClearContainer(_searchResultsContainer);
        ShowPageLoadingIndicator(false);
        ClosePreview();     
        ShowEmptyMessageContainer(true, "TYPE SOMETHING\nor");  
        _clearInputButton.gameObject.SetActive(false); 

        _searchInput.Select();

       _searchInput.onValueChanged.AddListener((val) => {
           if(_debounceInputCoroutine != null){
               StopCoroutine(_debounceInputCoroutine);
           }
           _debounceInputCoroutine = StartCoroutine(DebounceInputCoroutine());
       });        

       _clearInputButton.onClick.AddListener(() => {
           _searchInput.text = "";
           _searchInput.Select();
       });

       _previewOverlayButton.onClick.AddListener(ClosePreview);

       _surpriseMeButton.onClick.AddListener(ShowRandomPhoto);

       // limits tracking
       UnsplashExplorer.Main.OnAPIRequestLimitsReport += (report) => {
           if(report.Remaining == 0){
               _apiStatusText.text = $"<color=red>Unsplash requests limit of {report.Total} calls for this hour exceeded</color>";
           }else{
               _apiStatusText.text = $"Unsplash API requests used for this hour: {report.Remaining}/{report.Total}";
           }
       };
       
    }
    
    void LateUpdate()
    {
        if(_scrollRect.verticalNormalizedPosition < SCROLL_LOAD_THRESHOLD){
            if(!_isLoadingPage && _currentPage > 0 && _currentPage < _totalPages 
                    && Time.time - _lastTimeAppendedCells > SCROLL_LOAD_COOLDOWN_SEC
                    && Time.time - _lastTimeFailedToLoadPage > SCROLL_LOAD_ERROR_COOLDOWN_SEC
            ){                    
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
            ShowEmptyMessageContainer(true, "TYPE SOMETHING\nor");
            ClearContainer(_searchResultsContainer);
            _currentPage = -1;
            _clearInputButton.gameObject.SetActive(false);
            yield break;
        }

        _clearInputButton.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.5f);        

        _currentPage = 0;
        LoadNextPage();

        _debounceInputCoroutine = null;
    }

    private void ShowRandomPhoto(){

        _preview.SetActive(true);

        UnsplashExplorer.Main.GetRandomPhoto(onlyFeatured:true).ContinueWith(t => {
            if(t.IsCanceled){
                print("query canceled");
                _preview.SetActive(false);
            }else if(t.IsFaulted){
                Debug.LogError($"query failed: {t.Exception}");
                _preview.SetActive(false);
            }else{                
                ShowPreviewWith(t.Result);
            }            
        }, TaskScheduler.FromCurrentSynchronizationContext()).LogExceptions();
    }

    private void LoadPhotosForQuery(string query, int page = 1){       

        OnStartLoadingPage();        

        UnsplashExplorer.Main.SearchPhotos(query, page:page, perPage:30).ContinueWith(t => {
            if(t.IsCanceled){
                print("search query canceled");
            }else if(t.IsFaulted){
                Debug.LogError($"Search query failed: {t.Exception}");
                _lastTimeFailedToLoadPage = Time.time;
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

            var _photo = photo;
            cell.button.onClick.AddListener(() => {
                ShowPreviewWith(_photo);
            });
        }

        _lastTimeAppendedCells = Time.time;
    }

    private void ShowPageLoadingIndicator(bool show){
        _pageLoadingIndicator.transform.SetAsLastSibling();
        _pageLoadingIndicator.SetActive(show);
    }

    private void ShowPreviewWith(UnsplashPhoto photo){
        _preview.SetActive(true);
        _previewPhotoCell.InitWith(photo);
    }

    private void ClosePreview(){
        _previewPhotoCell.Reset();        
        _preview.SetActive(false);
    }


    private void ShowEmptyMessageContainer(bool show, string message = "NOTHING FOUND"){
        _emptyMessageContainer.SetActive(show);
        _emptyMessageText.text = message;
    }
    

    // EVENTS

    private void OnStartLoadingPage(){
        ShowPageLoadingIndicator(true);
        _isLoadingPage = true;

        ShowEmptyMessageContainer(false);
    }

    private void OnFinishedLoadingPage(){
        ShowPageLoadingIndicator(false);
        _isLoadingPage = false;

        if(_totalPages == 0){
            ShowEmptyMessageContainer(true);
        }
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
