using System.Collections.Generic;

namespace NaiveEntity.GamePrototype.EntProto.ViewProto
{
    public interface IConfig
    {
        IEntity Get<T>(string id);
        List<IEntity> Get();
        List<IEntity> Get<T>();
    }
    
    public class ConfigProvider : IConfig
    {
        private readonly EntityContext _ctx;

        public ConfigProvider(EntityContext ctx) => _ctx = ctx;

        public IEntity Get<T>(string id)
        {
            var e = _ctx.Get(id);
            return e == null || !e.HasComponent<T>() ? default : e;
        }

        public List<IEntity> Get() => _ctx.Get();

        public List<IEntity> Get<T>() => _ctx.Get<T>();
    }
}