using System.Text;
using Systems.Database;
using UnityEngine;

namespace ItemSystem
{
    public abstract class AbstractItemDataSo : IndexedAsset
    {
        public string itemName;
        public Sprite itemIcon;
        public int maxStack;

        [Range(0, 100f)] public float dropRate;
        
        protected StringBuilder StringBuilder = new StringBuilder();
        
        public virtual string GetDescription() => string.Empty;
    }
}