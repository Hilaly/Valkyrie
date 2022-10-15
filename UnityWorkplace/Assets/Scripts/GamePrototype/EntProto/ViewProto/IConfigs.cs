using System.Collections.Generic;

namespace NaiveEntity.GamePrototype.EntProto.ViewProto
{
    public interface IConfig
    {
        IEntity Get<T>(string id) where T : IComponent;
        List<IEntity> Get();
        List<IEntity> Get<T>() where T : IComponent;
    }
    
    public class ConfigProvider : IConfig
    {
        private readonly EntityContext _ctx;

        public ConfigProvider(EntityContext ctx) => _ctx = ctx;

        public IEntity Get<T>(string id) where T : IComponent
        {
            var e = _ctx.Get(id);
            return e == null || !e.HasComponent<T>() ? default : e;
        }

        public List<IEntity> Get() => _ctx.Get();

        public List<IEntity> Get<T>() where T : IComponent => _ctx.Get<T>();
    }
}