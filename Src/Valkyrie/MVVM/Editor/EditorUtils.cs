using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Valkyrie.MVVM.Editor
{
    public static class EditorUtils
    {
        [MenuItem("Valkyrie/Tools/Delete local save")]
        public static void DeleteSave()
        {
            PlayerPrefs.DeleteAll();
        }

        internal static string GetCoolName(string original)
        {
            var temp = original.Replace("ConfigData", "");
            var sb = new StringBuilder();
            foreach (var ch in temp)
            {
                if (char.IsUpper(ch))
                    sb.Append(' ');
                sb.Append(ch);
            }

            return sb.ToString();
        }

        #region Unity working

        public static bool KeysPressed(string controlName, params KeyCode[] keys)
        {
            return GUI.GetNameOfFocusedControl() == controlName
                   && Event.current.type == EventType.KeyUp
                   && keys.Any(u => u == Event.current.keyCode);
        }

        public static void DrawWithColor(Color color, Action drawAction)
        {
            var temp = GUI.color;
            GUI.color = color;
            drawAction();
            GUI.color = temp;
        }

        public static void CreateFolder(string path)
        {
            if (!path.StartsWith("Assets"))
                path = $"Assets/{path}";
            //path = Path.GetDirectoryName(path);
            if (Directory.Exists(path))
                return;
            if (AssetDatabase.IsValidFolder(path))
                return;
            var parts = path.Split(new[] {'/', '\\'});
            var parentFolder = parts[0];

            for (int i = 1; i < parts.Length - 1; ++i)
            {
                parentFolder = $"{parentFolder}/{parts[i]}";
            }

            CreateFolder(parentFolder);
            AssetDatabase.CreateFolder(parentFolder, parts[parts.Length - 1]);
        }

        public static string DrawPopup(string label, string currentValue, IList<string> values)
        {
            if (values.Count == 0)
                values.Add(string.Empty);
            var index = EditorGUILayout.Popup(new GUIContent(label), values.IndexOf(currentValue), values.ToArray());
            if (index < 0 || index >= values.Count)
                return string.Empty;
            return values[index];
        }

        #endregion

        public static bool IsType<T>(Type type, bool notAbstract)
        {
            return (notAbstract)
                ? !type.IsAbstract && (typeof(T).IsAssignableFrom(type) || type.IsSubclassOf(typeof(T)))
                : typeof(T).IsAssignableFrom(type) || type.IsSubclassOf(typeof(T));
        }

        #region Nodes

        public static void DrawComplexGrid(Rect rect, Vector2 offset, float gridSpacing, float gridOpacity,
            Color gridColor, int bold = 5)
        {
            var tempRect = rect;
            DrawGrid(tempRect, offset, gridSpacing, gridOpacity * .25f, gridColor);
            DrawGrid(tempRect, offset, gridSpacing * bold, gridOpacity * .5f, gridColor);
            DrawGrid(tempRect, offset, gridSpacing * bold * 2, gridOpacity, gridColor);
        }

        static void DrawGrid(Rect rect, Vector2 offset, float gridSpacing, float gridOpacity, Color gridColor)
        {
            int widthDivs = Mathf.CeilToInt(rect.width / gridSpacing) + 1;
            int heightDivs = Mathf.CeilToInt(rect.height / gridSpacing) + 1;

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            Vector3 newOffset = new Vector3((rect.position.x + offset.x) % gridSpacing,
                (rect.position.y + offset.y) % gridSpacing, 0);

            for (int i = 0; i <= widthDivs; i++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset,
                    new Vector3(gridSpacing * i, rect.height + gridSpacing, 0f) + newOffset);
            }

            for (int j = 0; j <= heightDivs; j++)
            {
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset,
                    new Vector3(rect.width + gridSpacing, gridSpacing * j, 0f) + newOffset);
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        #endregion
    }
}