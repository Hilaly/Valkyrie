using System;
using System.Collections.Generic;
using System.Linq;

namespace Valkyrie.Defines
{
    interface IMacrosContainer
    {}
    
    public interface ITypeDefine
    {
        string Namespace { get; set; }
        string Name { get; set; }
        bool IsValueType { get; set; }
        bool IsClass { get; set; }

        IReadOnlyList<IMemberDefine> GetMembers();
        IReadOnlyList<IPropertyDefine> GetProperties();
        IReadOnlyList<IFieldDefine> GetFields();

        ITypeDefine BaseType { get; set; }
        bool IsPublic { get; set; }
        IReadOnlyList<ITypeDefine> GetInterfaces();

        void TryAddField(IFieldDefine fieldDefine);
        void TryAddProperty(IPropertyDefine fieldDefine);
        void TryAddInterface(ITypeDefine define);
    }
    
    class TypeDefine : ITypeDefine
    {
        private readonly List<IMemberDefine> _members = new();
        private readonly List<ITypeDefine> _interfaces = new();

        public string Namespace { get; set; }
        public string Name { get; set; }
        public bool IsValueType { get; set; }
        public bool IsClass { get; set; }
        public bool IsPublic { get; set; }

        public ITypeDefine BaseType { get; set; }

        public IReadOnlyList<IMemberDefine> GetMembers()
        {
            return GetProperties().OfType<IMemberDefine>()
                .Union(GetFields())
                .ToList();
        }

        public IReadOnlyList<IPropertyDefine> GetProperties() => _members.OfType<IPropertyDefine>().ToList();

        public IReadOnlyList<IFieldDefine> GetFields() => _members.OfType<IFieldDefine>().ToList();

        public IReadOnlyList<ITypeDefine> GetInterfaces()
        {
            if(BaseType != null)
                return _interfaces
                    .Union(BaseType.GetInterfaces())
                    .ToList();
            return _interfaces;
        }

        public void TryAddField(IFieldDefine define)
        {
            var members = GetFields();
            var exist = members.FirstOrDefault(x => x.Name == define.Name);
            if (exist != null && !exist.Type.Equals(define.Type))
                throw new Exception($"Try to add field {define} but already exist");
            _members.Add(define);
        }

        public void TryAddProperty(IPropertyDefine define)
        {
            var members = GetProperties();
            var exist = members.FirstOrDefault(x => x.Name == define.Name);
            if (exist != null && !exist.Type.Equals(define.Type))
                throw new Exception($"Try to add property {define} but already exist");
            _members.Add(define);
        }

        public void TryAddInterface(ITypeDefine define)
        {
            if(_interfaces.Contains(define))
                return;
            _interfaces.Add(define);
        }
    }
}