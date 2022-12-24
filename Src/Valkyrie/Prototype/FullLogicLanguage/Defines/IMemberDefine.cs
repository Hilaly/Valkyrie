namespace Valkyrie.Defines
{
    public interface IMemberDefine
    {
        public ITypeDefine Type { get; set; }
        public string Name { get; set; }
    }

    public interface IFieldDefine : IMemberDefine
    {
        public bool IsPublic { get; set; }
    }

    public interface IPropertyDefine : IMemberDefine
    {
    }

    class MemberDefine : IMemberDefine
    {
        public ITypeDefine Type { get; set; }
        public string Name { get; set; }
    }

    class FieldDefine : MemberDefine, IFieldDefine
    {
        public bool IsPublic { get; set; }
    }

    class PropertyDefine : MemberDefine, IPropertyDefine
    {
        
    }
}