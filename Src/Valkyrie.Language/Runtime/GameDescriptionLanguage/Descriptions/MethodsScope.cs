using System.Collections.Generic;

namespace Valkyrie.Language.Description
{
    public abstract class BaseScope
    {
        public readonly LocalVariables LocalVariables = new LocalVariables();
    }
    public class DependentScope : BaseScope
    {
        public List<FactsFilterMethodDescription> Filters { get; } = new List<FactsFilterMethodDescription>();
    }

    public class MethodScope : BaseScope
    {
        public string Result;
        public string Name;
        public List<FieldDescription> Args = new List<FieldDescription>();
        public List<FactCreationMethodDescription> Methods { get; } = new List<FactCreationMethodDescription>();

        public string GetResultType()
        {
            if (!string.IsNullOrEmpty(Result))
                return LocalVariables.Get(Result).FieldDescription.Type;
            return "void";
        }
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