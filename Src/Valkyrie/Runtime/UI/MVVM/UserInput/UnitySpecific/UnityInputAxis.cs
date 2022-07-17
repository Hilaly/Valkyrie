namespace Valkyrie.UserInput.UnitySpecific
{
    public class UnityInputAxis : IVirtualAxis
    {
        private readonly string _unityAxisId;

        public UnityInputAxis(string unityAxisId)
        {
            _unityAxisId = unityAxisId;
        }

        public float Value => UnityEngine.Input.GetAxis(_unityAxisId);
    }
}