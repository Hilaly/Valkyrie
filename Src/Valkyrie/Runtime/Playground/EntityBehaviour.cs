using System;
using System.Collections.Generic;
using UnityEngine;
using Valkyrie.Di;

namespace Valkyrie.Playground
{
    public interface IEntity
    {
        string Id { get; }
        IReadOnlyList<T> Get<T>() where T : IComponent;
    }

    [SelectionBase]
    public class EntityBehaviour : MonoBehaviour, IEntity
    {
        [Inject] private GameState _gameState;

        private IDisposable _disposable;
        
        public string Id => gameObject.GetInstanceID().ToString();
        
        public IReadOnlyList<T> Get<T>() where T : IComponent => 
            gameObject.GetComponents<T>();

        private void OnEnable() => _disposable = _gameState.Register(this);

        private void OnDisable()
        {
            _disposable?.Dispose();
            _disposable = null;
        }
    }
}