using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Valkyrie.Language.Description;

namespace Editor
{
    public class ValkyrieGameLanguageCompiler : AssetPostprocessor
    {
        private const string PreferencesAutoRecompileEnabled = "ValkyrieUnity_LogicLanguage_Compiler_Enabled";
        private const string PreferencesSimulationPath = "ValkyrieUnity_LogicLanguage_Compiler_Path";

        internal static bool AutoCompilationEnabled
        {
            get => EditorPrefs.GetBool(PreferencesAutoRecompileEnabled, true);
            set => EditorPrefs.SetBool(PreferencesAutoRecompileEnabled, value);
        }

        internal static string CompilationPath
        {
            get => EditorPrefs.GetString(PreferencesSimulationPath,
                Path.Combine("Assets", "Scripts", $"LogicLanguageClasses.Generated.cs"));
            private set => EditorPrefs.SetString(PreferencesSimulationPath, value);
        }

        [PreferenceItem("Valkyrie Logic Language")]
        static void PreferenceItem()
        {
            EditorGUI.BeginChangeCheck();

            AutoCompilationEnabled = EditorGUILayout.Toggle(
                new GUIContent("Enable Compilation on change",
                    "This allow valkyrie to generate code, based on gdl file changes"),
                AutoCompilationEnabled);
            EditorGUI.BeginDisabledGroup(!AutoCompilationEnabled);
            CompilationPath = EditorGUILayout.TextField(
                "Output file for generated code", CompilationPath);
            if (GUILayout.Button(new GUIContent("Generate Logic")))
                Compile();
            EditorGUI.EndDisabledGroup();

            EditorGUI.EndChangeCheck();
        }

        [DidReloadScripts]
        internal static void Recompile()
        {
            if (!AutoCompilationEnabled)
                return;
            Compile();
        }

        [UnityEditor.MenuItem("Valkyrie/Logic/Generate Logic World")]
        public static void Compile()
        {
            Debug.Log($"Valkyrie:<color=green> generating logic ...</color>");

            var gdlFiles = Directory.EnumerateFiles("Assets", "*.gdl", SearchOption.AllDirectories).ToList();
            if (gdlFiles.Any())
            {
                var world = new WorldDescription();
                foreach (var gdlFile in gdlFiles)
                    try
                    {
                        Compiler.CompileWorldLogic(world, File.ReadAllText(gdlFile));
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"Valkyrie: <color=red>error during parsing {gdlFile}</color>");
                        throw;
                    }

                File.WriteAllText(CompilationPath, world.ToString());
                Debug.Log($"Valkyrie: <color=green>logic generated</color>");
                AssetDatabase.Refresh();
            }
            else
                Debug.Log($"Valkyrie:<color=yellow> gdl files not found</color>");
        }
    }
}