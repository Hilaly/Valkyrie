using System;
using Valkyrie.Language.Ecs;

namespace Valkyrie.Language.Language.Expressions.Rules
{
    class MathOpExprCode : CompOperatorCode
    {
        private readonly Func<float, float, float> _call;

        public MathOpExprCode(IFactRefArgCode left, IFactRefArgCode right, Func<float, float, float> call) : base(left, right)
        {
            _call = call;
        }

        protected override Variable DoOp(Variable l, Variable r)
        {
            return new Variable(_call(ConvertToFloat(l), ConvertToFloat(r)));
        }
    }
    abstract class BaseCompOperator : CompOperatorCode
    {
        protected BaseCompOperator(IFactRefArgCode left, IFactRefArgCode right) : base(left, right)
        {
        }

        protected override Variable DoOp(Variable l, Variable r) => Op(ConvertToFloat(l), ConvertToFloat(r));

        protected abstract bool Op(float l, float r);
    }

    abstract class CompOperatorCode : DefaultExprCode
    {
        private readonly IFactRefArgCode _left;
        private readonly IFactRefArgCode _right;

        protected CompOperatorCode(IFactRefArgCode left, IFactRefArgCode right)
        {
            _left = left;
            _right = right;
        }

        protected float ConvertToFloat(Variable l)
        {
            var lf = l.IsFloat()
                ? l.AsFloat()
                : l.IsInt()
                    ? (float)l.AsInt()
                    : l.IsBool()
                        ? 1
                        : 0;
            return lf;
        }

        public override Variable TryExecute(IWorld world, Fact fact, Variable[] localVariable)
        {
            var l = _left.TryExecute(world, fact, localVariable);
            var r = _right.TryExecute(world, fact, localVariable);
            return DoOp(l, r);
        }

        protected abstract Variable DoOp(Variable l, Variable r);
    }
}