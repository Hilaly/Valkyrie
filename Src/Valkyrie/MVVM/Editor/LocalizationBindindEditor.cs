using UnityEditor;
using Valkyrie.MVVM.Bindings;

namespace Valkyrie.MVVM.Editor
{
    [CustomEditor(typeof(LocalizationBinding))]
    public class LocalizationBindindEditor : AbstractBindingEditor
    {
        protected override void DrawGui()
        {
        }
        /*
        private LocalizationBinding Component => (LocalizationBinding) serializedObject.targetObject;
        
        private SerializedProperty _localizeKey;
        private SerializedProperty _viewModelProperties;

        private SerializedProperty LocaleKey =>
            _localizeKey ?? (_localizeKey = serializedObject.FindProperty(nameof(_localizeKey)));

        public SerializedProperty ViewModelProperties => _viewModelProperties ?? (_viewModelProperties = serializedObject.FindProperty(nameof(_viewModelProperties)));

        private ILocalizationEditor _locales;
        private IList<string> _keys;

        public IList<string> Keys => _keys ?? (_keys = _locales.Keys);

        private void OnEnable()
        {
            _locales = EditorUtils.LoadLocales();
            _localizeKey = serializedObject.FindProperty(nameof(_localizeKey));
            _viewModelProperties = serializedObject.FindProperty(nameof(_viewModelProperties));
        }

        protected override void DrawGui()
        {
            LocaleKey.stringValue =
                EditorUtils.DrawPopup("Localization key",
                    LocaleKey.stringValue,
                    Keys);

            for (var i = 0; i < ViewModelProperties.arraySize; ++i)
            {
                var prop = ViewModelProperties.GetArrayElementAtIndex(i);
                //ViewModel property
                prop.stringValue =
                    EditorUtils.DrawPopup("Model property",
                        prop.stringValue,
                        FindAllViewModelProperties(Component.gameObject));
            }

            if (GUILayout.Button("Add property"))
                ViewModelProperties.arraySize += 1;
            if (ViewModelProperties.arraySize > 0 && GUILayout.Button("Remove last property"))
                ViewModelProperties.arraySize -= 1;
            
            EditorGUILayout.Separator();

            if (GUILayout.Button("Reload keys"))
            {
                _locales = EditorUtils.LoadLocales();
                _keys = null;
            }
        }
        
        List<string> FindAllViewModelProperties(GameObject o)
        {
            var flags = BindingFlags.GetProperty;

            bool ViewModelPropertiesFilter(PropertyInfo info) =>
                info.GetCustomAttribute<BindingAttribute>() != null;

            return FindAllProperties(AllAvailableViewModels(o), flags, ViewModelPropertiesFilter);
        }

        [MenuItem("Valkyrie/Localization/Localize all texts")]
        static void UpdateAllLocales()
        {
            var roots = new HashSet<Transform>(FindObjectsOfType<Transform>().Select(u => u.transform.root));
            var keys = EditorUtils.LoadLocales().Keys;
            bool changed = false;
            foreach (var root in roots)
            {
                foreach (var text in root.GetComponentsInChildren<Text>(true))
                {
                    if(text.GetComponent<LocalizationBinding>() != null)
                        continue;
                    if(text.GetComponents<PropertyBinding>().Any(u => u.IsTwoSided))
                        continue;
                    text.gameObject.AddComponent<LocalizationBinding>();
                    changed = true;
                }
            }
            if(changed)
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
        */
    }
}