using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Valkyrie.Di
{
    public static class InjectExtension
    {
        public static ISingletonRegistration<T> RegisterSingleInstance<T>(this IContainer container) =>
            container.Register<T>().AsInterfacesAndSelf().SingleInstance();


        public static IConcreteTypeFactoryRegistration<T> RegisterFromComponentOnNewPrefab<T>(this IContainer container,
            GameObject prefab) where T : Component
        {
            T Factory()
            {
                var instance = Object.Instantiate(prefab).GetComponent<T>();
                var components = instance.gameObject.GetComponentsInChildren<Component>(true);
                foreach (var component in components)
                {
                    if (component == instance)
                        continue;
                    container.Inject(component);
                }

                return instance;
            }

            return container.Register(Factory);
        }

        public static IConcreteTypeFactoryRegistration<T> RegisterFromComponentOnNewPrefab<T>(this IContainer container,
            T prefab) where T : Component
        {
            T Factory()
            {
                var instance = Object.Instantiate(prefab);
                var components = instance.gameObject.GetComponentsInChildren<Component>(true);
                foreach (var component in components)
                {
                    if (component == instance)
                        continue;
                    container.Inject(component);
                }

                return instance;
            }

            return container.Register(Factory);
        }

        public static IConcreteTypeFactoryRegistration<T> RegisterFromNewComponentOnNewGameObject<T>(
            this IContainer container, string goName = null) where T : Component
        {
            return container.Register<T>(c =>
            {
                var go = !string.IsNullOrEmpty(goName) ? new GameObject(goName) : new GameObject();
                var instance = go.AddComponent<T>();
                return instance;
            });
        }

        public static IConcreteInstanceRegistration<T> RegisterFromHierarchy<T>(this IContainer container,
            Scene scene)
            where T : Component
        {
            T Factory()
            {
                //if (scene.isLoaded)
                foreach (var rootGameObject in scene.GetRootGameObjects())
                {
                    var comp = rootGameObject.GetComponentInChildren<T>();
                    if (comp != null)
                        return comp;
                }

                throw new Exception($"can not find component {typeof(T)} on scene");
            }


            return container.Register(Factory());
        }

        public static T Instantiate<T>(this IContainer container, T prefab) where T : Object
        {
            var instance = Object.Instantiate(prefab);
            if (instance is Component component)
                InjectGameObject(container, component.gameObject);
            else if (instance is GameObject go)
                InjectGameObject(container, go);
            return instance;
        }

        public static T Instantiate<T>(this IContainer container, T prefab, Transform parent) where T : Object
        {
            var instance = Object.Instantiate(prefab, parent);
            if (instance is Component component)
                InjectGameObject(container, component.gameObject);
            else if (instance is GameObject go)
                InjectGameObject(container, go);
            return instance;
        }

        public static T Instantiate<T>(this IContainer container, T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Object
        {
            var instance = Object.Instantiate(prefab, position, rotation, parent);
            if (instance is Component component)
                InjectGameObject(container, component.gameObject);
            else if (instance is GameObject go)
                InjectGameObject(container, go);
            return instance;
        }

        public static GameObject InjectGameObject(this IContainer container, GameObject o, bool includeInactive = true)
        {
            if (o.activeInHierarchy || includeInactive)
            {
                foreach (var component in o.GetComponents<Component>())
                {
                    //Skip missing mono behaviours
                    if(component == null)
                        continue;
                    
                    if (component is Behaviour beh)
                    {
                        if (beh.isActiveAndEnabled || includeInactive)
                            container.Inject(component);
                    }
                    else
                    {
                        container.Inject(component);
                    }
                }

                for (var i = 0; i < o.transform.childCount; i++)
                    container.InjectGameObject(o.transform.GetChild(i).gameObject);
            }

            return o;
        }

        public static IContainer FindContainerInScene(this GameObject gameObject)
        {
            var scene = gameObject.scene;
            foreach (var rootGameObject in scene.GetRootGameObjects())
            {
                var c = rootGameObject.GetComponentInChildren<SceneContext>();
                if (c != null)
                    return c.Container;
            }

            return ProjectContext.Instance.Container;
        }
    }
}