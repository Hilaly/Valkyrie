using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor.Meta
{
    public static class LocalStorageUtils
    {
        [MenuItem("Valkyrie/Save/Delete local storage")]
        static void CleanLocalStorageProfile()
        {
            File.Delete(Path.Combine(Application.persistentDataPath, "profile.json"));
        }
    }
}