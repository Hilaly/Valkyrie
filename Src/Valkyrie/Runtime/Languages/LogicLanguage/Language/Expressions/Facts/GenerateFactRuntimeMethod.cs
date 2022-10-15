using System.Collections.Generic;
using Valkyrie.Language.Ecs;

namespace Valkyrie.Language.Language.Expressions.Facts
{
    class GenerateFactRuntimeMethod
    {
        private readonly int _factId;
        private readonly List<IRuntimeExpression> _expressions;
        private readonly string _sentence;
        private readonly byte _ignored;
        private readonly Variable[] _buffer;

        public GenerateFactRuntimeMethod(int factId, List<IRuntimeExpression> expressions, string sentence)
        {
            _factId = factId;
            _expressions = expressions;
            _sentence = sentence;
            _ignored = 0;
            for (var i = 0; i < expressions.Count; ++i)
            {
                if(expressions[i].IsIgnoredOnCompare)
                    _ignored |= (byte)(1 << i);
            }

            _buffer = new Variable[_expressions.Count];
        }

        public void Run(IWorld world, Variable[] localVariables)
        {
            for (int i = 0; i < _expressions.Count; i++)
                _buffer[i] = _expressions[i].Run(world, localVariables);
            
            world.TryAddFact(_factId, _ignored, _buffer);
        }
    }
}