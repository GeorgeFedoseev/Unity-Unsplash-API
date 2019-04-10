using System;
using System.Collections.Generic;

namespace UnsplashExplorerForUnity.Model {

    [Serializable]
    public class UnsplashRequestResult {
        public List<string> errors;

        public bool HasErrors {
            get {
                return errors != null && errors.Count > 0;
            }
        }
    }

}