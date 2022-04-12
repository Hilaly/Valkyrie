using System.IO;
using UnityEditor;
using UnityEngine;

namespace Valkyrie.Editor
{
    public class MonoBehaviourCompilationPreferences : AssetPostprocessor
    {
        private const string PreferencesSimulationCompilerEnabled = "ValkyrieUnity_MonoSimulationCompilation_Enabled";
        private const string PreferencesSimulationPath = "ValkyrieUnity_MonoSimulationCompilation_Path";

        internal static bool MonoSimulationCompilationEnabled
        {
            get => EditorPrefs.GetBool(PreferencesSimulationCompilerEnabled, true);
            private set => EditorPrefs.SetBool(PreferencesSimulationCompilerEnabled, value);
        }

        internal static string MonoSimulationCompilationPath
        {
            get => EditorPrefs.GetString(PreferencesSimulationPath,
                Path.Combine("Assets", "Scripts", $"{SimulationCompiler.MonoTypeName}.cs"));
            private set => EditorPrefs.SetString(PreferencesSimulationPath, value);
        }

        [PreferenceItem("Valkyrie.Ecs")]
        static void PreferencesItem()
        {
            EditorGUI.BeginChangeCheck();

            MonoSimulationCompilationEnabled = EditorGUILayout.Toggle(
                new GUIContent("Enable rebuild GameObjectState",
                    "This allow to regenerate code for simulation GameState, based on monoBehaviours components"),
                MonoSimulationCompilationEnabled);
            EditorGUI.BeginDisabledGroup(!MonoSimulationCompilationEnabled);
            MonoSimulationCompilationPath = EditorGUILayout.TextField(
                "Output file for generated game state with monoBehaviours components", MonoSimulationCompilationPath);
            if (GUILayout.Button(new GUIContent("Generate EcsGameState")))
                SimulationCompiler.Recompile();
            EditorGUI.EndDisabledGroup();

            EditorGUI.EndChangeCheck();
        }
    }
}