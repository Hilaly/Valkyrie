using Project.Playground.Features;
using UnityEngine;

namespace Valkyrie.Playground.Features.MonoImplementations
{
    class NameComponent : MonoComponent, INameComponent
    {
        [SerializeField] private string lidName;

        public string LidName
        {
            get => lidName;
            set => lidName = value;
        }
    }
}