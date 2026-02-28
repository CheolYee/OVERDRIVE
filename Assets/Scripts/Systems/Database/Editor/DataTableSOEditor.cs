using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Systems.Database.Editor
{
    [CustomEditor(typeof(AbstractDataTableSO), true)]
    public class DataTableSOEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset editorView = default;
        
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);
            editorView.CloneTree(root);

            root.Q<Button>("GenerateButton").clicked += HandleGenerateButtonClick;
            
            return root;
        }

        private void HandleGenerateButtonClick()
        {
            AbstractDataTableSO tableData = target as AbstractDataTableSO;

            int index = 0;
            foreach (IndexedAsset asset in tableData.AssetList)
            {
                asset.AssetIndex = index++;
                EditorUtility.SetDirty(asset);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}