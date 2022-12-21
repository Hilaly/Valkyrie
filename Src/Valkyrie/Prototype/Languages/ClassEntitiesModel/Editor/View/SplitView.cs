using UnityEngine.UIElements;

namespace Valkyrie.View
{
    public class SplitView : TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<SplitView, UxmlTraits> { }

        public SplitView()
        {
        }
    }
}