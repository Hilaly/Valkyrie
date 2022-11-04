using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Valkyrie.Editor.ClassEntitiesModel
{
    public abstract class BaseGraphView : GraphView, IGraphView
    {
        protected Dictionary<string, INodeView> _nodeViewCache = new();
        
        public IGraph Graph { get; set; }
        
        public GridBackground GridBackground { get; private set; }
        public CemSearchProvider SearchProvider { get; private set; }
        public MiniMap MiniMap { get; private set; }
        
        public IEdgeConnectorListener EdgeConnectorListener { get; }

        protected BaseGraphView()
        {
            EdgeConnectorListener = new CemGraphEdgeConnectorListener(this);
        }
        
        protected void CreateGridBackground()
        {
            GridBackground = new GridBackground {name = "Grid"};
            Insert(0, GridBackground);
        }

        protected void CreateMiniMap()
        {
            MiniMap = new MiniMap {anchored = true, maxWidth = 200, maxHeight = 100, visible = false};
            Add(MiniMap);
        }

        protected void CreateSearch()
        {
            SearchProvider = ScriptableObject.CreateInstance<CemSearchProvider>();
            SearchProvider.Initialize(this);
            nodeCreationRequest = ctx => SearchWindow.Open(new SearchWindowContext(ctx.screenMousePosition), SearchProvider);
        }

        public void CreateNode(INodeFactory data, Vector2 position)
        {
            RecordUndo("Create Node");
            var node = data.Create();
            Graph.Add(node); // Definition & Initialize are called here
            var window = EditorWindow.GetWindow<CemWindow>();
            node.NodePosition = window.rootVisualElement.ChangeCoordinatesTo(contentViewContainer, position - window.position.position - new Vector2(3, 26));
            CreateNodeView<CemNodeView>(node, data);
            Save();
        }

        protected void RecordUndo(string title)
        {
            Debug.LogWarning($"[CEM]: undo {title} not recorded");
            // if (GraphAsset != null) Undo.RecordObject((ScriptableObject)GraphAsset, title);
        }

        internal void OpenSearch(Vector2 screenPosition, CemPortView port = null)
        {
            //TODO: port doing
            SearchWindow.Open(new SearchWindowContext(screenPosition), SearchProvider);
        }

        #region API

        protected void CreateNodeView<T>(INode node, INodeFactory info) where T : NodeViewBase, new()
        {
            var element = new T();
            if (element is IEditorNodeView editorView) editorView.EdgeListener = EdgeConnectorListener;
            if (element is INodeView nodeView)
            {
                nodeView.Initialize(node, info);
                _nodeViewCache.Add(node.Uid, nodeView);
            }
            AddElement(element);
        }
        public abstract void Save();

        public abstract void Reload();

        #endregion
    }
}