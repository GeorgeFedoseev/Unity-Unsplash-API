

using System;

namespace UnsplashExplorerForUnity {

    public class UnsplashAPIException : Exception {
        public UnsplashAPIException(string errorMessage) : base(errorMessage) {}
    }

}