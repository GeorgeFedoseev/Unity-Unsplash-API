﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnsplashExplorerForUnity;

public class PhotoCellScript : MonoBehaviour
{
    public UnsplashPhotoSize _photoSize = UnsplashPhotoSize.Thumb;

    public Button button;

    [SerializeField]
    private bool _attributionIsClickable = false;

    [SerializeField]
    private RawImage _rawImage;

    [SerializeField]
    private AspectRatioFitter _aspectRatioFitter;

    [SerializeField]
    private Text _attributionText;

    [SerializeField]
    private GameObject _loadingOverlay;
    [SerializeField]
    private Image _loadingProgressIndicator;

    [SerializeField]
    private GameObject _errorLoadingOverlay;
    [SerializeField]
    private Text _errorLoadingText;

    private UnsplashDownloader _downloader;
    private UnsplashPhoto _photo;
    



    public void InitWith(UnsplashPhoto photo){
        Reset();

        _photo = photo;

        _attributionText.text = $"Photo by {photo.user.name}";
        _attributionText.raycastTarget = _attributionIsClickable;
        if(_attributionIsClickable){            
            _attributionText.GetComponent<Button>().onClick.AddListener(() => {
                Application.OpenURL(photo.user.links.html);
            });
        }
        

        

        _downloader = new UnsplashDownloader();
        _downloader.DownloadPhotoAsync(_photo, new Progress<float>(OnDownloadProgress), _photoSize)
        .ContinueWith(t => {
            if(t.IsCanceled){
                ShowErrorLoading(true, "Loading Canceled");
            }else if(t.IsFaulted){
                ShowErrorLoading(true, "ERROR");
                Debug.LogException(t.Exception);
            }else{
                SetTexture(t.Result);
                ShowRawImage(true);
            }

            ShowLoading(false);            
        }, TaskScheduler.FromCurrentSynchronizationContext()).LogExceptions();
    }

    public void Reset(){
        SetTexture(null);
        _loadingProgressIndicator.fillAmount = 0;
        _attributionText.GetComponent<Button>().onClick.RemoveAllListeners();
        ShowErrorLoading(false);
        ShowRawImage(false);
        ShowLoading(true);
    }

    private void ShowLoading(bool show){
        _loadingOverlay.SetActive(show);
        _loadingProgressIndicator.gameObject.SetActive(show);
    }

    private void ShowErrorLoading(bool show, string message = "Error"){
        _errorLoadingOverlay.SetActive(show);
        _errorLoadingText.text = message;
    }

    private void ShowRawImage(bool show){
        _rawImage.gameObject.SetActive(show);
    }

    private void SetTexture(Texture2D tex){
        if(_rawImage.texture != null){
            Destroy(_rawImage.texture);
        }
        _rawImage.texture = tex;
        if(tex != null){
            _aspectRatioFitter.aspectRatio = (float)tex.width/tex.height;
        }        
    }

    // EVENTS
    private void OnDownloadProgress(float progress){
        _loadingProgressIndicator.fillAmount = progress;
    }

    private void OnDestroy(){
        if(_rawImage.texture != null){
            Destroy(_rawImage.texture);
        }
    }
}