using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.Di
{
    public class SceneContext : MonoBehaviour
    {
        [SerializeField] private List<MonoBehaviourInstaller> monoBehavioursInstallers;

        public IContainer Container { get; protected set; }

        void Awake()
        {
            Container = ProjectContext.Instance.Container.CreateChild();

            BuildContainer();
            InjectScene();
        }

        void InjectScene()
        {
            var scene = gameObject.scene;
            foreach (var rootGameObject in scene.GetRootGameObjects())
                Container.InjectGameObject(rootGameObject);
        }

        protected void BuildContainer()
        {
            Container.Register(gameObject.scene).AsSelf();
            
            foreach (var library in Libraries)
                Container.RegisterLibrary(library);

            Container.Build();
        }

        private void OnDestroy()
        {
            Container?.Dispose();
            Container = null;
        }

        IEnumerable<ILibrary> Libraries
        {
            get
            {
                if (monoBehavioursInstallers != null)
                    foreach (var installer in monoBehavioursInstallers)
                        yield return installer;
            }
        }
    }
}