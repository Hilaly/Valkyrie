using System;
using System.Collections.Generic;
using UnityEngine;
using Valkyrie.Di;

namespace Valkyrie.Playground
{
    public interface IPositionComponent : IComponent
    {
        public Vector3 Position { get; set; }
    }

    public interface IRotationComponent : IComponent
    {
        public Vector3 Direction { get; set; }
        public Quaternion Rotation { get; set; }
    }
    public interface ITransformComponent : IPositionComponent, IRotationComponent
    {
    }

    public interface IEntity
    {
        string Id { get; }
        T Get<T>() where T : IComponent;
        IReadOnlyList<T> GetAll<T>() where T : IComponent;
        T Add<T>() where T : MonoComponent;
    }

    [SelectionBase]
    public class EntityBehaviour : MonoBehaviour, IEntity, ITransformComponent
    {
        [Inject] private GameState _gameState;

        private IDisposable _disposable;

        string IEntity.Id => gameObject.GetInstanceID().ToString();
        T IEntity.Get<T>() => gameObject.GetComponent<T>();
        IReadOnlyList<T> IEntity.GetAll<T>() => gameObject.GetComponents<T>();
        public T Add<T>() where T : MonoComponent
        {
            UnityEngine.Debug.Assert(this.Get<T>() == null);
            return gameObject.AddComponent<T>();
        }

        #region Unity events

        private void OnEnable() => _disposable = _gameState.Register(this);

        private void OnDisable()
        {
            _disposable?.Dispose();
            _disposable = null;
        }

        #endregion

        #region ITransform Component

        IEntity IComponent.Entity => this;
        
        public Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;
        }

        public Vector3 Direction
        {
            get => transform.forward;
            set => transform.forward = value;
        }

        public Quaternion Rotation
        {
            get => transform.rotation;
            set => transform.rotation = value;
        }
        
        #endregion
    }
}