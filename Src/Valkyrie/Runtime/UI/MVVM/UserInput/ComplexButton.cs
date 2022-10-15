using System.Linq;

namespace Valkyrie.UserInput
{
    class ComplexButton : GenericInnerListOwner<IVirtualButton>, IVirtualButton
    {
        public bool IsPressed()
        {
            return Values.Any(u => u.IsPressed());
        }
    }
}