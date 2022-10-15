using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;


namespace Valkyrie.MVVM.Bindings
{
    public abstract class AbstractBindingComponent : MonoBehaviour
    {
        internal static Func<object> GetModelMethod(GameObject go, string modelTypeName, out GameObject disposeHandler)
        {
            try
            {
                foreach (var component in go.GetComponentsInParent<Component>(true))
                {
                    if (component is Template template && template.GetTemplateType().Name == modelTypeName)
                    {
                        disposeHandler = component.gameObject;
                        return () => template.ViewModel;
                    }

                    if (component is FieldBinding fieldBinding && fieldBinding.GetTemplateType().Name == modelTypeName)
                    {
                        disposeHandler = component.gameObject;
                        return fieldBinding.GetModel;
                    }

                    if (component.GetType().Name == modelTypeName)
                    {
                        disposeHandler = component.gameObject;
                        return () => component;
                    }
                }

                throw new Exception($"Can not find model {modelTypeName}");
            }
            catch (Exception e)
            {
                Debug.LogException(e, go);

                throw new Exception($"Can not find model {modelTypeName}", e);
            }
        }

        internal static object GetModel(GameObject go, string modelTypeName, out GameObject disposeHandler) => GetModelMethod(go, modelTypeName, out disposeHandler)();

        internal static bool SplitTypeProperty(string fullText, out string typeName, out string propertyName)
        {
            var colonIndex = fullText.IndexOf(':');
            var text = colonIndex >= 0 ? fullText.Substring(0, colonIndex) : fullText;
            var fullNameParts = text.Split(new[] {'/', '.'});
            if (fullNameParts.Length > 1)
            {
                typeName = fullNameParts[^2];
                propertyName = fullNameParts[^1];
                return true;
            }
            else
            {
                typeName = propertyName = string.Empty;
                return false;
            }
        }

        private static List<Type> _adapters;

        protected Bind BindViewModelProperty(string viewModelProperty, string viewModelChangeEventName,
            string adapterType, out GameObject disposeHandler)
        {
            SplitTypeProperty(viewModelProperty, out var typeName, out var propertyName);
            var viewModelMethod = GetModelMethod(gameObject, typeName, out disposeHandler);

            var binding = DataExtensions.CreateBinding(viewModelMethod, propertyName, adapterType, viewModelChangeEventName);
            return binding;
        }
    }
}