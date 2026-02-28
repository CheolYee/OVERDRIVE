using UnityEngine;

namespace Systems.CoreSystem
{
    [CreateAssetMenu(fileName = "Save ID", menuName = "System/Save ID", order = 0)]
    public class SaveIdData : ScriptableObject
    {
        [field: SerializeField] public int Id { get; private set; }
        [SerializeField, TextArea] private string description;
    }
}