namespace Valkyrie.UserInput
{
    public class ButtonsVirtualAxis : IVirtualAxis
    {
        private readonly IVirtualButton _up;
        private readonly IVirtualButton _down;

        public ButtonsVirtualAxis(IVirtualButton up, IVirtualButton down)
        {
            _up = up;
            _down = down;
        }

        public float Value
        {
            get
            {
                var result = 0f;
                if (_up.IsPressed())
                    result += 1f;
                if (_down.IsPressed())
                    result -= 1f;
                return result;
            }
        }
    }
}