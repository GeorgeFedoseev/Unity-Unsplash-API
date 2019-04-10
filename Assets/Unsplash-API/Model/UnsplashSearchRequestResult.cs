using System;
using System.Collections.Generic;

namespace UnsplashExplorerForUnity.Model {

    [Serializable]
    public class UnsplashSearchRequestResult : UnsplashRequestResult {
        public long total;
        public long total_pages;
        public List<UnsplashPhoto> results;
    }
}