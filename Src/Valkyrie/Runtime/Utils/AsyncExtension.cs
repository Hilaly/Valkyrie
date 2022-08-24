using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Valkyrie.MVVM
{
    public static class AsyncExtension
    {
        private static CoroutineRunner _coroutineSource;
        private static readonly object LockObject = new object();

        class CoroutineRunner : MonoBehaviour
        {
            
        }

        private static CoroutineRunner Runner
        {
            get
            {
                lock (LockObject)
                {
                    if (_coroutineSource == null)
                    {
                        var go = new GameObject("CoroutineRunner");
                        //TODO: HideAndDontSave
                        UnityEngine.Object.DontDestroyOnLoad(go);
                        _coroutineSource = go.AddComponent<CoroutineRunner>();
                    }

                    return _coroutineSource;
                }
            }
        }

        public static Task WaitForEndOfFrame() => CoroutineAwaiterToTask(new WaitForEndOfFrame());
        public static Task WaitForFixedUpdate() => CoroutineAwaiterToTask(new WaitForFixedUpdate());
        public static Task WaitForEndUpdate() => CoroutineAwaiterToTask(null);
        public static Task WaitForSeconds(float seconds) => CoroutineAwaiterToTask(new WaitForSeconds(seconds));

        public static async void RunEveryUpdate(Action work, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await WaitForEndUpdate();
                work();
            }
        }

        static Task CoroutineAwaiterToTask(object awaiter)
        {
            var tcs = new TaskCompletionSource<bool>();
            Runner.StartCoroutine(BaseCoroutine(awaiter, tcs));
            return tcs.Task;
        }

        static IEnumerator BaseCoroutine(object toWait, TaskCompletionSource<bool> tcs)
        {
            yield return toWait;
            tcs.TrySetResult(true);
        }

        public static Task LoadSceneAsync(string sceneName, LoadSceneMode loadSceneMode)
        {
            return CoroutineAwaiterToTask(SceneManager.LoadSceneAsync(sceneName, loadSceneMode));
        }
    }
}