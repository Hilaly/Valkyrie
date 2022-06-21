namespace Valkyrie.Language.Language.Expressions.Rules
{
    class LessOrEqualOperatorCode : BaseCompOperator
    {
        public LessOrEqualOperatorCode(IFactRefArgCode left, IFactRefArgCode right) : base(left, right)
        {
        }

        protected override bool Op(float l, float r)
        {
            return l <= r;
        }
    }
}