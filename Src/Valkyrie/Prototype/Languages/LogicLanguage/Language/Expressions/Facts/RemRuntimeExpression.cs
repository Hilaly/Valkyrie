namespace Valkyrie.Language.Language.Expressions.Facts
{
    class RemRuntimeExpression : DuoRuntimeExpression
    {
        public RemRuntimeExpression(IRuntimeExpression first, IRuntimeExpression second) : base(first, second)
        {
        }

        protected override float DoOp(float f, float s) => f - s;
    }
}