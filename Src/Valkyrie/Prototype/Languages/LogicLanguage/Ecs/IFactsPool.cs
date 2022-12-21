using System;

namespace Valkyrie.Language.Ecs
{
    public interface IFactsPool
    {
        Span<Fact> All { get; }
        Span<Fact> Changed { get; }
        Span<Fact> NonChanged { get; }
    }
}