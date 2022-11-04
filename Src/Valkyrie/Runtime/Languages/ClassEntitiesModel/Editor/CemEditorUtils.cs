using UnityEditor;
using Valkyrie.Utils;

namespace Valkyrie.Editor.ClassEntitiesModel
{
    public static class CemEditorUtils
    {
        internal static WorldModelInfo Load() => ClassModelSerializer.Load("Assets");

        internal static void Save(WorldModelInfo worldModel) => worldModel.Save();
    }
}