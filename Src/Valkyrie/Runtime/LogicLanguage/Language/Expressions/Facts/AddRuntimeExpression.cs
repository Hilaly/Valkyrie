namespace Valkyrie.Language.Language.Expressions.Facts
{
    class AddRuntimeExpression : DuoRuntimeExpression
    {
        public AddRuntimeExpression(IRuntimeExpression first, IRuntimeExpression second) : base(first, second)
        {
        }

        protected override float DoOp(float f, float s) => f + s;
    }
}