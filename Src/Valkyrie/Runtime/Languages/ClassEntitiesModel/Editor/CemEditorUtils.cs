using UnityEditor;
using Valkyrie.Utils;

namespace Valkyrie.Editor.ClassEntitiesModel
{
    public static class CemEditorUtils
    {
        [MenuItem("Valkyrie/CEM %g")]
        static CemWindow OpenWindow()
        {
            var window = EditorWindow.GetWindow<CemWindow>();
            window.Load();
            return window;
        }

        internal static WorldModelInfo Load() => ClassModelSerializer.Load("Assets");

        internal static void Save(WorldModelInfo worldModel) => worldModel.Save();
    }
}