using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Valkyrie.Di;

namespace Valkyrie.MVVM.Bindings
{
    public sealed class TypeBinding : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private string _templateClass;
        private object _viewModel;
#pragma warning restore 649
        
        [Inject] private IContainer _container;
        
        public object Model
        {
            get { return _viewModel ??= _container.Resolve(GetTemplateType()); }
            set
            {
                if (value.GetType().IsSubclassOf(GetTemplateType())) 
                    _viewModel = value;
                else
                    throw new Exception($"Type mismatch");
            }
        }

        static List<Type> _cachedTypes;

        private List<Type> Types => _cachedTypes ??= typeof(object).GetAllSubTypes(u =>
            !u.IsAbstract && u.GetCustomAttribute<BindingAttribute>(false) != null);

        public Type GetTemplateType() => Types.Find(u => u.FullName == _templateClass);
    }
}