

using System;

namespace UnsplashExplorerForUnity {

    public class UnsplashRequestLimitExceededException : Exception {
        public UnsplashRequestLimitExceededException(string errorMessage) : base(errorMessage) {}
    }

}