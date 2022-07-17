using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Valkyrie.MVVM.Bindings
{
    public abstract class TemplateSelector : AbstractBindingComponent
    {
#pragma warning disable 649
        [SerializeField] protected string _viewModelProperty;
        [SerializeField] private GameObject _templateSelector;

        [SerializeField] protected bool _isPolling = true;
#pragma warning restore 649

        readonly List<GameObject> _templates = new List<GameObject>();

        void Awake()
        {
            bool IsTemplate(GameObject o)
            {
                return o.GetComponents<Template>().Length > 0;
            }

            if (IsTemplate(_templateSelector))
            {
                _templates.Add(_templateSelector);
                _templateSelector.gameObject.SetActive(false);
            }

            for (var i = 0; i < _templateSelector.transform.childCount; ++i)
            {
                var child = _templateSelector.transform.GetChild(i).gameObject;
                if (IsTemplate(child))
                {
                    _templates.Add(child);
                    child.gameObject.SetActive(false);
                }
            }

            //Start();
        }

        void Start()
        {
            var binding = BindViewModelProperty(_viewModelProperty, null, string.Empty, out var disposeHandler);

            this.SetBinding("ViewModelProperty", binding);

            if (_isPolling)
                DataExtensions.RunPolling(disposeHandler, () =>
                {
                    if (this != null && gameObject != null && gameObject.activeInHierarchy)
                        binding.Update();
                });
        }

        public abstract bool IsValidViewModelProperty(PropertyInfo info);

        static bool FindComponent(GameObject o, Type viewModelType)
        {
            var us = o.GetComponents<Template>();
            for (var index = 0; index < us.Length; index++)
            {
                var u = us[index];
                if (u.GetTemplateType() == viewModelType)
                    return true;
            }

            return false;
        }

        GameObject SelectTemplate(Type viewModelType)
        {
            foreach (var template in _templates)
            {
                if (FindComponent(template, viewModelType))
                    return template;
            }

            if (FindComponent(_templateSelector, viewModelType))
                return _templateSelector;

            for (var i = 0; i < _templateSelector.transform.childCount; ++i)
            {
                var child = _templateSelector.transform.GetChild(i).gameObject;
                if (FindComponent(child, viewModelType))
                    return child;
            }

            throw new Exception($"Can not find template for {viewModelType.Name}");
        }

        protected GameObject SpawnTemplate(object viewModel)
        {
            var template = SelectTemplate(viewModel.GetType());
            var instance = Instantiate(template, transform);
            instance.GetComponent<Template>().ViewModel = viewModel;
            instance.SetActive(true);
            return instance;
        }

        protected GameObject RemoveTemplate(GameObject o)
        {
            Debug.Assert(o.transform.parent == transform);
            Destroy(o);
            return null;
        }

        public abstract object ViewModelProperty { set; }
    }
}