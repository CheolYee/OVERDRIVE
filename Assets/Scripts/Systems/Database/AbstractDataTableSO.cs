using UnityEngine;

namespace Systems.Database
{
    public abstract class AbstractDataTableSO : ScriptableObject
    {
        [field: SerializeField] public string TableName { get; private set; }
        [field: SerializeField] public IndexedAsset[] AssetList { get; private set; } 
    }
}