using System;
using System.Collections.Generic;
using System.Linq;

namespace Valkyrie.Language.Language
{
    class ActionRule : IRule
    {
        private readonly Action<IWorld, List<int>> _call;
        private readonly int[] _depends;

        public ActionRule(Action<IWorld, List<int>> call, int[] depends)
        {
            _call = call;
            _depends = depends;
        }

        public bool IsDependsOn(List<int> factId)
        {
            return factId.Any(x => _depends.Contains(x));
        }

        public void Run(IWorld world, List<int> changedTypes) =>
            _call(world, changedTypes);

        public void RunAll(IWorld world) => _call(world, new List<int>());
    }

    class StartupActionRule : IRule
    {
        private readonly Action<IWorld> _call;

        public StartupActionRule(Action<IWorld> call)
        {
            _call = call;
        }

        public bool IsDependsOn(List<int> factId) => false;

        public void Run(IWorld world, List<int> changedTypes) => _call(world);
        public void RunAll(IWorld world) => _call(world);
    }
}