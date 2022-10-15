using System;
using System.Reflection;
using Valkyrie.Di;

namespace Valkyrie.Tools
{
    public static class ObjectExtension
    {
        /// <summary>
        /// Return target
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static T CopyFields<T>(this object source, T target)
        {
            return (T) CopyFields(source, (object) target);
        }

        /// <summary>
        /// Returns target
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        static object CopyFields(this object source, object target)
        {
            var sourceType = source.GetType();
            var targetType = target.GetType();

            var sourceFields = DiUtils.GetFields(sourceType,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var targetFields = DiUtils.GetFields(targetType,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var sourceField in sourceFields)
            {
                foreach (var targetField in targetFields)
                {
                    if (targetField.Name != sourceField.Name)
                        continue;

                    targetField.SetValue(target, sourceField.GetValue(source));
                    break;
                }
            }

            return target;
        }

        public static object MakeCopy(this object source)
        {
            var type = source.GetType();
            var result = Activator.CreateInstance(type);
            foreach (var field in DiUtils.GetFields(type,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                field.SetValue(result, field.GetValue(source));
            return result;
        }

        public static T MakeCopy<T>(this T source)
        {
            return (T) MakeCopy((object) source);
        }

    }
}