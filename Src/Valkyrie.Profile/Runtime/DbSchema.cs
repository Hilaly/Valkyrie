using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Valkyrie.Profile
{
    class DbSchema
    {
        private readonly List<Action<object, SerializationContext>> _deserializeCalls = new();
        private readonly List<Action<object, SerializationContext>> _serializeCalls = new();

        public Type Type { get; }

        private List<DbTableDesc> Tables { get; } = new();

        public DbSchema(Type type)
        {
            Type = type;
            Fill();
        }

        private void Fill()
        {
            var properties = Type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic
                                                                    | BindingFlags.Instance
                                                                    | BindingFlags.DeclaredOnly);

            foreach (var propertyInfo in properties)
            {
                var propType = propertyInfo.PropertyType;
                if (typeof(IEnumerable).IsAssignableFrom(propType))
                {
                    if (!propType.IsGenericType || propType.GetGenericTypeDefinition() != typeof(List<>))
                        Debug.LogWarning(
                            $"[PROFILE]: Only supports List<> collections, property {propertyInfo.Name} ignored");
                    else
                    {
                        var tableType = propType.GetGenericArguments()[0];
                        if (IsValidTableType(tableType, out var errorStr))
                        {
                            var table = GetOrCreate(tableType);
                            _deserializeCalls.Add((instance, ctx) =>
                            {
                                var exist = propertyInfo.GetValue(instance);
                                if (exist == null)
                                    propertyInfo.SetValue(instance, exist = Activator.CreateInstance(propType));
                                else
                                    ((IList)exist).Clear();
                                var collection = ctx.Get(table);
                                foreach (var pair in collection)
                                    ((IList)exist).Add(pair.Value);
                            });
                            _serializeCalls.Add((instance, ctx) =>
                            {
                                var exist = propertyInfo.GetValue(instance);
                                if(exist == null)
                                    return;
                                foreach (var o in ((IList)exist)) 
                                    ctx.Add(table, o);
                            });
                        }
                        else
                            Debug.LogWarning(
                                $"[PROFILE]: {tableType} is not valid table type, {errorStr}, property {propertyInfo.Name} ignored");
                    }

                    continue;
                }
            }
        }

        DbTableDesc GetOrCreate(Type tableType)
        {
            var attribute = tableType.GetCustomAttribute<TableAttribute>();
            var name = attribute?.Name ?? tableType.Name;
            var r = Tables.Find(x => x.Name == name);
            if (r == null)
                Tables.Add(r = new DbTableDesc(name, tableType, 
                    GetId(tableType),
                    GetDeser(tableType), GetSer(tableType)));
            else if (r.Type != tableType)
                throw new NotSupportedException(
                    $"Table {name} supports {r.Type.FullName}, please specify table name for {tableType.FullName}");
            return r;
        }

        private bool IsValidTableType(Type type, out string errString)
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

        public string Serialize(object dbContext)
        {
            var ctx = new SerializationContext();

            foreach (var call in _serializeCalls)
                call(dbContext, ctx);

            return ctx.Serialize(Tables);
        }

        public void Deserialize(object dbContext, string strData)
        {
            var ctx = new SerializationContext(strData);

            foreach (var call in _deserializeCalls)
                call(dbContext, ctx);
        }

        static Func<DbTableDesc, object, object> GetId(Type type)
        {
            var idPropInfo = type.GetProperty("Id",
                                 BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty |
                                 BindingFlags.SetProperty)
                             ?? type.GetProperty($"{type.Name}Id",
                                 BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty |
                                 BindingFlags.SetProperty);

            object ToByte(DbTableDesc table, object instance)
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
            object ToSByte(DbTableDesc table, object instance)
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
            object ToShort(DbTableDesc table, object instance)
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
            object ToUShort(DbTableDesc table, object instance)
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
            object ToInt(DbTableDesc table, object instance)
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
            object ToUInt(DbTableDesc table, object instance)
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
            object ToLong(DbTableDesc table, object instance)
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
            object ToULong(DbTableDesc table, object instance)
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
            Func<DbTableDesc, object, object> converter;
            if (t == typeof(byte))
                converter = ToByte;
            else if (t == typeof(sbyte))
                converter = ToSByte;
            else if (t == typeof(short))
                converter = ToShort;
            else if (t == typeof(ushort))
                converter = ToUShort;
            else if (t == typeof(int))
                converter = ToInt;
            else if (t == typeof(uint))
                converter = ToUInt;
            else if (t == typeof(long))
                converter = ToLong;
            else if (t == typeof(ulong))
                converter = ToULong;
            else
                throw new NotSupportedException($"Type {t.Name} is not supported as id");
            return converter;
        }

        static Func<string, object> GetDeser(Type type)
        {
            var setters = new List<Action<object, JObject>>();
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.Public);
            foreach (var propertyInfo in properties)
            {
                var m = GetDeserMethod(propertyInfo);
                if (m != null)
                    setters.Add(m);
            }

            object FactoryMethod(string json)
            {
                var jo = JObject.Parse(json);
                var r = Activator.CreateInstance(type);
                foreach (var setter in setters) setter(r, jo);
                return r;
            }

            return FactoryMethod;
        }

        private Func<object,string> GetSer(Type type)
        {
            var getters = new List<Action<object, JObject>>();
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public);
            foreach (var propertyInfo in properties)
            {
                var m = GetSerMethod(propertyInfo);
                if (m != null)
                    getters.Add(m);
            }

            string FactoryMethod(object o)
            {
                var jo = new JObject();
                foreach (var getter in getters) getter(o, jo);
                return jo.ToString();
            }

            return FactoryMethod;
        }

        private Action<object,JObject> GetSerMethod(PropertyInfo propertyInfo)
        {
            return GetTypedSerMethod<byte>(propertyInfo)
                   ?? GetTypedSerMethod<sbyte>(propertyInfo)
                   ?? GetTypedSerMethod<short>(propertyInfo)
                   ?? GetTypedSerMethod<ushort>(propertyInfo)
                   ?? GetTypedSerMethod<int>(propertyInfo)
                   ?? GetTypedSerMethod<uint>(propertyInfo)
                   ?? GetTypedSerMethod<long>(propertyInfo)
                   ?? GetTypedSerMethod<ulong>(propertyInfo)
                   ?? GetTypedSerMethod<string>(propertyInfo)
                   ?? GetTypedSerMethod<float>(propertyInfo)
                   ?? GetTypedSerMethod<double>(propertyInfo)
                ;
        }

        private static Action<object, JObject> GetDeserMethod(PropertyInfo propertyInfo)
        {
            return GetTypedDeserMethod<byte>(propertyInfo)
                   ?? GetTypedDeserMethod<sbyte>(propertyInfo)
                   ?? GetTypedDeserMethod<short>(propertyInfo)
                   ?? GetTypedDeserMethod<ushort>(propertyInfo)
                   ?? GetTypedDeserMethod<int>(propertyInfo)
                   ?? GetTypedDeserMethod<uint>(propertyInfo)
                   ?? GetTypedDeserMethod<long>(propertyInfo)
                   ?? GetTypedDeserMethod<ulong>(propertyInfo)
                   ?? GetTypedDeserMethod<string>(propertyInfo)
                   ?? GetTypedDeserMethod<float>(propertyInfo)
                   ?? GetTypedDeserMethod<double>(propertyInfo)
                ;
        }

        static Action<object, JObject> GetTypedDeserMethod<T>(PropertyInfo propertyInfo)
        {
            var it = propertyInfo.PropertyType;
            if (it == typeof(T))
                return ((instance, jo) =>
                {
                    var token = jo[propertyInfo.Name];
                    if (token != null)
                        propertyInfo.SetValue(instance, token.Value<T>());
                });
            return null;
        }

        static Action<object, JObject> GetTypedSerMethod<T>(PropertyInfo propertyInfo)
        {
            var it = propertyInfo.PropertyType;
            if (it == typeof(T))
                return ((instance, jo) =>
                {
                    var value = propertyInfo.GetValue(instance);
                    if (value != null)
                        jo.Add(new JProperty(propertyInfo.Name, value));
                });
            return null;
        }
    }
}