

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnsplashExplorerForUnity {

    [Serializable]
    public class UnsplashPhoto {
        public string id;
        public string created_at;

        public int width;
        public int height;

        public string color;

        public long likes;
        public bool liked_by_user;

        public string description;

        public UnsplashUser user;

        public UnsplashPhotoUrls urls;
        public UnsplashPhotoLinks links;
    }
}