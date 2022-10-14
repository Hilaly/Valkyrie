using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using Valkyrie.Di;
using Valkyrie.Tools;
using Random = System.Random;

namespace Utils
{
    public static class DataExtensions
    {
        #region Naming

        public static string ConvertToCamelCasePropertyName(this string original)
        {
            var sb = new StringBuilder();

            var parts = original.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                sb.Append(part.Substring(0, 1).ToUpperInvariant());
                if (part.Length > 1)
                    sb.Append(part.Substring(1));
            }

            return sb.ToString();
        }

        public static string ConvertToUnityPropertyName(this string original)
        {
            var sb = new StringBuilder();

            var parts = original.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
            for (var index = 0; index < parts.Length; index++)
            {
                var part = parts[index];
                sb.Append(index > 0
                    ? part.Substring(0, 1).ToUpperInvariant()
                    : part.Substring(0, 1).ToLowerInvariant());
                if (part.Length > 1)
                    sb.Append(part.Substring(1));
            }

            return sb.ToString();
        }

        public static string ConvertToCamelCaseFieldName(this string original)
        {
            var sb = new StringBuilder("_");

            var parts = original.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
            for (var index = 0; index < parts.Length; index++)
            {
                var part = parts[index];
                sb.Append(index > 0
                    ? part.Substring(0, 1).ToUpperInvariant()
                    : part.Substring(0, 1).ToLowerInvariant());
                if (part.Length > 1)
                    sb.Append(part.Substring(1));
            }

            return sb.ToString();
        }

        #endregion

        #region Strings

        private static HashAlgorithm _hashAlgorithm;

        private static string ToHashString(byte[] data)
        {
            // Create a new StringBuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder(data.Length * 2);

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (var i = 0; i < data.Length; i++)
                sBuilder.Append(BitConverter.ToString(data, i, 1));

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        static HashAlgorithm HashAlgorithm => _hashAlgorithm ?? (_hashAlgorithm = SHA1.Create());

        private static string GetHash(HashAlgorithm hashAlgorithm, string input)
        {
            // Convert the input string
            // to a byte array and compute the hash.
            return GetHash(hashAlgorithm, Encoding.UTF8.GetBytes(input));
        }

        private static string GetHash(HashAlgorithm hashAlgorithm, byte[] input)
        {
            //compute the hash.
            return ToHashString(hashAlgorithm.ComputeHash(input));
        }

        public static string ComputeHash(this string source)
        {
            return source.Length < 20
                ? source
                : GetHash(HashAlgorithm, source);
        }

        public static string ComputeHash(this byte[] source)
        {
            return GetHash(HashAlgorithm, source);
        }

        #endregion

        #region Type

        public static List<Type> GetAllSubTypes(this Type aBaseClass, Func<Type, bool> where)
        {
            var result = new List<Type>
            {
                aBaseClass
            };
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var assemblyTypes = assembly.GetTypes();
                    var selectedTypes = assemblyTypes
                        .Where(typ => typ.IsSubclassOf(aBaseClass) || aBaseClass.IsAssignableFrom(typ)).ToArray();
                    result.AddRange(selectedTypes);
                }
                catch
                {
                    //Do nothing if we got to assembly that probably not from this project
                }
            }

            return where != null ? result.Where(where).ToList() : result;
        }

        public static T GetCustomAttribute<T>(this ICustomAttributeProvider type, bool inherit) where T : Attribute
        {
            var attrs = type.GetCustomAttributes(typeof(T), inherit);
            if (attrs.Length > 0)
                return (T)attrs[0];
            return default(T);
        }

        #endregion

        #region Json

        public static string ToJson<T>(this T o)
        {
            return JsonConvert.SerializeObject(o);
        }

        public static T ToObject<T>(this string json)
        {
            var result = JsonConvert.DeserializeObject<T>(json);
            return result;
        }

        public static bool IsValidJson<T>(this string json)
        {
            try
            {
                return ToObject<T>(json) != null;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        #endregion

        #region Xml

        public static string ToXml<T>(this T o)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var targetStream = new MemoryStream())
            {
                serializer.Serialize(targetStream, o);
                targetStream.Flush();
                targetStream.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(targetStream))
                    return reader.ReadToEnd();
            }
        }

        public static T FromXml<T>(this string xml)
        {
            return (T)new XmlSerializer(typeof(T)).Deserialize(new XmlTextReader(xml));
        }

        #endregion

        #region Math

        public static readonly float Deg2Rad = (float)(System.Math.PI / 180);
        public static readonly float Rad2Deg = (float)(180 / System.Math.PI);

        public static int Clamp(this int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public static float Clamp(this float value, float min, float max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public static float Clamp01(this float value)
        {
            if (value < 0)
                return 0;
            if (value > 1)
                return 1;
            return value;
        }

        public static float RadToDeg(this float value)
        {
            return value * Rad2Deg;
        }

        public static float DegToRad(this float value)
        {
            return value * Deg2Rad;
        }

        public static string ToBigNumberString(this int value)
        {
            return ((long)value).ToBigNumberString();
        }

        public static float Pow(this float value, int pow)
        {
            float temp = 1;
            for (var i = 0; i < pow; ++i)
                temp *= value;
            return temp;
        }

        public static int Pow(this int value, int pow)
        {
            var temp = 1;
            for (var i = 0; i < pow; ++i)
                temp *= value;
            return temp;
        }

        public static long Pow(this long value, int pow)
        {
            long temp = 1;
            for (var i = 0; i < pow; ++i)
                temp *= value;
            return temp;
        }

        public static string ToBigNumberString(this long value)
        {
            var formats = new[]
            {
                "{0:F1}K",
                "{0:F1}M",
                "{0:F1}B",
                "{0:F1}T",
                "{0:F1}q",
                "{0:F1}Q",
                //"{0:F1}s",
                //"{0:F1}S",
                "A lot"
            };

            var sign = value < 0;
            var absValue = System.Math.Abs(value);

            if (absValue < 1000)
                return value.ToString();

            for (var i = formats.Length - 1; i >= 0; --i)
            {
                var expr = 1000L.Pow(i + 1);
                if (absValue >= expr)
                    return string.Format(formats[i], (double)value / expr);
            }

            return "A lot";
        }

        #endregion

        #region Enumerable

        public static bool Chance(this float chance, Random random)
        {
            return random.NextDouble() < chance;
        }

        public static int GetWeighted(this System.Random random, IEnumerable<float> weights)
        {
            var arr = weights.ToArray();
            var weight = random.NextDouble() * arr.Sum();
            for (var i = 0; i < arr.Length; ++i)
            {
                if (weight <= arr[i])
                    return i;
                weight -= arr[i];
            }

            return arr.Length - 1;
        }

        public static T GetWeighted<T>(this IEnumerable<T> source, Func<T, float> weightProvider)
        {
            return source.ToArray()[_random.GetWeighted(source.Select(weightProvider))];
        }

        public static float GetRange(this Random random, float min, float max)
        {
            return (float)(random.NextDouble() * (max - min) + min);
        }

        public static T Random<T>(this IEnumerable<T> collection)
        {
            return collection.ToArray().Random();
        }

        public static T Random<T>(this T[] collection)
        {
            return collection.Random(out var index);
        }

        public static T Random<T>(this List<T> collection)
        {
            return collection.Random(out var index);
        }

        private static Random _random = new Random(DateTime.UtcNow.GetHashCode());

        public static T Random<T>(this T[] collection, out int index)
        {
            if (collection.Length == 0)
            {
                index = -1;
                return default(T);
            }

            index = _random.Next(collection.Length);
            return collection[index];
        }

        public static T Random<T>(this List<T> collection, out int index)
        {
            if (collection.Count == 0)
            {
                index = -1;
                return default(T);
            }

            index = _random.Next(collection.Count);
            return collection[index];
        }

        #endregion

        #region Files

        public static string GetFileHash(string filename)
        {
            if (!File.Exists(filename))
                return null;

            using (var stream = File.OpenRead(filename))
                return ToHashString(HashAlgorithm.ComputeHash(stream));
        }

        /*
        public static IDisposable WatchFile(this string path, IContainer scope, Action<string> callback)
        {
            return path.WatchFileChanges((p) =>
                scope.Resolve<Scheduler>().Run(() => callback(p), (int) (_random.NextDouble() * 1000 + 500)));
        }
        */

        public static IDisposable WatchFileChanges(this string path, Action<string> callback)
        {
            if (!Path.IsPathRooted(path))
                path = Path.Combine(Directory.GetCurrentDirectory(), path);

            var f = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(path),
                Filter = Path.GetFileName(path),
                EnableRaisingEvents = true
            };

            f.Changed += (sender, args) => callback(path);

            if (File.Exists(path))
                callback(path);

            return f;
        }

        #endregion

        #region Embedded

        internal static Stream GetEmbeddedResourceStream(string resourceName)
        {
            var assembly = typeof(DataExtensions).Assembly;
            var resourceFullname = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourceName));
            return assembly.GetManifestResourceStream(resourceFullname);
        }

        #endregion

        #region Time

        public static readonly DateTime UnixEpoch
            = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime ToDateTime(this long date)
        {
            return UnixEpoch.AddSeconds(date);
        }

        public static long ToLongTime(this DateTime date)
        {
            return (long)(date - UnixEpoch).TotalSeconds;
        }

        #endregion

        public static bool SetBinding(this object target, string propertyName, Bind binding)
        {
            binding.Target = target;
            binding.TargetPath = propertyName;
            return binding.Update();
        }

        public static IDisposable Subscribe(this UnityEvent unityEvent, UnityAction handler)
        {
            unityEvent.AddListener(handler);
            return new ActionDisposable(() => unityEvent.RemoveListener(handler));
        }

        public static void CallOnDestroy(this GameObject go, Action call) => new ActionDisposable(call).AttachTo(go);

        public static T AttachTo<T>(this T disposableInstance, GameObject gameObject) where T : IDisposable
        {
            var disposable = gameObject.GetComponent<DisposableUnityComponent>();
            if (disposable == null)
                disposable = gameObject.gameObject.AddComponent<DisposableUnityComponent>();
            disposable.Add(disposableInstance);
            return disposableInstance;
        }

        private static Dictionary<string, Type> _adapters; // = new Dictionary<string, Type>();

        static IBindingAdapter GetAdapter(string adapterType)
        {
            if (adapterType.IsNullOrEmpty() || adapterType == "None")
                return null;
            _adapters ??= typeof(IBindingAdapter).GetAllSubTypes(u => !u.IsAbstract)
                .ToDictionary(x => x.FullName, x => x);
            var type = _adapters[adapterType];
            return (IBindingAdapter)Activator.CreateInstance(type);
        }
        
        public static Bind CreateBinding(Func<object> modelFunc, string propertyName, string adapterType,
            string modelChangeEventName)
        {
            var binding = new Bind
            {
                Source = modelFunc,
                Path = propertyName,
                UpdatedEventName = modelChangeEventName,
                AllowPrivateProperties = true,
                SourceConverter = GetAdapter(adapterType)
            };

            return binding;
        }

        public static Bind CreateBinding(this object model, string propertyName, string adapterType,
            string modelChangeEventName) =>
            CreateBinding(() => model, propertyName, adapterType, modelChangeEventName);

        public static Bind CreateBinding(this object model, string propertyName, string adapterType) =>
            model.CreateBinding(propertyName, adapterType, null);
    }
}