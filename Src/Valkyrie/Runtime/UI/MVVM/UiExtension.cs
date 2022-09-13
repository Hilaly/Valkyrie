using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using Utils;
using Valkyrie.Di;
using Valkyrie.Tools;
using Valkyrie.XPath;

namespace Valkyrie.MVVM
{
    public static class UiExtension
    {
        private static Dictionary<Type, Action<object, GameObject>> _autoBindTypes;
        
        public static void InjectAutoBind(this GameObject go)
        {
            var components = go.GetComponentsInChildren<Component>();
            foreach (var component in components)
                InjectAutoBind(component);
        }

        public static void InjectAutoBind(this Component component)
        {
            var componentType = component.GetType();
            var factoryMethod = GetFactoryMethod(componentType);
            if (factoryMethod != null)
                factoryMethod(component, component.gameObject);
        }

        private static Action<object, GameObject> GetFactoryMethod(Type type)
        {
            BuildAutoBindCache();

            return _autoBindTypes.TryGetValue(type, out var method) ? method : default;
        }

        private static void BuildAutoBindCache()
        {
            if (_autoBindTypes == null)
            {
                _autoBindTypes = new Dictionary<Type, Action<object, GameObject>>();
                var typesList = typeof(Component).GetAllSubTypes(x => true);
                foreach (var t in typesList)
                {
                    var method = CreateAutoBindMethod(t);
                    if (method != null)
                        _autoBindTypes.Add(t, method);
                }
            }
        }

        static Action<object, GameObject> CreateAutoBindMethod(Type type)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var allProperties = type.GetProperties(flags);
            var autoBindProperties =
                allProperties.Where(x => CustomAttributeExtensions.GetCustomAttributes<AutoBindAttribute>((MemberInfo)x).Any()).ToList();
            var allMethods = type.GetMethods(flags);
            var autoBindMethods = allMethods.Where(x => x.GetCustomAttributes<AutoBindAttribute>().Any()).ToList();
            if (autoBindProperties.Count == 0 && autoBindMethods.Count == 0)
                return default;
            var calls = new List<Action<object, GameObject>>();
            foreach (var autoBindProperty in autoBindProperties)
            {
                var attributes = autoBindProperty.GetCustomAttributes<AutoBindAttribute>();
                foreach (var bindAttribute in attributes.Where(x => x.XPath.NotNullOrEmpty()))
                {
                    var xPath = new XPath.XPath(bindAttribute.XPath);
                    var method = CreatePropertyBindingMethod(type, xPath, bindAttribute, autoBindProperty);
                    calls.Add(method);
                }
            }

            foreach (var autoBindMethod in autoBindMethods)
            {
                var attributes = autoBindMethod.GetCustomAttributes<AutoBindAttribute>();
                foreach (var bindAttribute in attributes)
                {
                    if (bindAttribute.XPath.IsNullOrEmpty())
                        continue;
                    var xPath = new XPath.XPath(bindAttribute.XPath);
                    var method = CreateEventBindingMethod(type, xPath, bindAttribute, autoBindMethod);
                    calls.Add(method);
                }
            }

            void FactoryMethod(object instance, GameObject gameObject) =>
                calls.ForEach(action => action.Invoke(instance, gameObject));

            return FactoryMethod;
        }

        private static Action<object, GameObject> CreateEventBindingMethod(Type type, XPath.XPath xPath,
            AutoBindAttribute bindAttribute, MethodInfo autoBindMethod)
        {
            return (model, go) =>
            {
                void EventHandler() => autoBindMethod.Invoke(model, null);

                var xElement = xPath.SelectSingleNode(go) as XPathMemberElement;
                if (xElement?.Value == null || !(xElement.Info is PropertyInfo info))
                {
                    Debug.LogWarning(
                        $"Couldn't find target method xPath={bindAttribute.XPath} for type {type.Name}",
                        go);
                    return;
                }

                var view = xElement.Value;
                var viewGameObject = view as GameObject ?? ((Component)view).gameObject;

                var componentEvent = (UnityEvent)info.GetValue(view);
                if (componentEvent == null || viewGameObject == null)
                {
                    Debug.LogWarning(
                        $"Couldn't find target method xPath={bindAttribute.XPath} for type {type.Name}",
                        go);
                    return;
                }

                componentEvent.Subscribe(EventHandler).AttachTo(viewGameObject);
            };
        }

        private static Action<object, GameObject> CreatePropertyBindingMethod(Type type, XPath.XPath xPath,
            AutoBindAttribute bindAttribute,
            PropertyInfo autoBindProperty)
        {
            return (model, go) =>
            {
                var xElement = xPath.SelectSingleNode(go) as XPathMemberElement;
                if (xElement?.Value == null || !(xElement.Info is PropertyInfo info))
                {
                    Debug.LogWarning(
                        $"Couldn't find target property xPath={bindAttribute.XPath} for type {type.Name}",
                        go);
                    return;
                }

                var view = xElement.Value;
                var viewGameObject = view as GameObject ?? ((Component)view).gameObject;

                var binding = model.CreateBinding(autoBindProperty.Name, bindAttribute.Adapter?.FullName);
                view.SetBinding(info.Name, binding);

                RunPolling(viewGameObject, () =>
                {
                    if (viewGameObject != null && model != null)
                        binding.Update();
                });
            };
        }

        private static GameObject Find(GameObject go, string xPath)
        {
            return go;
        }


        internal static void RunPolling(GameObject disposeHandler, Action work)
        {
            var tcs = new CancellationTokenSource();
            AsyncExtension.RunEveryUpdate(work, tcs.Token);
            new ActionDisposable(() => tcs.Cancel()).AttachTo(disposeHandler);
        }
    }
}