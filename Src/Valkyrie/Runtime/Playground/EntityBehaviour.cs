using System;
using System.Collections.Generic;
using UnityEngine;
using Valkyrie.Di;
using Valkyrie.Playground.Features;

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
            Debug.Assert(this.Get<T>() == null);
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

        #region Context menu

        [ContextMenu("ECS/Move by input joystick")]
        void InitForMoveJoystick()
        {
        }

        #endregion
    }

#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects, UnityEditor.CustomEditor(typeof(EntityBehaviour), editorForChildClasses: true)]
    class EntityBehaviourEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Set("Set as camera target", AddIfNotExist<CameraPointComponent>);
            Set("Can move", AddIfNotExist<MoveAbilityComponent>);
        }

        void Set(string text, Action call)
        {
            if (GUILayout.Button(text))
                call();
        }

        void AddIfNotExist<T>() where T : MonoComponent
        {
            foreach (var o in targets)
            {
                var e = o as EntityBehaviour;
                if (e == null)
                    continue;
                var exist = e.GetComponent<T>();
                if (exist != null)
                    continue;
                e.Add<T>();
                UnityEditor.EditorUtility.SetDirty(o);
            }
        }
    }
#endif
}