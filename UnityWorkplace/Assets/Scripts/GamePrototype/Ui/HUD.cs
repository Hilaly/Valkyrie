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
}