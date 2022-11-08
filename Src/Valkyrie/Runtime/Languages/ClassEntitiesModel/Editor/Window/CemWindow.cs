using System;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Valkyrie.Model;
using Valkyrie.Utils;
using Valkyrie.View;

namespace Valkyrie.Window
{
    public class CemWindow : EditorWindow
    {
        [MenuItem("Window/UI Toolkit/CemWindow")]
        [MenuItem("Valkyrie/CEM %g")]
        public static void OpenWindow()
        {
            CemWindow wnd = GetWindow<CemWindow>();
            wnd.titleContent = new GUIContent("CemWindow");
            
            wnd.Load();
        }

        internal static string fileName = "Assets/graph.json";
        internal static JsonSerializerSettings SerializeSettings = new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        private void Load()
        {
            if (File.Exists(fileName))
            {
                try
                {
                    Debug.Log($"[CEM]: loading from {fileName}");
                    _graphView.Graph = JsonConvert.DeserializeObject<OverAllGraph>(File.ReadAllText(fileName), SerializeSettings);
                    _graphView.Reload();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            if (_graphView.Graph == null)
            {
                _graphView.Graph = new OverAllGraph();
                _graphView.Reload();
            }
        }

        private CemGraphView _graphView;
        private Toolbar _toolbar;

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/CemWindow.uxml");
            visualTree.CloneTree(root);
            
            CreateToolbar();

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/CemWindow.uss");
            root.styleSheets.Add(styleSheet);

            _graphView = root.Q<CemGraphView>();

            Load();
        }

        private void CreateToolbar()
        {
            _toolbar = new Toolbar();

            var _toolbarBreadcrumbs = new ToolbarBreadcrumbs();
            _toolbar.Add(_toolbarBreadcrumbs);

            var spacer = new ToolbarSpacer {flex = true};
            _toolbar.Add(spacer);
            
            var saveBtn = new Button(Save) {text = "Save"};
            _toolbar.Add(saveBtn);
            
            var executeBtn = new Button(Compile) {text = "Compile"};
            _toolbar.Add(executeBtn);

            var search = new ToolbarSearchField();
            _toolbar.Add(search);
            
            /*
            var clearBtn = new Button(Clear) {text = "Clear"};
            _toolbar.Add(clearBtn);
            */

            rootVisualElement.Add(_toolbar);
        }

        private void Save()
        {
            _graphView.Graph.MarkDirty();
        }

        private void Compile()
        {
            CemCodeGenerator.Generate(_graphView.Graph);
        }

        private void OnDisable()
        {
            _graphView.Graph.MarkDirty();
        }
    }
}