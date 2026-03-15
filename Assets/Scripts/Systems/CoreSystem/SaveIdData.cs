using Systems.Database;
using UnityEngine;

namespace Systems.CoreSystem
{
    [CreateAssetMenu(fileName = "Save ID", menuName = "System/Save ID", order = 0)]
    public class SaveIdData : IndexedAsset
    {
        public int Id { get => AssetIndex; private set => AssetIndex = value; }
        [SerializeField, TextArea] private string description;
    }
}