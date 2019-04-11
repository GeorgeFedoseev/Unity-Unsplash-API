

using System;

namespace UnsplashExplorerForUnity {

    public class UnsplashRequestException : Exception {
        public UnsplashRequestException(string errorMessage) : base(errorMessage) {}
    }

}