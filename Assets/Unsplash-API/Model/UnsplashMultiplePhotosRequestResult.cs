using System;
using System.Collections.Generic;

namespace UnsplashExplorerForUnity {

    [Serializable]
    public class UnsplashMultiplePhotosRequestResult : UnsplashRequestResult {
        public long total;
        public long total_pages;
        public List<UnsplashPhoto> results;
    }
}