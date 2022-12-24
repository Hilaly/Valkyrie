using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Valkyrie.Defines;
using Valkyrie.Language.Description.Utils;
using Valkyrie.Tools;
using Valkyrie.Utils;

namespace Valkyrie
{
    public static class PExtensions
    {
        public static string WriteFullTypeName(this Type t)
        {
            var tn = t.Name;
            var length = tn.IndexOf('`');
            if (length > 0)
                tn = tn[..length];
            return $"{t.Namespace}.{tn}";
        }

        public static string GetFullName(this ITypeDefine typeDefine)
        {
            var ns = typeDefine.Namespace.IsNullOrEmpty()
                ? "global::"
                : $"{typeDefine.Namespace}.";
            return $"{ns}{typeDefine.Name}";
        }

        internal static string Log(this ParserContext context)
        {
            var sb = new FormatWriter();

            if (context.Aliases.Any())
                sb.WriteBlock("Aliases", () =>
                {
                    foreach (var @alias in context.Aliases)
                        sb.AppendLine($"{alias.Key} -> {alias.Value}");
                });
            sb.AppendLine(context.Game.Log());

            return sb.ToString();
        }

        public static string Log(this GameDescription gameDescription)
        {
            var sb = new FormatWriter();

            if (gameDescription.Types.Any())
                sb.WriteBlock("Types", () =>
                {
                    foreach (var typeDefine in gameDescription.Types.Values.Where(x => x is NativeTypeDefine))
                        sb.AppendLine($"imported {typeDefine.GetFullName()}");

                    foreach (var typeDefine in gameDescription.Types.Values.Where(x => x is not NativeTypeDefine))
                        sb.WriteBlock(typeDefine.GetFullName(), () =>
                        {
                            foreach (var define in typeDefine.GetFields())
                                sb.AppendLine(
                                    $"{(define.IsPublic ? "public" : "private")} {define.Type.GetFullName()} {define.Name};");
                            foreach (var define in typeDefine.GetProperties())
                                sb.AppendLine($"{define.Type.GetFullName()} {define.Name} get; set;");
                        });
                });

            var components = gameDescription.GetComponents().ToList();
            if (components.Any())
                sb.WriteBlock("Components", () =>
                {
                    foreach (var component in components)
                        sb.AppendLine($"{component.Name}: {component.Type.GetTypeString()}");
                });

            var archetypes = gameDescription.GetArchetypes().ToList();
            if (archetypes.Any())
                sb.WriteBlock("Archetypes", () =>
                {
                    foreach (var archetype in archetypes)
                    {
                        if (archetype.Components.Any())
                            sb.WriteBlock($"archetype {archetype.Name}", () =>
                            {
                                foreach (var component in archetype.Components)
                                    sb.AppendLine($"{component.Name}: {component.Type}");
                            });
                        else
                            sb.AppendLine($"archetype {archetype.Name} {{ }}");
                    }
                });

            return sb.ToString();
        }

        internal static void Write(this FormatWriter sb, ITypeDefine typeDefine)
        {
            void WriteType()
            {
                var glAcc = (typeDefine.IsPublic ? "public" : "private");
                var tType = typeDefine.IsClass ? "class" : ("struct");
                var typeDefStr = $"{glAcc} {tType} {typeDefine.Name}";
                if (typeDefine.BaseType != null)
                    typeDefStr += $" : {typeDefine.BaseType.GetFullName()}";
                foreach (var define in typeDefine.GetInterfaces())
                    typeDefStr += $", {define.GetFullName()}";
                sb.WriteBlock(typeDefStr, () =>
                {
                    //TODO: implement later
                    sb.AppendLine("//TODO: add fields");
                    sb.AppendLine("//TODO: add properties");
                    sb.AppendLine("//TODO: add methods");
                });
            }

            if (typeDefine.Namespace.NotNullOrEmpty())
                sb.WriteInNamespace(typeDefine.Namespace, WriteType);
            else
                WriteType();
        }


        internal static void WriteToSeparateFile(List<KeyValuePair<string, string>> methods, string subDir,
            string fileName,
            Action<FormatWriter> internalWrite)
        {
            methods.Add(WriteToSeparateFile(subDir, fileName, internalWrite));
        }

        static KeyValuePair<string, string> WriteToSeparateFile(string dir, string filename,
            Action<FormatWriter> internalWrite)
        {
            var path = Path.Combine(dir, filename);

            var sb = new FormatWriter();
            internalWrite(sb);

            return new KeyValuePair<string, string>(path, sb.ToString());
        }

        internal static void WriteToDirectory(string dirPath, IEnumerable<KeyValuePair<string, string>> methods)
        {
            CleanDirectory(dirPath);

            //3. Write files
            foreach (var (fileName, text) in methods)
            {
                var fullPath = Path.Combine(dirPath, fileName);
                UtilsExtensions.EnsureDirectoryExistsForFile(fullPath);
                Debug.Log($"[GENERATION]: writing to file {fullPath}");
                File.WriteAllText(fullPath, text);
            }

            Debug.Log($"[GENERATION]: SUCCESS in {dirPath}");
        }

        private static void CleanDirectory(string dirPath)
        {
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            foreach (var filePath in Directory.EnumerateFiles(dirPath, "*.cs", SearchOption.AllDirectories))
            {
                Debug.Log($"[GENERATION]: remove file {filePath}");
                File.Delete(filePath);
            }
        }
    }
}