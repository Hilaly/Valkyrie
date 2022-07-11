using System;
using UnityEngine;

namespace Valkyrie.MVVM
{
    public class AutoBindBehaviour : MonoBehaviour
    {
        private void Awake()
        {
            DataExtensions.InjectAutoBind(gameObject);
        }
    }
}