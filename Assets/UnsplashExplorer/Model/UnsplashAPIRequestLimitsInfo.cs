namespace UnsplashExplorerForUnity {

    public class UnsplashAPIRequestLimitsInfo {
        public int Remaining {get; private set;}
        public int Total {get; private set;}

        public UnsplashAPIRequestLimitsInfo(int remaining, int total){
            Remaining = remaining; Total = total;
        }
    }

}

