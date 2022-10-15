using System;
using System.Collections.Generic;
using Valkyrie.Language.Ecs;
using Valkyrie.Language.Language;

namespace Valkyrie.Language
{
    public interface ILogic
    {
        void AddRule(IRule rule);
        void AddStartupRule(IRule rule);

        void Init();
        void Simulate();
    }

    public interface IRequestApi
    {
        void AddWorldQuery(string name, IWorldQuery worldQuery);
        IWorldQuery GetWorldQuery(string name);
    }

    public interface IWorld : IDataProvider, ILogic, IRequestApi
    {
        void PushFact(Fact fact);
        void AddFact(Fact fact, bool skipIfExist);

        IFactsPool GetFacts(int factId);
    }

    public interface IWorldQueryResult : IEnumerable<Variable>
    {
        Variable this[string name] { get; }
        Variable this[int index] { get; }
        
        int Count { get; }
    }

    public interface IWorldQuery
    {
        Span<IWorldQueryResult> Request();
    }
}