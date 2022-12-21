namespace Valkyrie.UserInput
{
    public interface IMoveJoystick : IVirtualJoystick
    {
        bool CruiseControl { get; set; }
        bool IsDynamic { get; set; }
    }
}