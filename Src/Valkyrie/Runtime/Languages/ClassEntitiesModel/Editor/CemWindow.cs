using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Valkyrie.Utils;

namespace Valkyrie.Editor.ClassEntitiesModel
{
    public class CemWindow : EditorWindow
    {
        private const string ClearTitle = "Clear Graph?";
        private const string ClearMessage = "Clear all nodes and connections from this graph?";

        public WorldModelInfo WorldModel { get; private set; }

        private IGraphView _graphView;
        
        private Toolbar _toolbar;
        private ToolbarBreadcrumbs _toolbarBreadcrumbs;

        private void OnEnable()
        {
            //rootVisualElement.styleSheets.Add(Resources.Load<StyleSheet>("CemWindow"));
            
            CreateGraphView();
            CreateToolbar();
            
            titleContent = new GUIContent("CEM");
        }

        private void OnDisable()
        {
            if (_graphView is VisualElement element)
                rootVisualElement.Remove(element);
            if(_toolbar != null)
                rootVisualElement.Remove(_toolbar);
        }

        private void CreateGraphView()
        {
            _graphView = new SimulationFeatureGraphView(WorldModel, WorldModel);

            if (_graphView is VisualElement element)
            {
                element.StretchToParentSize();
                
                rootVisualElement.Add(element);
            }
        }

        private void CreateToolbar()
        {
            _toolbar = new Toolbar();

            _toolbarBreadcrumbs = new ToolbarBreadcrumbs();
            _toolbar.Add(_toolbarBreadcrumbs);

            var spacer = new ToolbarSpacer {flex = true};
            _toolbar.Add(spacer);
            
            var saveBtn = new Button(_graphView.Save) {text = "Save"};
            _toolbar.Add(saveBtn);
            
            var clearBtn = new Button(Clear) {text = "Reload"};
            _toolbar.Add(clearBtn);

            //var executeBtn = new Button(Execute) {text = "Execute All"};
            //_toolbar.Add(executeBtn);

            var search = new ToolbarSearchField();
            _toolbar.Add(search);

            var compileButton = new Button(Compile) { text = "Compile" };
            _toolbar.Add(compileButton);
            
            rootVisualElement.Add(_toolbar);
        }

        private void Compile()
        {
            var dirPath = Path.Combine("Assets", "Scripts", "Generated");
            var fileName = "Gen.cs";
            WorldModel?.WriteToDirectory(dirPath, fileName);
        }

        private void Clear()
        {
            if (EditorUtility.DisplayDialog(ClearTitle, ClearMessage, "Yes"))
            {
                Reload();
            }
        }

        void Save()
        {
            CemEditorUtils.Save(WorldModel);
        }

        public void Load()
        {
            WorldModel = new WorldModelInfo();// ??= CemEditorUtils.Load();
            _graphView.Graph = new Graph();
            if (File.Exists("Assets/graph.json"))
                _graphView.Graph = JsonConvert.DeserializeObject<Graph>(File.ReadAllText("Assets/graph.json"), GraphSerializer.SerSettings);
            _graphView.Reload();
        }

        void Reload()
        {
            WorldModel = null;
            Load();
        }
    }
}