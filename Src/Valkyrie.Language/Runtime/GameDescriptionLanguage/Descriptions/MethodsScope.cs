using System.Collections.Generic;

namespace Valkyrie.Language.Description
{
    public class DependentScope
    {
        public readonly LocalVariables LocalVariables = new LocalVariables();
        public List<FactsFilterMethodDescription> Filters { get; } = new List<FactsFilterMethodDescription>();
    }

    public class ViewScope : DependentScope
    {
        public string Name;
        public List<ViewProperty> Properties = new List<ViewProperty>();
    }

    public class ViewProperty
    {
        public FieldDescription Field;
        public string Op;
    }

    public class MethodsScope : DependentScope, ISimPart
    {
        public List<FactCreationMethodDescription> Methods { get; } = new List<FactCreationMethodDescription>();
    }
}