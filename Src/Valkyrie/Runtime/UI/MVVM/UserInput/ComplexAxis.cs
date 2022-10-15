using System.Linq;

namespace Valkyrie.UserInput
{
    class ComplexAxis : GenericInnerListOwner<IVirtualAxis>, IVirtualAxis
    {
        public float Value
        {
            get { return Values.Sum(u => u.Value); }
        }
    }
}