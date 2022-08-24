using System;
using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.MVVM
{
    public class DisposableUnityComponent : MonoBehaviour
    {
        readonly List<IDisposable> _compositeDisposable = new List<IDisposable>();

        public void Add(IDisposable disposable)
        {
            _compositeDisposable.Add(disposable);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnDestroy()
        {
            _compositeDisposable.ForEach(x => x.Dispose());
            _compositeDisposable.Clear();
        }
    }
}