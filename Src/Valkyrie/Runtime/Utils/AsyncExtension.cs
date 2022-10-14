using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tools;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utils
{
    public static class AsyncExtension
    {
        private static CoroutineRunner _coroutineSource;
        private static readonly object LockObject = new object();

        class CoroutineRunner : MonoBehaviour
        {
            public struct LateUpdateAction
            {
                public Action Work;
                public CancellationToken Token;
            }

            public readonly List<LateUpdateAction> LateUpdateActions = new();
            
            private void LateUpdate()
            {
                for (var i = 0; i < LateUpdateActions.Count;)
                {
                    if (LateUpdateActions[i].Token.IsCancellationRequested)
                        LateUpdateActions.RemoveAndReplaceWithLast(i);
                    else
                    {
                        LateUpdateActions[i].Work();
                        ++i;
                    }
                }
            }
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
            if(cancellationToken.IsCancellationRequested)
                return;

            Runner.LateUpdateActions.Add(new CoroutineRunner.LateUpdateAction()
            {
                Token = cancellationToken,
                Work = work
            });
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