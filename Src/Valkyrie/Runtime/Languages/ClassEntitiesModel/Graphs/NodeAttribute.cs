using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Scripting;
using Utils;

namespace Valkyrie
{
    public interface IReflectionData
    {
        string Name { get; }
        string Tooltip { get; }
        Vector2 MinSize { get; }
        bool Deletable { get; }
        bool Movable { get; }

        Type EditorView { get; }

        INode Create();
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class NodeAttribute : PreserveAttribute, IReflectionData
    {
        private const BindingFlags BindingFlags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy;

        public HashSet<string> Tags { get; }
        /// <summary>
        /// Slash-delimited path to categorize this node in the search window.
        /// </summary>
        public string Path { get; set; }
        public float MinWidth { get; set; } = 50;
        public float MinHeight { get; set; } = 10;
        public Type Type { get; private set; }
        
        #region IReflectionData

        public string Name { get; set; }
        public string Tooltip { get; set; }
        public Vector2 MinSize => new Vector2(MinWidth, MinHeight);
        public bool Deletable { get; set; } = true;
        public bool Movable { get; set; } = true;

        #endregion
        
        public Type EditorView { get; private set; }
        public readonly List<IValuePortAttribute> ValuePorts = new();


        public NodeAttribute(params string[] tags)
        {
            Tags = new HashSet<string>(tags);
        }

        public INode Create() => (INode)Activator.CreateInstance(Type);

        private void Initialize(Type nodeType)
        {
            Type = nodeType;
            
            var methodTable = Type.GetMethodTable(BindingFlags);
            methodTable.Add(string.Empty, null);

            ExtractSettings();
            ExtractValuePorts(methodTable);
            ExtractFlowPorts(methodTable);
            //ExtractViews();
        }

        private void ExtractValuePorts(Dictionary<string, MethodInfo> methodTable)
        {
            // This OrderBy sorts the fields by the order they are defined in the code with subclass fields first
            foreach (var info in Type.GetFields(BindingFlags).OrderBy(field => field.MetadataToken))
            {
                foreach (var attribute in info.GetCustomAttributes<ValuePortAttribute>(true))
                {
                    attribute.SetInfo(info);
                    //TODO: attribute.SetCallbackInfo(methodTable[attribute.Callback]);
                    // Debug.Log($"Extracting Value Port '{attribute.Name} {attribute.Direction}'");
                    ValuePorts.Add(attribute);
                }
            }
        }

        private void ExtractFlowPorts(Dictionary<string, MethodInfo> methodTable)
        {
            /*TODO
            FlowPorts = new List<IFlowPortAttribute>();
            // This OrderBy sorts the fields by the order they are defined in the code with subclass fields first
            foreach (var fieldInfo in Type.GetFields(BindingFlags).OrderBy(field => field.MetadataToken))
            {
                foreach (var attribute in fieldInfo.GetCustomAttributes<FlowPortAttribute>(true))
                {
                    attribute.SetInfo(fieldInfo);
                    attribute.SetCallbackInfo(methodTable[attribute.Callback]);
                    // Debug.Log($"Extracting Flow Port '{attribute.Name} {attribute.Direction}'");
                    FlowPorts.Add(attribute);
                }
            }
            */
        }


        private void ExtractSettings()
        {
            Name = string.IsNullOrEmpty(Name) ? Type.Name.Replace("Node", "").Replace(".", "/") : Name;
            Path = string.IsNullOrEmpty(Path) ? Type.Namespace?.Replace(".", "/") : Path;
        }
    }
}