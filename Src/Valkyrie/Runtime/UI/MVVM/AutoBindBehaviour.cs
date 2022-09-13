using UnityEngine;

namespace Valkyrie.MVVM
{
    public class AutoBindBehaviour : MonoBehaviour
    {
        private void Awake()
        {
            gameObject.InjectAutoBind();
        }
    }
}