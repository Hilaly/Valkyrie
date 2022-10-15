using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Valkyrie.Entities
{
    public interface IEntitiesSerializer
    {
        /// <summary>
        /// Registers type as possible component
        /// </summary>
        /// <param name="type">a class type, inherited from IComponent</param>
        void RegisterComponent(Type type);

        /// <summary>
        /// Creates new entity in entitesContext
        /// </summary>
        /// <param name="entitiesContext">context where entity will create</param>
        /// <param name="jsonText">json string with entity description</param>
        /// <returns>Finalize method, must be called after all entities will be Deserialzied</returns>
        Action Deserialize(EntitiesContext entitiesContext, string jsonText);
        /// <summary>
        /// Write json description of entity
        /// </summary>
        /// <param name="e">the entity to serialize</param>
        /// <param name="formatting">formatting for result json text</param>
        /// <returns>json string with entity description</returns>
        string Serialize(Entity e, Formatting formatting = Formatting.Indented);

        /// <summary>
        /// Write json description of entities collection
        /// </summary>
        /// <param name="es">collection of entities to serialize</param>
        /// <param name="formatting">formatting for result json text</param>
        /// <returns>json string with entity description</returns>
        string Serialize(IEnumerable<Entity> es, Formatting formatting = Formatting.Indented);
    }
}