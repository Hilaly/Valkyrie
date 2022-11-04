using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Valkyrie.GraphDefineImpl
{
    [Preserve]
    [Node("test", Path = "Debug")]
    public class TestNode : GenericNode<TestNode>, INodeContent
    {
        protected override void DefineValuePorts(List<IValuePort> inPorts, List<IValuePort> outPorts)
        {
            inPorts.Add(new ValuePort()
            {
                
            });
        }

        public void FillBody(VisualElement container)
        {
            var propertiesContainer = new VisualElement();
            var propertiesTitle = new VisualElement() { name = "TypePropertiesTitle" };
            propertiesTitle.AddToClassList("TypePropertiesTitle");
            propertiesTitle.Add(new Label("Properties"));
            propertiesTitle.Add(new Button(AddProperty) { text = "+"});
            
            propertiesContainer.Add(propertiesTitle);
            container.Add(propertiesContainer);
        }

        private void AddProperty()
        {
            Debug.LogWarning($"CEM property add clicked");
        }
    }
}