using System;
using System.Collections.Generic;
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
            GetOrCreate().Load();
        }

        static CemWindow GetOrCreate()
        {
            CemWindow wnd = GetWindow<CemWindow>();
            wnd.titleContent = new GUIContent("CemWindow");
            return wnd;
        }

        public static void Open(IGraph graph)
        {
            GetOrCreate().Load(graph);
        }

        internal static string fileName = "Assets/graph.json";
        internal static readonly JsonSerializerSettings SerializeSettings = new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        private void Load(IGraph graph = default)
        {
            if (graph == null)
                if (File.Exists(fileName))
                {
                    try
                    {
                        Debug.Log($"[CEM]: loading from {fileName}");
                        graph = JsonConvert.DeserializeObject<OverAllGraph>(File.ReadAllText(fileName),
                            SerializeSettings);
                    }
                    catch (Exception e)
                    {
                        graph = new OverAllGraph();
                        Debug.LogException(e);
                    }
                }
                else
                {
                    graph = new OverAllGraph();
                }

            _graphView.Graph = graph;

            UpdateView();
        }

        private void UpdateView()
        {
            var graph = _graphView.Graph;
            
            while (_toolbarBreadcrumbs.childCount > 0) _toolbarBreadcrumbs.PopItem();

            var graphList = new List<IGraph>();
            var g = graph;
            while (g != null)
            {
                graphList.Add(g);
                g = g is INode node ? node.Graph : default;
            }

            for (var i = graphList.Count - 1; i >= 0; --i)
            {
                var temp = graphList[i];
                _toolbarBreadcrumbs.PushItem(temp.Name, () => HandleBreadcrumbClick(temp));
            }
            
            _graphView.Reload();
        }

        private void HandleBreadcrumbClick(IGraph graph)
        {
            Load(graph);
        }

        private CemGraphView _graphView;
        private Toolbar _toolbar;
        private ToolbarBreadcrumbs _toolbarBreadcrumbs;

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

            _toolbarBreadcrumbs = new ToolbarBreadcrumbs();
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