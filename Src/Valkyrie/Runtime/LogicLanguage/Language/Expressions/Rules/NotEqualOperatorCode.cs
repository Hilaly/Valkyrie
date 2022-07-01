using Valkyrie.Language.Ecs;

namespace Valkyrie.Language.Language.Expressions.Rules
{
    class NotEqualOperatorCode : CompOperatorCode
    {
        public NotEqualOperatorCode(IFactRefArgCode left, IFactRefArgCode right) : base(left, right)
        {
        }

        protected override Variable DoOp(Variable l, Variable r)
        {
            return new Variable(l != r);
        }
    }
}