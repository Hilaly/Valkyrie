using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Valkyrie.Profile
{
    class SerializationUtils
    {
        public static bool IsSupportedClass(Type type, out string errString)
        {
            if (!type.IsClass)
            {
                errString = "must be a class";
                return false;
            }

            if (type.IsAbstract)
            {
                errString = "couldn't be abstract";
                return false;
            }

            if (type.GetConstructor(Type.EmptyTypes) == null)
            {
                errString = "must have parameterless constructor";
                return false;
            }

            var idPropInfo = type.GetProperty("Id",
                                 BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty |
                                 BindingFlags.SetProperty)
                             ?? type.GetProperty($"{type.Name}Id",
                                 BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty |
                                 BindingFlags.SetProperty);
            if (idPropInfo == null)
            {
                errString = $"must have Id or {type.Name}Id property with public setter and getter";
                return false;
            }

            var idPropType = idPropInfo.PropertyType;
            if (idPropType != typeof(byte)
                && idPropType != typeof(sbyte)
                && idPropType != typeof(short)
                && idPropType != typeof(ushort)
                && idPropType != typeof(int)
                && idPropType != typeof(uint)
                && idPropType != typeof(long)
                && idPropType != typeof(ulong)
            )
            {
                errString = $"id property must be of integral type";
                return false;
            }

            errString = string.Empty;
            return true;
        }

        public static string GetTableName(Type tableType)
        {
            var attribute = tableType.GetCustomAttribute<TableAttribute>();
            var name = attribute?.Name ?? tableType.Name;
            return name;
        }

        public static Func<TypeSerializationInfo, object, object> GetIdMethod(Type type, out Type idType)
        {
            var idPropInfo = type.GetProperty("Id",
                                 BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty |
                                 BindingFlags.SetProperty)
                             ?? type.GetProperty($"{type.Name}Id",
                                 BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty |
                                 BindingFlags.SetProperty);

            object ToByte(TypeSerializationInfo table, object instance)
            {
                var v = (byte)idPropInfo.GetValue(instance);
                if (v == 0)
                {
                    var lv = table.Id++;
                    v = (byte)lv;
                    idPropInfo.SetValue(instance, v);
                }

                return v;
            }

            object ToSByte(TypeSerializationInfo table, object instance)
            {
                var v = (sbyte)idPropInfo.GetValue(instance);
                if (v == 0)
                {
                    var lv = table.Id++;
                    v = (sbyte)lv;
                    idPropInfo.SetValue(instance, v);
                }

                return v;
            }

            object ToShort(TypeSerializationInfo table, object instance)
            {
                var v = (short)idPropInfo.GetValue(instance);
                if (v == 0)
                {
                    var lv = table.Id++;
                    v = (short)lv;
                    idPropInfo.SetValue(instance, v);
                }

                return v;
            }

            object ToUShort(TypeSerializationInfo table, object instance)
            {
                var v = (ushort)idPropInfo.GetValue(instance);
                if (v == 0)
                {
                    var lv = table.Id++;
                    v = (ushort)lv;
                    idPropInfo.SetValue(instance, v);
                }

                return v;
            }

            object ToInt(TypeSerializationInfo table, object instance)
            {
                var v = (int)idPropInfo.GetValue(instance);
                if (v == 0)
                {
                    var lv = table.Id++;
                    v = (int)lv;
                    idPropInfo.SetValue(instance, v);
                }

                return v;
            }

            object ToUInt(TypeSerializationInfo table, object instance)
            {
                var v = (uint)idPropInfo.GetValue(instance);
                if (v == 0)
                {
                    var lv = table.Id++;
                    v = (uint)lv;
                    idPropInfo.SetValue(instance, v);
                }

                return v;
            }

            object ToLong(TypeSerializationInfo table, object instance)
            {
                var v = (long)idPropInfo.GetValue(instance);
                if (v == 0)
                {
                    var lv = table.Id++;
                    v = (long)lv;
                    idPropInfo.SetValue(instance, v);
                }

                return v;
            }

            object ToULong(TypeSerializationInfo table, object instance)
            {
                var v = (ulong)idPropInfo.GetValue(instance);
                if (v == 0)
                {
                    var lv = table.Id++;
                    v = (ulong)lv;
                    idPropInfo.SetValue(instance, v);
                }

                return v;
            }

            var t = idPropInfo.PropertyType;
            Func<TypeSerializationInfo, object, object> converter;
            if (t == typeof(byte))
            {
                idType = typeof(byte);
                converter = ToByte;
            }
            else if (t == typeof(sbyte))
            {
                idType = typeof(sbyte);
                converter = ToSByte;
            }
            else if (t == typeof(short))
            {
                idType = typeof(short);
                converter = ToShort;
            }
            else if (t == typeof(ushort))
            {
                idType = typeof(ushort);
                converter = ToUShort;
            }
            else if (t == typeof(int))
            {
                idType = typeof(int);
                converter = ToInt;
            }
            else if (t == typeof(uint))
            {
                idType = typeof(uint);
                converter = ToUInt;
            }
            else if (t == typeof(long))
            {
                idType = typeof(long);
                converter = ToLong;
            }
            else if (t == typeof(ulong))
            {
                idType = typeof(ulong);
                converter = ToULong;
            }
            else
                throw new NotSupportedException($"Type {t.Name} is not supported as id");

            return converter;
        }

        public static bool IsSimpleSupportedType(Type type)
        {
            return
                type == typeof(byte)
                || type == typeof(sbyte)
                || type == typeof(short)
                || type == typeof(ushort)
                || type == typeof(int)
                || type == typeof(uint)
                || type == typeof(long)
                || type == typeof(ulong)
                || type == typeof(string)
                || type == typeof(float)
                || type == typeof(double)
                ;
        }

        static Action<object, JObject> GetSerializeMethod(SerializationData serRoot, PropertyInfo propertyInfo,
            out string errString)
        {
            var propType = propertyInfo.PropertyType;
            if (typeof(IEnumerable).IsAssignableFrom(propType))
            {
                if (!propType.IsGenericType || propType.GetGenericTypeDefinition() != typeof(List<>))
                {
                    errString = "supports only List<> collections";
                    return default;
                }

                var enumType = propType.GetGenericArguments()[0];
                if (IsSimpleSupportedType(enumType))
                {
                    errString = string.Empty;
                    return GetSimpleSerializeMethod(propertyInfo);
                }

                if (!IsSupportedClass(enumType, out errString))
                    return default;

                return GetReferenceListSerializeMethod(serRoot, propertyInfo);
            }

            if (IsSimpleSupportedType(propType))
            {
                errString = string.Empty;
                return GetSimpleSerializeMethod(propertyInfo);
            }

            if (!IsSupportedClass(propType, out errString))
                return default;

            return GetReferenceSerializeMethod(serRoot, propertyInfo);
        }

        private static Action<object, JObject> GetDeserializeMethod(SerializationData serRoot, PropertyInfo propertyInfo, out string errString)
        {
            var propType = propertyInfo.PropertyType;
            if (typeof(IEnumerable).IsAssignableFrom(propType))
            {
                if (!propType.IsGenericType || propType.GetGenericTypeDefinition() != typeof(List<>))
                {
                    errString = "supports only List<> collections";
                    return default;
                }

                var enumType = propType.GetGenericArguments()[0];
                if (IsSimpleSupportedType(enumType))
                {
                    errString = string.Empty;
                    return GetSimpleDeserializeMethod(propertyInfo);
                }

                if (!IsSupportedClass(enumType, out errString))
                    return default;

                return GetReferenceListDeserializeMethod(serRoot, propertyInfo);
            }
            
            errString = string.Empty;
            if (IsSimpleSupportedType(propType))
            {
                return GetSimpleDeserializeMethod(propertyInfo);
            }

            errString = string.Empty;
            return (o, jo) => { propertyInfo.SetValue(o, default); };
        }
        
        static Action<object, JObject> GetReferenceListSerializeMethod(SerializationData serRoot,
            PropertyInfo propertyInfo)
        {
            var propType = propertyInfo.PropertyType;
            var enumType = propType.GetGenericArguments()[0];
            var table = serRoot.GetTypeInfo(enumType);
            var referenceList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(table.IdType));

            return (instance, jo) =>
            {
                var sourceList = (IList)propertyInfo.GetValue(instance);
                if (sourceList == null || sourceList.Count == 0)
                    return;
                referenceList.Clear();
                for (int i = 0; i < sourceList.Count; i++)
                {
                    var refObject = sourceList[i];
                    if (refObject == null)
                        referenceList.Add(0);
                    else
                        referenceList.Add(table.GetId(refObject));
                }

                jo.Add(new JProperty(propertyInfo.Name, referenceList));
            };
        }
        static Action<object, JObject> GetReferenceListDeserializeMethod(SerializationData serRoot,
            PropertyInfo propertyInfo) =>
            (instance, jo) =>
            {
                if(propertyInfo.GetValue(instance) == null)
                    propertyInfo.SetValue(instance, Activator.CreateInstance(propertyInfo.PropertyType));
            };

        static Action<object, JObject> GetReferenceSerializeMethod(SerializationData serRoot, PropertyInfo propertyInfo)
        {
            var propType = propertyInfo.PropertyType;
            var table = serRoot.GetTypeInfo(propType);

            return (instance, jo) =>
            {
                var refObject = propertyInfo.GetValue(instance);
                if (refObject != null)
                    jo.Add(new JProperty(propertyInfo.Name, table.GetId(refObject)));
            };
        }

        static Action<object, JObject> GetSimpleSerializeMethod(PropertyInfo propertyInfo) =>
            (instance, jo) =>
            {
                var value = propertyInfo.GetValue(instance);
                if (value != null)
                    jo.Add(new JProperty(propertyInfo.Name, value));
            };
        
        static Action<object, JObject> GetSimpleDeserializeMethod(PropertyInfo propertyInfo) =>
            (instance, jo) =>
            {
                if (jo.TryGetValue(propertyInfo.Name, out var token))
                    propertyInfo.SetValue(instance, token.ToObject(propertyInfo.PropertyType));
            };

        public static Func<TypeSerializationInfo, object, JObject> SerializeMethod(SerializationData serRoot, Type type)
        {
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public);

            var calls = new List<Action<object, JObject>>();

            foreach (var propertyInfo in properties)
            {
                var m = GetSerializeMethod(serRoot, propertyInfo, out var errString);
                if (m != null)
                    calls.Add(m);
                else
                    Debug.LogWarning($"[PROFILE]: {errString}, property {type.FullName}.{propertyInfo.Name} ignored");
            }

            JObject FactoryMethod(TypeSerializationInfo typeInfo, object o)
            {
                var jo = new JObject();
                foreach (var call in calls) call(o, jo);
                return jo;
            }

            return FactoryMethod;
        }

        public static Action<TypeSerializationInfo,JObject,object> DeserializeMethod(SerializationData serRoot, Type type)
        {
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.Public);

            var calls = new List<Action<object, JObject>>();

            foreach (var propertyInfo in properties)
            {
                var m = GetDeserializeMethod(serRoot, propertyInfo, out var errString);
                if (m != null)
                    calls.Add(m);
                else
                    Debug.LogWarning($"[PROFILE]: {errString}, property {type.FullName}.{propertyInfo.Name} ignored");
            }
            
            void FactoryMethod(TypeSerializationInfo typeInfo, JObject json, object o)
            {
                foreach (var call in calls) call(o, json);
            }

            return FactoryMethod;
        }

    }
}