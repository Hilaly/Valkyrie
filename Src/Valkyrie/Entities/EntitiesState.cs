using System;
using System.Collections.Generic;

namespace Valkyrie.Entities
{
    public class EntitiesState
    {
        private readonly EntitiesConfigService _configService;
        private readonly EntitiesContext _context;

        public EntitiesState(EntitiesConfigService configService)
        {
            _configService = configService;
            _context = new EntitiesContext(null);
        }

        public Entity CreateEntity(string id)
        {
            var r = new Entity(id);
            _context.Add(r);
            return r;
        }

        public Entity CreateFromTemplate(string id, string templateName)
        {
            var template = _configService.Context.GetEntity(templateName);
            var r = CreateEntity(id);
            
            r._templates.Add(template);
            foreach (var slot in template._slots) 
                r._slots.Add(slot.Key, slot.Value);
            
            foreach (var component in template.CollectComponents(true)) 
                r.AddComponent(component.MakeCopy());
            
            return r;
        }

        public Entity CreateFromTemplate(string templateName) =>
            CreateFromTemplate(Guid.NewGuid().ToString(), templateName);

        public void Destroy(Entity e)
        {
            _context.Destroy(e);
        }

        public List<Entity> GetAll() => _context.GetEntities(false);
    }
}