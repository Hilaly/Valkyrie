using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.Playground
{
    public interface IEntitiesDatabase
    {
        IReadOnlyList<IEntity> GetAll();
    }

    [CreateAssetMenu(menuName = "Valkyrie/Entities/Database")]
    public class EntitiesDatabase : ScriptableObject, IEntitiesDatabase
    {
        [SerializeField] private List<EntityBehaviour> prefabs = new();

        public IReadOnlyList<IEntity> GetAll() => prefabs;
    }
}