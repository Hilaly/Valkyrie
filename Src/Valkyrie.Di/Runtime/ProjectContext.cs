using UnityEngine;

namespace Valkyrie.Di
{
    public class ProjectContext : SceneContext
    {
        private static readonly object LockObject = new object();
        private static bool _isInit;

        private static ProjectContext _instance;

        public static ProjectContext Instance
        {
            get
            {
                lock (LockObject)
                {
                    if (_isInit)
                        return _instance;
                    _isInit = true;
                }

                if (_instance != null)
                    return _instance;

                var resource = Resources.Load<ProjectContext>(nameof(ProjectContext));
                if (resource != null)
                    Instantiate(resource);
                else
                    new GameObject(nameof(ProjectContext), typeof(ProjectContext)).hideFlags =
                        HideFlags.HideAndDontSave;

                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
                Destroy(this);
            else
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }

            Container = new Container();

            BuildContainer();
            
            //TODO: for old process
            //Container.Inject(new GameObject("Valkyrie.Core").AddComponent<Core>());
        }
    }
}