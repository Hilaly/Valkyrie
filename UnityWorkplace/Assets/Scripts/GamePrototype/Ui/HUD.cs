using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using Valkyrie.MVVM;
using Valkyrie.UserInput.UnitySpecific;

namespace GamePrototype.Ui
{
    [Binding]
    public class HUD : MonoBehaviour
    {
        [SerializeField] private Joystick2Axis moveJoystick;
    }
    
    
    [Binding]
    class OrientationViewModel
    {
        [Binding] public bool IsLandscape { get; set; }
        [Binding] public bool IsPortrait { get; set; }

        void LateUpdate()
        {
            IsLandscape = Screen.width > Screen.height;
            IsPortrait = !IsLandscape;
        }
    }
}