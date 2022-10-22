using System;
using System.Collections.Generic;

namespace Tools
{
    public static class CollectionExtensions
    {
        #region Enumerables

        public static void RemoveAtWithReplace<T>(this List<T> list, int index)
        {
            var remIndex = list.Count - 1;
            list[index] = list[remIndex];
            list.RemoveAt(remIndex);
        }

        public static void RemoveAndReplaceWithLast<T>(this IList<T> list, int index)
        {
            if (index < list.Count)
            {
                var lastIndex = list.Count - 1;
                list[index] = list[lastIndex];
                list.RemoveAt(lastIndex);
            }
        }
        
        public static void RemoveWhere<T>(this IList<T> list, Func<T, bool> expression)
        {
            for (var i = 0; i < list.Count;)
                if (expression(list[i]))
                    list.RemoveAndReplaceWithLast(i);
                else
                    ++i;
        }

        public static void RemoveWhere<T>(this IList<T> list, Func<T, bool> expression, Action<T> onRemove)
        {
            for (var i = 0; i < list.Count;)
                if (expression(list[i]))
                {
                    onRemove?.Invoke(list[i]);
                    list.RemoveAndReplaceWithLast(i);
                }
                else
                    ++i;
        }

        public static void SyncToView<TModel, TView>(this IEnumerable<TModel> models, IList<TView> views,
            Func<TModel, TView> createMethod,
            Action<TModel, TView> syncMethod,
            Action<TView> disableMethod)
        {
            var modelsCount = 0;

            foreach (var model in models)
            {
                if (views.Count > modelsCount)
                {
                    var view = views[modelsCount];
                    syncMethod(model, view);
                }
                else
                {
                    var view = createMethod(model);
                    syncMethod(model, view);
                    views.Add(view);
                }
                modelsCount++;
            }

            for (var i = views.Count - 1; i >= modelsCount; --i)
            {
                var view = views[i];
                disableMethod(view);
            }
        }

        public static void SyncToView<TModel, TView>(this IList<TModel> models, IList<TView> views,
            Func<TModel, TView> createMethod,
            Action<TModel, TView> syncMethod,
            Action<TView> disableMethod)
        {
            for (var i = 0; i < models.Count; ++i)
            {
                var model = models[i];
                if (views.Count > i)
                {
                    var view = views[i];
                    syncMethod(model, view);
                }
                else
                {
                    var view = createMethod(model);
                    syncMethod(model, view);
                    views.Add(view);
                }
            }

            for (var i = views.Count - 1; i >= models.Count; --i)
            {
                var view = views[i];
                disableMethod(view);
            }
        }

        public static void SyncToViewDeleteExtra<TModel, TView>(this IList<TModel> models, IList<TView> views,
            Func<TModel, TView> createMethod,
            Action<TModel, TView> syncMethod,
            Action<TView> onDeleteMethod = null)
        {
            for (var i = 0; i < models.Count; ++i)
            {
                var model = models[i];
                if (views.Count > i)
                {
                    var view = views[i];
                    syncMethod(model, view);
                }
                else
                {
                    var view = createMethod(model);
                    syncMethod(model, view);
                    views.Add(view);
                }
            }

            if (views.Count > models.Count)
            {
                if (onDeleteMethod != null)
                    onDeleteMethod(views[models.Count]);
                views.RemoveAndReplaceWithLast(models.Count);
            }
        }
        
        #endregion
    }
}