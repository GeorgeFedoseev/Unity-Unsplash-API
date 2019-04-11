

using System;

namespace UnsplashExplorerForUnity {

    [Serializable]
    public class UnsplashPhotoUrls {
        public string raw;
        public string full;
        public string regular;
        public string small;
        public string thumb;

        public string GetUrlForSize(UnsplashPhotoSize size){
            switch(size){
                case UnsplashPhotoSize.Raw:
                    return raw;
                case UnsplashPhotoSize.Full:
                    return full;
                case UnsplashPhotoSize.Regular:
                    return regular;
                case UnsplashPhotoSize.Small:
                    return small;
                case UnsplashPhotoSize.Thumb:
                    return thumb;
            }

            return null;
        }
    }
}