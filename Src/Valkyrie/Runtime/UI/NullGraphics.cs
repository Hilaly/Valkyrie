using UnityEngine;
using UnityEngine.UI;

namespace Valkyrie.UnityExtensions.Components
{
    [AddComponentMenu("UI/Null Graphic", 100)]
    [RequireComponent(typeof(CanvasRenderer))]
    public class NullGraphics : Graphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }

        public override void SetVerticesDirty()
        {
            if (!IsActive())
                return;

            m_OnDirtyVertsCallback?.Invoke();
        }

        public override void SetMaterialDirty()
        {
            if (!IsActive())
                return;

            m_OnDirtyMaterialCallback?.Invoke();
        }
    }
}