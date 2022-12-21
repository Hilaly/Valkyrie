namespace Valkyrie.Language.Language.Expressions.Rules
{
    class MoreOperatorCode : BaseCompOperator
    {
        public MoreOperatorCode(IFactRefArgCode left, IFactRefArgCode right) : base(left, right)
        {
        }

        protected override bool Op(float l, float r)
        {
            return l > r;
        }
    }
}