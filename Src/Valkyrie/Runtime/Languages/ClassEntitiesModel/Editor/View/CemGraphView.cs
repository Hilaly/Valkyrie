using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Valkyrie.View
{
    class CemSearchProvider : ScriptableObject, ISearchWindowProvider
    {
        public void Initialize(CemGraphView graphView)
        {
            //TODO:
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            throw new System.NotImplementedException();
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            throw new System.NotImplementedException();
        }
    }

    public class CemGraphView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<CemGraphView, UxmlTraits>
        {
        }

        private CemSearchProvider _searchProvider;
        private MiniMap _minimap;

        public IGraph Graph { get; private set; }

        public CemGraphView()
        {
            //TODO: SetupZoom(ContentZoomer.DefaultMinScale * 0.5f, ContentZoomer.DefaultMaxScale);

            CreateDefaultElements();

            RegisterCallbacks();
            SetupManipulators();

            LoadStyleSheets();

            CreateSearch();
        }

        #region Initialization

        void RegisterCallbacks()
        {
            RegisterCallback<GeometryChangedEvent>(GeometryChangedCallback);
            RegisterCallback<KeyUpEvent>(OnKeyUp);

            //TODO: graphViewChanged = OnGraphViewChanged;
            //TODO: viewTransformChanged = OnViewTransformChanged;
        }

        private void CreateDefaultElements()
        {
            Insert(0, new GridBackground() { name = "Grid" });
            Add(_minimap = new MiniMap { anchored = true, maxWidth = 200, maxHeight = 100, visible = false });
        }

        private void LoadStyleSheets()
        {
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/CemWindow.uss"));
        }

        private void SetupManipulators()
        {
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        private void CreateSearch()
        {
            _searchProvider = ScriptableObject.CreateInstance<CemSearchProvider>();
            _searchProvider.Initialize(this);
            nodeCreationRequest = ctx =>
                SearchWindow.Open(new SearchWindowContext(ctx.screenMousePosition), _searchProvider);
        }

        #endregion

        #region Callbacks

        private void GeometryChangedCallback(GeometryChangedEvent evt)
        {
            _minimap.SetPosition(new Rect(worldBound.width - 205, 25, 200, 100));
        }

        private void OnKeyUp(KeyUpEvent evt)
        {
            if (evt.target != this)
            {
                return;
            }

            switch (evt.keyCode)
            {
                case KeyCode.C when !evt.ctrlKey && !evt.commandKey:
                    // AddComment();
                    break;
                case KeyCode.M:
                    _minimap.visible = !_minimap.visible;
                    break;
                case KeyCode.H when !evt.ctrlKey && !evt.commandKey:
                    HorizontallyAlignSelectedNodes();
                    break;
                case KeyCode.V when !evt.ctrlKey && !evt.commandKey:
                    VerticallyAlignSelectedNodes();
                    break;
            }
        }

        private void HorizontallyAlignSelectedNodes()
        {
            float sum = 0;
            int count = 0;

            // TODO: Implement a way to Align to "first selected" thing rather then average
            foreach (var selectable in selection)
            {
                if (selectable is Node node)
                {
                    sum += node.GetPosition().xMin;
                    count++;
                }
            }

            float xAvg = sum / count;
            foreach (var selectable in selection)
            {
                if (selectable is INodeView { IsMovable: true } node)
                {
                    var pos = node.GetPosition();
                    pos.xMin = xAvg;
                    node.SetPosition(pos);
                }
            }
        }

        private void VerticallyAlignSelectedNodes()
        {
            float sum = 0;
            int count = 0;

            foreach (var selectable in selection)
            {
                if (selectable is INodeView { IsMovable: true } node)
                {
                    sum += node.GetPosition().yMin;
                    count++;
                }
            }

            float yAvg = sum / count;
            foreach (var selectable in selection)
            {
                if (selectable is Node node)
                {
                    var pos = node.GetPosition();
                    pos.yMin = yAvg;
                    node.SetPosition(pos);
                }
            }
        }

        #endregion
    }

    public interface IGraph
    {
    }
}