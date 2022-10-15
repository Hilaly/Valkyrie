using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Valkyrie.Tools
{
    public static class StringUtils
    {
        public static string[] SplitToLines(this string text) => Regex.Split(text, "\r\n|\r|\n");
        
        public static bool IsNullOrEmpty(this string text)
        {
            return string.IsNullOrEmpty(text);
        }

        public static bool NotNullOrEmpty(this string text)
        {
            return !string.IsNullOrEmpty(text);
        }

        public static string Join<T>(this IEnumerable<T> source, string separator)
        {
            return string.Join(separator, source.Select(u => u.ToString()).ToArray());
        }

        public static string Join(this string[] source, string separator)
        {
            return string.Join(separator, source);
        }

        public static Stream ToStream(this string source)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(source));
        }

        public static string ReadAllText(this Stream stream)
        {
            return new StreamReader(stream).ReadToEnd();
        }

    }
}