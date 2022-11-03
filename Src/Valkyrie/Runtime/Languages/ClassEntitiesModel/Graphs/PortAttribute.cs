using System;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Scripting;
using Valkyrie.Tools;

namespace Valkyrie
{
    public interface IPortAttribute
    {
        FieldInfo Info { get; }
        string Name { get; }
        Direction Direction { get; }
        Port.Capacity Capacity { get; }
        
        void SetInfo(FieldInfo info);
    }
    
    public interface IValuePortAttribute : IPortAttribute
    {}

    public abstract class PortAttribute : PreserveAttribute, IPortAttribute
    {
        public FieldInfo Info { get; private set; }
        
        public string Name { get; set; }
        public abstract Direction Direction { get; }
        public abstract Port.Capacity Capacity { get; set; }

        public void SetInfo(FieldInfo info)
        {
            Info = info;
            Name = Name.IsNullOrEmpty() ? info.Name : Name;
        }
    }

    public abstract class ValuePortAttribute : PortAttribute, IValuePortAttribute
    {}

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ValueInAttribute : ValuePortAttribute
    {
        public override Direction Direction => Direction.Input;
        public override Port.Capacity Capacity { get; set; } = Port.Capacity.Single;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ValueOutAttribute : ValuePortAttribute
    {
        public override Direction Direction => Direction.Output;
        public override Port.Capacity Capacity { get; set; } = Port.Capacity.Single;
    }
}