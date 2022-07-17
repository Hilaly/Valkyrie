using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.UserInput.UnitySpecific
{
    class UnityControls : ComplexControls, IUnityControls
    {
        private readonly IInput _input;

        public UnityControls(IInput input)
        {
            _input = input;
        }

        public List<Touch> GetTouches()
        {
            //TODO: implement logic
            return _input.GetTouches();
        }
    }
}