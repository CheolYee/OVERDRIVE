using UnityEngine;

namespace Systems.Database
{
    public abstract class IndexedAsset : ScriptableObject
    {
        [field: SerializeField] public int AssetIndex { get; set; }
    }
}