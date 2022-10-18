using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Valkyrie.MVVM.Bindings
{
    public class Template : DisposableUnityComponent
    {
#pragma warning disable 649
        [SerializeField] private string _templateClass;
        private object _viewModel;
#pragma warning restore 649

        public object ViewModel
        {
            get => _viewModel;
            set
            {
                _viewModel = value;
                if (_viewModel is IDisposable disposable)
                    Add(disposable);
            }
        }

        static List<Type> _cachedTypes;

        private List<Type> Types => _cachedTypes ??= typeof(object).GetAllSubTypes(u =>
            !u.IsAbstract && u.GetCustomAttribute<BindingAttribute>(false) != null);

        public Type GetTemplateType() => Types.Find(u => u.FullName == _templateClass);
    }
}