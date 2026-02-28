using UnityEditor;
using UnityEngine.UIElements;

namespace Agents.Players.Editor
{
    [CustomPropertyDrawer(typeof(LevelDataSo.LevelData))]
    public class LevelDataPropertyView : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();

            IntegerField levelField = new IntegerField("Level")
            {
                bindingPath = "currentLevel",
                style =
                {
                    minWidth = 40, flexGrow = 1, marginRight = 5
                }
            };

            IntegerField requireExpField = new IntegerField("Exp")
            {
                bindingPath = "requiredExp",
                style =
                {
                    minWidth = 40, flexGrow = 1,
                }
            };
            root.Add(levelField);
            root.Add(requireExpField);
            root.style.flexDirection = FlexDirection.Row;
            
            return root;
        }
    }
}