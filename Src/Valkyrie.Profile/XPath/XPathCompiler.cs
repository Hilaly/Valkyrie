using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Valkyrie.Grammar;

namespace Valkyrie.XPath
{
    internal class XPathCompiler
    {
        public IPathSelector Compile(IAstNode ast)
        {
            var name = ast.Name;
            var children = ast.GetChildren();
            switch (name)
            {
                case "<root>":
                {
                    switch (children.Count)
                    {
                        case 3:
                            return new MultiPathSelector(Compile(children[0]), Compile(children[2]));
                        case 1:
                            return Compile(children[0]);
                        default:
                            throw new Exception($"Invalid AST");
                    }
                }
                case "<path>":
                {
                    var nodeSelectorNode = children.Find(x => x.Name == "<node_select>");
                    var resultNode = Compile(nodeSelectorNode);
                    if (children.First().Name == "<move_op>")
                    {
                        switch (children.First().GetString())
                        {
                            case "/":
                                resultNode = new RedirectSelector(new AbsolutePathSelector(), resultNode);
                                break;
                            case "//":
                                resultNode = new RedirectSelector(new AnyNodeInSceneSelector(), resultNode);
                                break;
                            default:
                                throw new Exception($"Invalid move operator {children.First().GetString()}");
                        }
                    }

                    if (children.Last().Name == "<path_nodes>")
                    {
                        resultNode = new RedirectSelector(resultNode, Compile(children.Last()));
                    }

                    return resultNode;
                }
                case "<node_select>":
                {
                    if (children.First().Name == "<axis_test>")
                        throw new NotImplementedException($"Axis Test not implemented in XPath");
                    var selectorNode = children.Find(x => x.Name == "<node_test>");
                    var selector = Compile(selectorNode);
                    if (children.Last().Name == "<predicate_test>")
                        selector = new RedirectSelector(selector, Compile(children.Last()));
                    return selector;
                }
                case "<node_test>":
                {
                    switch (children[0].Name)
                    {
                        case "<self_node>":
                        case "<parent_node>":
                        case "<component_node>":
                        case "<any_node>":
                            return Compile(children[0]);
                        default:
                            return new ByNameSelector(children[0].GetString());
                    }
                }
                case "<self_node>":
                    return new SelfSelector();
                case "<parent_node>":
                    return new ParentSelector();
                case "<any_node>":
                    return new RepeatSelector();
                case "<component_node>":
                    return new ComponentSelector(ast.GetString());
                case "<path_nodes>":
                {
                    var selector = Compile(children.Find(x => x.Name == "<path_node>"));
                    if (children.Last().Name == "<path_nodes>")
                        selector = new RedirectSelector(selector, Compile(children.Last()));
                    return selector;
                }
                case "<path_node>":
                {
                    if (children[0].Name == "<move_op>")
                    {
                        var selectNode = Compile(children[1]);
                        var moveNode = Compile(children[0]);
                        if (moveNode is ChildrenMoveSelector && (selectNode is ParentSelector || selectNode is ComponentSelector))
                            return selectNode;
                        return new RedirectSelector(moveNode, selectNode);
                    }

                    return new MemberMoveSelector(children[1].GetString());
                }
                case "<move_op>":
                {
                    switch (children.First().GetString())
                    {
                        case "/":
                            return new ChildrenMoveSelector();
                        case "//":
                            return new DescendantsMoveSelector();
                        default:
                            throw new Exception($"Invalid move operator {children.First().GetString()}");
                    }
                }
                case "<predicate_test>":
                    return Compile(children[1]);
                case "<predicate>":
                    return Compile(children[0]);
                case "<index_predicate>":
                    return new IndexSelector(ast.GetInt());
                default:
                    throw new Exception($"Unknown node '{name}'");
            }
        }
    }

    /// <summary>
    /// Selects component with name only from gameobjects
    /// </summary>
    class ComponentSelector : IPathSelector
    {
        private readonly string _componentType;

        public ComponentSelector(string strComponent)
        {
            _componentType = strComponent.Substring(1);
        }

        public IEnumerable<XPathElement> SelectNodes(IEnumerable<XPathElement> nodes)
        {
            foreach (var xPathElement in nodes)
            {
                if (xPathElement.Type == XPathType.GameObject)
                {
                    var component = xPathElement.GetGameObject().GetComponent(_componentType);
                    if(component != null)
                        yield return new XPathElement(XPathType.Component, component);
                }
            }
        }
    }
    
    class MemberMoveSelector : IPathSelector
    {
        private readonly string _memberName;

        public MemberMoveSelector(string memberName)
        {
            _memberName = memberName;
        }

        public IEnumerable<XPathElement> SelectNodes(IEnumerable<XPathElement> nodes)
        {
            foreach (var pathElement in nodes)
            {
                var type = pathElement.Value.GetType();
                var member = type.GetMember(_memberName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var memberInfo in member)
                    yield return new XPathMemberElement(pathElement.Value, memberInfo);
            }
        }
    }

    /// <summary>
    /// Returns same nodes
    /// </summary>
    class RepeatSelector : IPathSelector
    {
        public IEnumerable<XPathElement> SelectNodes(IEnumerable<XPathElement> nodes) => nodes;
    }

    /// <summary>
    /// Returns node with index
    /// </summary>
    class IndexSelector : IPathSelector
    {
        private readonly int _index;

        public IndexSelector(int index)
        {
            _index = index;
        }

        public IEnumerable<XPathElement> SelectNodes(IEnumerable<XPathElement> nodes)
        {
            var currentIndex = 0;
            using var enumerator = nodes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (currentIndex != _index)
                {
                    currentIndex++;
                    continue;
                }

                yield return enumerator.Current;
                yield break;
            }
        }
    }

    /// <summary>
    /// Selects all children nodes
    /// </summary>
    class ChildrenMoveSelector : IPathSelector
    {
        public IEnumerable<XPathElement> SelectNodes(IEnumerable<XPathElement> nodes)
        {
            foreach (var pathElement in nodes)
            {
                var go = pathElement.GetGameObject();
                for (var i = 0; i < go.transform.childCount; ++i)
                    yield return new XPathElement(go.transform.GetChild(i).gameObject);
            }
        }
    }

    /// <summary>
    /// Selects all descendants nodes
    /// </summary>
    class DescendantsMoveSelector : IPathSelector
    {
        public IEnumerable<XPathElement> SelectNodes(IEnumerable<XPathElement> nodes)
        {
            foreach (var pathElement in nodes)
            {
                var go = pathElement.GetGameObject();
                yield return new XPathElement(go);
                for (var i = 0; i < go.transform.childCount; ++i)
                    foreach (var o in AnyNodeInSceneSelector.SelectAllNodesInTree(go.transform.GetChild(i).gameObject))
                        yield return new XPathElement(o);
            }
        }
    }

    /// <summary>
    /// Selects parent node
    /// </summary>
    class ParentSelector : IPathSelector
    {
        public IEnumerable<XPathElement> SelectNodes(IEnumerable<XPathElement> nodes)
        {
            foreach (var node in nodes)
            {
                var go = node.GetParent();
                yield return new XPathElement(go);
            }
        }
    }

    /// <summary>
    /// Returns all nodes in scene
    /// </summary>
    class AnyNodeInSceneSelector : IPathSelector
    {
        public IEnumerable<XPathElement> SelectNodes(IEnumerable<XPathElement> nodes)
        {
            foreach (var node in nodes)
            {
                var go = node.GetGameObject();
                foreach (var rootGameObject in go.scene.GetRootGameObjects())
                foreach (var gameObject in SelectAllNodesInTree(rootGameObject))
                    yield return new XPathElement(gameObject);
            }
        }

        internal static IEnumerable<GameObject> SelectAllNodesInTree(GameObject o)
        {
            yield return o;
            for (int i = 0; i < o.transform.childCount; i++)
                foreach (var go in SelectAllNodesInTree(o.transform.GetChild(i).gameObject))
                    yield return go;
        }
    }

    /// <summary>
    /// Return all root game objects in scene
    /// </summary>
    class AbsolutePathSelector : IPathSelector
    {
        public IEnumerable<XPathElement> SelectNodes(IEnumerable<XPathElement> nodes)
        {
            foreach (var node in nodes)
            {
                var go = node.GetGameObject();
                foreach (var rootGameObject in go.scene.GetRootGameObjects())
                    yield return new XPathElement(rootGameObject);
            }
        }
    }

    /// <summary>
    /// Redirects output from one selector to other
    /// </summary>
    class RedirectSelector : IPathSelector
    {
        private readonly IPathSelector _from;
        private readonly IPathSelector _to;

        public RedirectSelector(IPathSelector @from, IPathSelector to)
        {
            _from = @from;
            _to = to;
        }

        public IEnumerable<XPathElement> SelectNodes(IEnumerable<XPathElement> nodes) =>
            _to.SelectNodes(_from.SelectNodes(nodes));
    }

    /// <summary>
    /// Selects all nodes from all paths
    /// </summary>
    class MultiPathSelector : IPathSelector
    {
        private readonly IPathSelector[] _selectors;

        public MultiPathSelector(params IPathSelector[] selectors)
        {
            _selectors = selectors;
        }

        public IEnumerable<XPathElement> SelectNodes(IEnumerable<XPathElement> nodes)
        {
            foreach (var pathElement in _selectors.SelectMany(selector => selector.SelectNodes(nodes)))
                if (pathElement != null)
                    yield return pathElement;
        }
    }

    /// <summary>
    /// select nodes with name
    /// </summary>
    class ByNameSelector : IPathSelector
    {
        private readonly string _nameSelector;

        public ByNameSelector(string nameSelector)
        {
            _nameSelector = nameSelector;
        }

        public IEnumerable<XPathElement> SelectNodes(IEnumerable<XPathElement> nodes) =>
            nodes.Where(x => x.GetNodeName() == _nameSelector);
    }

    /// <summary>
    /// Selects all nodes from list
    /// </summary>
    class SelfSelector : IPathSelector
    {
        public IEnumerable<XPathElement> SelectNodes(IEnumerable<XPathElement> startNode) => startNode;
    }
}