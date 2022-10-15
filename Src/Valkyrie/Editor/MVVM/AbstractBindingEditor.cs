using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Utils;
using Valkyrie.MVVM.Bindings;

namespace Valkyrie.MVVM.Editor
{
    public abstract class AbstractBindingEditor : UnityEditor.Editor
    {
        internal static readonly Type[] ViewPropertiesTypes = {
            typeof(bool),
            typeof(float),
            typeof(string),
            typeof(int),
            typeof(byte),
            typeof(short),
            typeof(Vector2),
            typeof(Vector3),
            typeof(Quaternion),
            typeof(Color),
            typeof(Sprite),
            typeof(Mesh)
        };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawGui();

            serializedObject.ApplyModifiedProperties();
        }

        protected abstract void DrawGui();

        #region Requesting
        
        private readonly Dictionary<GameObject, List<Type>> _viewModels = new Dictionary<GameObject, List<Type>>();
        private readonly Dictionary<Type, List<Type>> _typesCache = new Dictionary<Type, List<Type>>();
        
        IEnumerable<Type> FindAllAvailableViewModels(GameObject o)
        {
            foreach (var component in o.GetComponentsInParent<Component>(true))
            {
                if(component == null)
                    continue;
                
                var type = component.GetType();
                if (type.GetCustomAttribute<BindingAttribute>(true) != null)
                    yield return type;
                
                if (component is Template template)
                {
                    var templateType = template.GetTemplateType();
                    if (templateType != null)
                        yield return templateType;
                }

                if (component is FieldBinding fieldBinding)
                {
                    var fieldType = fieldBinding.GetTemplateType();
                    if (fieldType != null)
                        yield return fieldType;
                }
            }
        }

        protected List<Type> AllAvailableViewModels(GameObject o)
        {
            if (!_viewModels.TryGetValue(o, out var list))
            {
                list = FindAllAvailableViewModels(o).ToList();
                _viewModels.Add(o, list);
            }

            return list;
        }

        static IEnumerable<Component> FindAllAvailableViews(Component component)
        {
            return component.gameObject.GetComponents<Component>().Where(c => c != null && c != component);
        }

        protected List<Component> AllAvailableViews(Component component)
        {
            return FindAllAvailableViews(component).ToList();
        }
        
        internal static List<string> FindAllProperties(IEnumerable<Type> types, BindingFlags flags, Func<PropertyInfo, bool> filter)
        {
            var result = new List<string>();
            foreach (var type in types)
            {
                var typeName = type.FullName;
                var propertiesNames = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | flags)
                    .Where(filter).OrderBy(u => u.Name)
                    .Select(u => $"{typeName}/{u.Name}:{u.PropertyType.Name}"/*.Replace(".", "/")*/);
                foreach (var propertyName in propertiesNames)
                    if (!result.Contains(propertyName))
                        result.Add(propertyName);
            }

            return result;
        }

        protected List<Type> GetAllSubTypes<T>(Func<Type, bool> filter)
        {
            var type = typeof(T);
            if (!_typesCache.TryGetValue(type, out var list))
            {
                list = type.GetAllSubTypes(filter).ToList();
                _typesCache.Add(type, list);
            }

            return list;
        }

        static List<IBindingAdapter> _adapters;

        internal static List<IBindingAdapter> FindAdapters(string resultType)
        {
            if (_adapters == null)
                _adapters = typeof(IBindingAdapter).GetAllSubTypes(u => !u.IsAbstract)
                    .ConvertAll(u => (IBindingAdapter) Activator.CreateInstance(u));

            return _adapters.Where(u => u.GetResultType().Name == resultType).ToList();
        }
        
        internal static List<string> FindAdapterNames(string propType)
        {
            var result = FindAdapters(propType).ConvertAll(u => u.GetType().FullName);
            result.Add("None");
            return result;
        }

        #endregion
    }
}