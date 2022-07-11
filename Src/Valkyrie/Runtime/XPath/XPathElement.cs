using System;
using System.Reflection;
using UnityEngine;

namespace Valkyrie.XPath
{
    public enum XPathType
    {
        GameObject,
        Component,
        Member
    }
    public class XPathElement
    {
        public XPathType Type { get; }
        public object Value { get; }

        public XPathElement(XPathType type, object value)
        {
            Type = type;
            Value = value;
        }
        public XPathElement(GameObject gameObject) : this(XPathType.GameObject, gameObject)
        {}

        public string GetNodeName()
        {
            switch (Type)
            {
                case XPathType.GameObject:
                    return ((GameObject)Value).name;
                case XPathType.Component:
                    return ((Component)Value).GetType().Name;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public GameObject GetGameObject()
        {
            switch (Type)
            {
                case XPathType.GameObject:
                    return (GameObject)Value;
                case XPathType.Component:
                    return default;// ((Component)Value).gameObject;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public GameObject GetParent()
        {
            switch (Type)
            {
                case XPathType.GameObject:
                    return ((GameObject)Value).transform.parent.gameObject;
                case XPathType.Component:
                    return ((Component)Value).gameObject;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class XPathMemberElement : XPathElement
    {
        public MemberInfo Info { get; }

        public XPathMemberElement(object value, MemberInfo info) : base(XPathType.Member, value)
        {
            Info = info;
        }
    }
}