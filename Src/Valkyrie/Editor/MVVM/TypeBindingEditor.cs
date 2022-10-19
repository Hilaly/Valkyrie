using UnityEditor;
using Utils;
using Valkyrie.MVVM.Bindings;

namespace Valkyrie.MVVM.Editor
{
    [CustomEditor(typeof(TypeBinding), true)]
    public class TypeBindingEditor : AbstractBindingEditor
    {
        private SerializedProperty _templateClass;

        private void OnEnable()
        {
            _templateClass = serializedObject.FindProperty(nameof(_templateClass));
        }

        protected override void DrawGui()
        {
            var typeNames = GetAllSubTypes<object>(u =>
                    !u.IsAbstract && u.GetCustomAttribute<BindingAttribute>(true) != null)
                .ConvertAll(u => u.FullName);

            _templateClass.stringValue = EditorUtils.DrawPopup("Model", _templateClass.stringValue, typeNames);
        }
    }
}