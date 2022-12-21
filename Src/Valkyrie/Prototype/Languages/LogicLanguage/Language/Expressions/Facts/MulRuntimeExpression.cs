namespace Valkyrie.Language.Language.Expressions.Facts
{
    class MulRuntimeExpression : DuoRuntimeExpression
    {
        public MulRuntimeExpression(IRuntimeExpression first, IRuntimeExpression second) : base(first, second)
        {
        }

        protected override float DoOp(float f, float s) => f * s;
    }
}