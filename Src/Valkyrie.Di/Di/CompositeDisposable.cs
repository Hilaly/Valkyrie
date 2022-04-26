using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Valkyrie.Di
{
    public class ActionDisposable : IDisposable
    {
        private readonly Action _disposeAction;

        public ActionDisposable(Action disposeAction)
        {
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            _disposeAction?.Invoke();
        }
    }

    public class CompositeDisposable
    {
        private readonly HashSet<IDisposable> _subs = new HashSet<IDisposable>();

        [Inject]
        public CompositeDisposable()
        {
        }

        public CompositeDisposable(IEnumerable<IDisposable> elements)
        {
            foreach (var disposable in elements)
                _subs.Add(disposable);
        }

        public CompositeDisposable(params IDisposable[] disposables)
        {
            foreach (var disposable in disposables)
                _subs.Add(disposable);
        }

        public void Dispose()
        {
            var list = _subs.ToList();
            _subs.Clear();

            for (var index = list.Count - 1; index >= 0; index--)
                list[index].Dispose();
        }

        public void Add(IDisposable d)
        {
            _subs.Add(d);
        }

        public bool Remove(IDisposable d)
        {
            if (!_subs.Remove(d))
                return false;

            d.Dispose();
            return true;
        }

        public IEnumerator GetEnumerator()
        {
            return _subs.GetEnumerator();
        }
    }
}