namespace Valkyrie.Language.Language.Expressions.Rules
{
    class LessOperatorCode : BaseCompOperator
    {
        public LessOperatorCode(IFactRefArgCode left, IFactRefArgCode right) : base(left, right)
        {
        }

        protected override bool Op(float l, float r)
        {
            return l < r;
        }
    }
}