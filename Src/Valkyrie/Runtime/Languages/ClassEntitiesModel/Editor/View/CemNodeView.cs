using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Valkyrie.Model;

namespace Valkyrie.View
{
    class CemNodeView : Node, INodeView, IEditorNodeView
    {
        public IEdgeConnectorListener EdgeListener { get; set; }

        public INode Node => (INode)userData;

        bool INodeView.IsMovable => true;

        public VisualElement GetRoot() => mainContainer;

        public void Init(INode node)
        {
            AddToClassList($"Node_{node.GetType()}");

            node.NodeChanged += OnNodeChanged;

            name = node.Uid;

            userData = node;
            viewDataKey = node.Uid;

            style.position = Position.Absolute;
            style.left = node.NodePosition.x;
            style.top = node.NodePosition.x;

            /*
            style.minWidth = node.NodeRect.size.x;
            style.minHeight = node.NodeRect.size.y;
            */

            UpdateTitle();
            InitializePorts();
            SetPosition(node.NodeRect);
            SetupProperties();

            if (node is IRenamable)
            {
                capabilities |= Capabilities.Renamable;
                SetupRenamableTitle();
            }

            RefreshExpandedState();
        }

        private void SetupProperties()
        {
            if (Node is INodeWithFields node)
            {
                foreach (var property in node.Properties)
                {
                    var propertyElement = CreateProperty(property);
                    if (propertyElement != null)
                        extensionContainer.Add(propertyElement);
                    else
                        extensionContainer.Add(new Label($"Placeholder: {property.Name}"));
                }
            }
        }

        private VisualElement CreateProperty(INodeProperty property)
        {
            var t = property.PropertyType;
            if (t == typeof(int))
            {
                var r = new IntegerField(property.Name);
                r.SetValueWithoutNotify((int)property.Value);
                r.RegisterValueChangedCallback(e => property.Value = e.newValue);
                r.RegisterCallback<FocusOutEvent>(e => property.Value = r.value);
                return r;
            }
            else if (t == typeof(float))
            {
                var r = new FloatField(property.Name);
                r.SetValueWithoutNotify((float)property.Value);
                r.RegisterValueChangedCallback(e => property.Value = e.newValue);
                r.RegisterCallback<FocusOutEvent>(e => property.Value = r.value);
                return r;
            }
            else if (t == typeof(string))
            {
                var r = new TextField(property.Name);
                r.SetValueWithoutNotify((string)property.Value);
                r.RegisterValueChangedCallback(e => property.Value = e.newValue);
                r.RegisterCallback<FocusOutEvent>(e => property.Value = r.value);
                return r;
            }

            Debug.LogWarning($"[CEM]: can not create property field for {property.PropertyType.FullName}");
            return default;
        }

        void InitializePorts()
        {
            foreach (var port in Node.Ports.OfType<IOutputPort>()) AddPort(port);
            foreach (var port in Node.Ports.OfType<IInputPort>()) AddPort(port);
        }

        private void AddPort(IPort port)
        {
            var portView = new CemPortView(port.Orientation, port.Direction, port.Capacity, port.Type, EdgeListener);

            portView.AddToClassList($"Port_{port.GetType()}");
            portView.portName = port.Name;
            portView.userData = port;
            portView.name = port.Uid;

            switch (port.Direction)
            {
                case Direction.Input:
                    inputContainer.Add(portView);
                    break;
                case Direction.Output:
                    outputContainer.Add(portView);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void SetupRenamableTitle()
        {
            var titleTextField = new TextField { isDelayed = true };
            var titleLabel = this.Q<Label>("title-label");

            titleTextField.style.display = DisplayStyle.None;
            titleLabel.parent.Insert(0, titleTextField);
            titleLabel.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.clickCount == 2 && e.button == (int)MouseButton.LeftMouse)
                    OpenTitleEditor();
            });

            titleTextField.RegisterValueChangedCallback(e => CloseAndSaveTitleEditor(e.newValue));

            titleTextField.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.clickCount == 2 && e.button == (int)MouseButton.LeftMouse)
                    CloseAndSaveTitleEditor(titleTextField.value);
            });

            titleTextField.RegisterCallback<FocusOutEvent>(e => CloseAndSaveTitleEditor(titleTextField.value));

            void OpenTitleEditor()
            {
                // show title textbox
                titleTextField.style.display = DisplayStyle.Flex;
                titleLabel.style.display = DisplayStyle.None;
                titleTextField.focusable = true;

                titleTextField.SetValueWithoutNotify(title);
                titleTextField.Focus();
                titleTextField.SelectAll();
            }

            void CloseAndSaveTitleEditor(string newTitle)
            {
                Debug.Log($"[CEM]: node {Node.Uid} renamed to {newTitle}");
                Node.Name = newTitle;

                // hide title TextBox
                titleTextField.style.display = DisplayStyle.None;
                titleLabel.style.display = DisplayStyle.Flex;
                titleTextField.focusable = false;

                UpdateTitle();
            }
        }

        void UpdateTitle() => title = Node.Name;

        private void OnNodeChanged(CemNodeChangedEvent obj)
        {
            Debug.LogWarning($"[CEM]: OnNodeChanged {Node.Uid} not implemented");
        }
    }
}