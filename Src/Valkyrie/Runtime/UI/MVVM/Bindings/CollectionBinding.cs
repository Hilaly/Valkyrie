using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Utils;

namespace Valkyrie.MVVM.Bindings
{
    public class CollectionBinding : TemplateSelector
    {
        readonly Dictionary<object, GameObject> _childMap = new Dictionary<object, GameObject>();
        private IEnumerable _viewModelsCollection;

        public override bool IsValidViewModelProperty(PropertyInfo info)
        {
            return typeof(IEnumerable).IsAssignableFrom(info.PropertyType);
        }

        public override object ViewModelProperty
        {
            set
            {
                _viewModelsCollection = (IEnumerable)value;

                var enumeration = _viewModelsCollection;
                if (enumeration != null)
                {
                    var collection = enumeration.Cast<object>().ToList();
                    //Remove missing
                    {
                        var toRemove = new List<object>();
                        foreach (var pair in _childMap)
                        {
                            if (!collection.Contains(pair.Key))
                                toRemove.Add(pair.Key);
                        }

                        foreach (var o in toRemove)
                        {
                            if (o is IViewOwner owner)
                                owner.View = null;

                            RemoveTemplate(_childMap[o]);
                            _childMap.Remove(o);
                        }
                    }

                    //Add new
                    foreach (var model in collection)
                    {
                        if (_childMap.ContainsKey(model))
                            continue;
                        var view = SpawnTemplate(model);
                        _childMap.Add(model, view);

                        if (model is IViewOwner owner)
                            owner.View = view;
                    }

                    //reorder
                    for (var i = 0; i < collection.Count; i++)
                        _childMap[collection[i]].transform.SetSiblingIndex(i);
                }
                else
                {
                    foreach (var o in _childMap)
                    {
                        if (o.Key is IViewOwner owner)
                            owner.View = null;
                        RemoveTemplate(o.Value);
                    }

                    _childMap.Clear();
                }
            }
        }

        void Start()
        {
            var binding = BindViewModelProperty(_viewModelProperty, null, string.Empty, out var disposeHandler);

            this.SetBinding(nameof(ViewModelProperty), binding);

            if (_isPolling)
                UiExtension.RunPolling(disposeHandler, () =>
                {
                    if (this != null && gameObject != null && gameObject.activeInHierarchy)
                        binding.Update();
                });
        }
    }
}