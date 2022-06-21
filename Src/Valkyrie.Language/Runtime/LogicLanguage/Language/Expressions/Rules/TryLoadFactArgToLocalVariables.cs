using Valkyrie.Language.Ecs;

namespace Valkyrie.Language.Language.Expressions.Rules
{
    class TryLoadFactArgToLocalVariables : IFactRefArgCode
    {
        private readonly int _factArgIndex;
        private readonly int _localArgIndex;

        public TryLoadFactArgToLocalVariables(int factArgIndex, int localArgIndex)
        {
            _factArgIndex = factArgIndex;
            _localArgIndex = localArgIndex;
        }

        public Variable TryExecute(IWorld world, Fact fact, Variable[] localVariable)
        {
            var factArg = fact[_factArgIndex];
            if (localVariable[_localArgIndex].IsNull() || localVariable[_localArgIndex] == factArg)
            {
                localVariable[_localArgIndex] = factArg;
                return new Variable(true);
            }

            return new Variable(false);
        }
    }
}