

using System;
using System.Collections.Generic;

namespace UnsplashExplorerForUnity {

    [Serializable]
    public class UnsplashUser {
        public string id;
        public string username;
        public string name;
        public string first_name;
        public string last_name;
        public string instagram_username;
        public string twitter_username;
        public string portfolio_url;
        public UnsplashUserProfileImageUrls profile_image;
        public UnsplashUserLinks links;

    }

}