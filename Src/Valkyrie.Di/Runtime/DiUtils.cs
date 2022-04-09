using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Valkyrie.Di
{
    static class DiUtils
    {
        public static IEnumerable<Type> GetResolvedTypes(this Type type)
        {
            foreach (var @interface in type.GetInterfaces())
                yield return @interface;

            var temp = type;
            while (temp != null)
            {
                yield return temp;
                temp = temp.BaseType;
            }
        }

        public static object Invoke(this IContainer container, MethodInfo methodInfo, object instance,
            params object[] args)
        {
            var ra = ((Container)container).StartResolving(args);
            return MakeMethodInvokeAction(methodInfo.DeclaringType, methodInfo)
                .Invoke(ra, instance);
        }

        private static bool TryGet(ResolvingArguments args, Type type, string name, out object result)
        {
            foreach (var argumentInfo in args.ResolvedArguments)
            {
                if (!argumentInfo.ResolvedAs.Contains(type) || argumentInfo.Name != name)
                    continue;

                result = argumentInfo.Argument;
                return true;
            }

            result = args.Container.TryResolve(args, type, name);
            if (result == null && type.IsConstructedGenericType &&
                type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                result = args.Container.ResolveAll(type.GetGenericArguments()[0]);
            return result != null;
        }

        private static readonly Dictionary<Type, Action<ResolvingArguments, object>> InjectActionsCash = new();

        public static Action<ResolvingArguments, object> MakeInjectionAction(Type instanceType)
        {
            if (InjectActionsCash.TryGetValue(instanceType, out var existAction))
                return existAction;

            var fields = GetFields(instanceType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(u => u.GetCustomAttribute<InjectAttribute>(true) != null).ToArray();
            var fieldsNames = new string[fields.Length];
            var fieldOptional = new bool[fields.Length];
            for (var i = 0; i < fields.Length; ++i)
            {
                var attr = fields[i].GetCustomAttribute<InjectAttribute>(true);
                fieldsNames[i] = attr.Name;
                fieldOptional[i] = attr.IsOptional;
            }

            var properties = GetProperties(instanceType,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(u => u.GetCustomAttribute<InjectAttribute>(true) != null).ToArray();
            var propertiesNames = new string[properties.Length];
            var propertiesOptional = new bool[properties.Length];
            for (var i = 0; i < properties.Length; ++i)
            {
                var attr = properties[i].GetCustomAttribute<InjectAttribute>(true);
                propertiesNames[i] = attr.Name;
                propertiesOptional[i] = attr.IsOptional;
            }

            var methods = GetMethods(instanceType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(u => u.GetCustomAttribute<InjectAttribute>(true) != null).ToArray();
            var methodInvokes = new Func<ResolvingArguments, object, object>[methods.Length];
            for (var i = 0; i < methods.Length; ++i)
                methodInvokes[i] = MakeMethodInvokeAction(instanceType, methods[i]);

            void InjectMethod(ResolvingArguments args, object instance)
            {
                for (var i = 0; i < fields.Length; ++i)
                    if (TryGet(args, fields[i].FieldType, fieldsNames[i], out var arg) || fieldOptional[i])
                        fields[i].SetValue(instance, arg);
                    else
                        throw new Exception(
                            $"Can not resolve argument of type {fields[i].FieldType.FullName} when inject into field {fields[i].Name} of {instanceType.FullName}");

                for (var i = 0; i < properties.Length; ++i)
                    if (TryGet(args, properties[i].PropertyType, propertiesNames[i], out var arg) ||
                        propertiesOptional[i])
                        properties[i].SetValue(instance, arg, null);
                    else
                        throw new Exception(
                            $"Can not resolve argument of type {properties[i].PropertyType.FullName} when inject into property {properties[i].Name} of {instanceType.FullName}");

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < methodInvokes.Length; ++i)
                    methodInvokes[i].Invoke(args, instance);
            }

            InjectActionsCash.Add(instanceType, existAction = InjectMethod);
            return existAction;
        }

        private static Func<ResolvingArguments, object, object> MakeMethodInvokeAction(Type instanceType,
            MethodInfo methodInfo)
        {
            var paramInfos = methodInfo.GetParameters();

            var names = new string[paramInfos.Length];
            var methodArgs = new object[paramInfos.Length];
            var methodOptional = new bool[paramInfos.Length];
            for (var i = 0; i < paramInfos.Length; ++i)
            {
                var attr = paramInfos[i].GetCustomAttribute<InjectAttribute>(true);
                names[i] = attr?.Name;
                methodOptional[i] = attr?.IsOptional ?? false;
            }

            object FactoryMethod(ResolvingArguments args, object instance)
            {
                for (var i = 0; i < paramInfos.Length; ++i)
                    if (TryGet(args, paramInfos[i].ParameterType, names[i], out var arg) || methodOptional[i])
                        methodArgs[i] = arg;
                    else
                        throw new Exception(
                            $"Can not resolve argument of type {paramInfos[i].ParameterType.FullName} when invoke {methodInfo.Name} of {instanceType.FullName}");

                return methodInfo.Invoke(instance, methodArgs);
            }

            return FactoryMethod;
        }

        public static IEnumerable<FieldInfo> GetFields(Type type, BindingFlags flags)
        {
            foreach (var propInfo in type.GetFields(flags))
                yield return propInfo;
            if (type.BaseType == null) yield break;
            foreach (var propInfo in GetFields(type.BaseType, BindingFlags.NonPublic | flags).Where(u => u.IsPrivate))
                yield return propInfo;
        }

        private static IEnumerable<PropertyInfo> GetProperties(Type type, BindingFlags flags)
        {
            foreach (var propInfo in type.GetProperties(flags))
                yield return propInfo;
            if (type.BaseType == null) yield break;
            foreach (var propInfo in GetProperties(type.BaseType, BindingFlags.NonPublic | flags))
                yield return propInfo;
        }

        private static IEnumerable<MethodInfo> GetMethods(Type type, BindingFlags flags)
        {
            foreach (var methodInfo in type.GetMethods(flags))
                yield return methodInfo;
            if (type.BaseType == null) yield break;
            foreach (var methodInfo in GetMethods(type.BaseType, BindingFlags.NonPublic | flags))
                yield return methodInfo;
        }

        public static Func<ResolvingArguments, object> MakeFactory(ConstructorInfo ctorInfo, Type instanceType)
        {
            var paramInfos = ctorInfo.GetParameters();

            var names = new string[paramInfos.Length];
            var ctorArgs = new object[paramInfos.Length];
            var ctorOptional = new bool[paramInfos.Length];
            for (var i = 0; i < paramInfos.Length; ++i)
            {
                var attr = paramInfos[i].GetCustomAttribute<InjectAttribute>(true);
                names[i] = attr?.Name;
                ctorOptional[i] = attr?.IsOptional ?? false;
            }

            object FactoryMethod(ResolvingArguments args)
            {
                for (var i = 0; i < paramInfos.Length; ++i)
                {
                    ctorArgs[i] = null;
                    var paramType = paramInfos[i].ParameterType;
                    if (!TryGet(args, paramType, names[i], out var arg) && !ctorOptional[i])
                        throw new Exception(
                            $"Can not resolve argument of type {paramType.FullName} when create instance of {instanceType.FullName}");
                    ctorArgs[i] = arg;
                }

                var result = ctorInfo.Invoke(ctorArgs);

                for (var i = 0; i < paramInfos.Length; ++i)
                    ctorArgs[i] = null;

                return result;
            }

            return FactoryMethod;
        }
    }
}