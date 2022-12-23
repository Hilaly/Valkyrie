using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;
using Valkyrie.Playground;

namespace Prototype.TryEvents
{
    public static class CExtensions
    {
        internal static IRootContext SubscribeAllEvents(this IRootContext rootContext, GameObject o)
        {
            foreach (var behaviour in o.GetComponentsInChildren<MonoBehaviour>())
                SubscribeAllEvents(rootContext, behaviour);
            return rootContext;
        }

        private static IRootContext SubscribeAllEvents(this IRootContext rootContext, MonoBehaviour o)
        {
            var t = o.GetType();
            var properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
            foreach (var propertyInfo in properties.Where(x => typeof(UnityEvent).IsAssignableFrom(x.PropertyType)))
            {
                var eventName =
                    $"{t.FullName}.{propertyInfo.Name}?instance={o.name}&entity={o.GetComponentInParent<IHasId>().Id}";

                var componentEvent = (UnityEvent)propertyInfo.GetValue(o, null);
                componentEvent.Subscribe(() => { rootContext.RaiseEvent(eventName); }).AttachTo(o.gameObject);
            }

            return rootContext;
        }

        public static IDisposable WhenClickOnButton(this IRootContext rootContext,
            string buttonName, string windowName, Action callback)
        {
            var evHandler = new EventHandler($"{typeof(Button).FullName}.{nameof(Button.onClick)}", callback)
                .AddArgument("instance", buttonName)
                .AddArgument("entity", windowName);

            return rootContext.AddHandler(evHandler);
        }
    }
}