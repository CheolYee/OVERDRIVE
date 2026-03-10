using System.IO;
using System.Linq;
using Agents.Players.Skills;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Systems.Database.Editor
{
    [CustomEditor(typeof(PlayerSkillDataTableSo))]
    public class PlayerSkillTableEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset editorView = default;
        
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            if (editorView != null)
            {
                editorView.CloneTree(root);
                root.Q<Button>("GenerateButton").clicked += HandleGenerateEnum;
            }
            
            return root;
        }

        private void HandleGenerateEnum()
        {
            PlayerSkillDataTableSo tableAsset = target as PlayerSkillDataTableSo;
            int index = 0;
            string enumString = string.Join(",", tableAsset.AssetList.Select(so =>
            {
                so.AssetIndex = index;
                EditorUtility.SetDirty(so);
                PlayerSkillDataSo skillData = so as PlayerSkillDataSo;
                Debug.Assert(skillData != null, $"플레이어 스킬 테이블에 있는 데이터가 아닙니다. : {so.name}");
                return $"{skillData.idName.ToUpper().Replace(' ', '_')} = {index++}";
            }));
            
            string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            string dirName = Path.GetDirectoryName(scriptPath);
            DirectoryInfo parentDirectory = Directory.GetParent(dirName);
            string path = parentDirectory.FullName;
            string code = string.Format(CodeFormat.EnumFormat, "Systems.Database", tableAsset.enumName, enumString);
            
            File.WriteAllText($"{path}/{tableAsset.enumName}.cs", code);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}