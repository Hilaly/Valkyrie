using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tools;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valkyrie.Di;

namespace Utils
{
    public static class AsyncExtension
    {
        private static CoroutineRunner _coroutineSource;
        private static readonly object LockObject = new object();

        class CoroutineRunner : MonoBehaviour
        {
            public struct UpdateAction
            {
                public Action Work;
                public CancellationToken Token;
            }

            public readonly List<UpdateAction> LateUpdateActions = new();
            public readonly List<UpdateAction> UpdateActions = new();

            static void UpdateUpdateActions(List<UpdateAction> list)
            {
                for (var i = 0; i < list.Count;)
                    if (list[i].Token.IsCancellationRequested)
                        list.RemoveAndReplaceWithLast(i);
                    else
                    {
                        list[i].Work();
                        ++i;
                    }
            }

            private void LateUpdate() => UpdateUpdateActions(LateUpdateActions);
            private void Update() => UpdateUpdateActions(UpdateActions);
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

        public static void RunEveryLateUpdate(Action work, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            Runner.LateUpdateActions.Add(new CoroutineRunner.UpdateAction()
            {
                Token = cancellationToken,
                Work = work
            });
        }

        public static void RunEveryUpdate(Action work, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            Runner.UpdateActions.Add(new CoroutineRunner.UpdateAction()
            {
                Token = cancellationToken,
                Work = work
            });
        }

        public static IDisposable RunEveryUpdate(Action work)
        {
            var cts = new CancellationTokenSource();
            RunEveryUpdate(work, cts.Token);
            return new ActionDisposable(cts.Cancel);
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