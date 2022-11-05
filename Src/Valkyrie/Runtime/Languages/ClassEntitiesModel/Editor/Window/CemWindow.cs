using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Valkyrie.Model;
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
        }

        private CemGraphView _graphView;

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/CemWindow.uxml");
            visualTree.CloneTree(root);

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/CemWindow.uss");
            root.styleSheets.Add(styleSheet);

            _graphView = root.Q<CemGraphView>();
            
            //TODO:
            _graphView.Graph = new TestGraph();
        }
    }
}