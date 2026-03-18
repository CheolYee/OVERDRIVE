using System.Collections.Generic;
using System.IO;
using System.Text;
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
            if (tableAsset == null || tableAsset.AssetList == null)
                return;

            int assetIndex = 0;
            int enumValue = 0;

            List<string> enumEntries = new();
            HashSet<string> usedEnumNames = new();

            foreach (var so in tableAsset.AssetList)
            {
                if (so == null)
                    continue;

                so.AssetIndex = assetIndex++;
                EditorUtility.SetDirty(so);

                PlayerSkillDataSo skillData = so as PlayerSkillDataSo;
                Debug.Assert(skillData != null, $"플레이어 스킬 테이블에 있는 데이터가 아닙니다. : {so.name}");
                if (skillData == null)
                    continue;

                string enumName = ToEnumName(skillData.idName);

                if (string.IsNullOrWhiteSpace(enumName))
                    enumName = $"SKILL_{enumValue}";

                if (usedEnumNames.Add(enumName))
                {
                    enumEntries.Add($"{enumName} = {enumValue++}");
                }
            }

            string enumString = string.Join(",\n        ", enumEntries);

            string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            string dirName = Path.GetDirectoryName(scriptPath);
            DirectoryInfo parentDirectory = Directory.GetParent(dirName);
            string path = parentDirectory.FullName;

            string code = string.Format(
                CodeFormat.EnumFormat,
                "Systems.Database",
                tableAsset.enumName,
                enumString);

            File.WriteAllText(Path.Combine(path, $"{tableAsset.enumName}.cs"), code, Encoding.UTF8);

            EditorUtility.SetDirty(tableAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static string ToEnumName(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName))
                return string.Empty;

            StringBuilder builder = new StringBuilder();
            bool lastWasUnderscore = false;

            foreach (char c in rawName.Trim().ToUpperInvariant())
            {
                if (char.IsLetterOrDigit(c))
                {
                    builder.Append(c);
                    lastWasUnderscore = false;
                }
                else
                {
                    if (!lastWasUnderscore)
                    {
                        builder.Append('_');
                        lastWasUnderscore = true;
                    }
                }
            }

            string result = builder.ToString().Trim('_');

            if (string.IsNullOrEmpty(result))
                return string.Empty;

            if (char.IsDigit(result[0]))
                result = "_" + result;

            return result;
        }
    }
}