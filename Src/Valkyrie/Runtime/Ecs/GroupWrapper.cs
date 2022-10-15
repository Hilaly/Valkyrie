using System.Collections.Generic;

namespace Valkyrie.Ecs
{
    public class GroupWrapper
    {
        private readonly IEcsGroup _group;
        private readonly List<EcsEntity> _buffer = new();

        public GroupWrapper(IEcsGroup @group)
        {
            _group = @group;
        }

        public GroupWrapper(IGroupBuilder builder) : this(builder.Build())
        {
        }

        public List<EcsEntity> Entities => _group.GetEntities(_buffer);
        public EcsEntity First => Entities.Find(x => true);

        public static GroupWrapper Wrap(IGroupBuilder builder) => new GroupWrapper(builder);
        public static GroupWrapper Wrap(IEcsGroup group) => new GroupWrapper(group);
    }
}