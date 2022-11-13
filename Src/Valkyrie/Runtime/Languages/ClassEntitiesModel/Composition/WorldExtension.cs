using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Valkyrie.Language.Description.Utils;
using Valkyrie.Tools;
using Valkyrie.Utils;

namespace Valkyrie.Composition
{
    public static partial class WorldExtension
    {
        private static readonly string[] Using = new[]
        {
            "System",
            "System.Linq",
            "System.Collections.Generic",
            "Valkyrie",
        };

        public static void WriteToDirectory(this IWorldInfo worldInfo, string dirPath)
        {
            var rootNamespace = worldInfo.Namespace;
            var methods = new List<KeyValuePair<string, string>>();

            WriteToSeparateFile(methods, string.Empty, "Gen.cs", rootNamespace,
                sb =>
                {
                    WriteComponents(worldInfo, sb);
                    sb.AppendLine();
                    WriteArchetypes(worldInfo, sb);
                    sb.AppendLine();
                    WriteSystems(worldInfo, sb);
                    sb.AppendLine();
                    WriteGeneralClassesAndInterfaces(worldInfo, sb);
                });

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

        static void WriteToSeparateFile(List<KeyValuePair<string, string>> methods, string dir, string filename,
            string namespaceName, Action<FormatWriter> internalWrite)
        {
            methods.Add(WriteToSeparateFile(dir, filename, namespaceName, internalWrite));
        }

        static KeyValuePair<string, string> WriteToSeparateFile(string dir, string filename, string namespaceName,
            Action<FormatWriter> internalWrite)
        {
            var path = Path.Combine(dir, filename);

            var sb = new FormatWriter();
            sb.WriteUsing(Using);
            sb.WriteInNamespace(namespaceName, () => internalWrite(sb));

            return new KeyValuePair<string, string>(path, sb.ToString());
        }

        public static IWorldInfo RegisterArchetype<T>(this IWorldInfo worldInfo) where T : IEntity =>
            worldInfo.RegisterArchetype(typeof(T));
        
        public static IWorldInfo RegisterSystem<T>(this IWorldInfo worldInfo) where T : ISimSystem =>
            worldInfo.RegisterSystem(typeof(T));
    }
}