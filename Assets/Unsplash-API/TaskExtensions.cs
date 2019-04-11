
using System.Threading.Tasks;
using UnityEngine;

namespace UnsplashExplorerForUnity {

    public static class TaskExtensions {
        public static void LogExceptions(this Task task){
            task.ContinueWith(t => {
                Debug.LogException(t.Exception);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}