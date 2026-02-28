using System;
using UnityEngine;

namespace Systems.Database
{
    [CreateAssetMenu(fileName = "UI Data", menuName = "System/UI data", order = 20)]
    public class UIDataSo : IndexedAsset
    {
        public string uiName;
        public int hashValue;
        public string displayName;
        public string hierarchyName;

        private void OnValidate()
        {
            if (!string.IsNullOrEmpty(uiName))
            {
                hashValue = Animator.StringToHash(uiName);
            }
        }
    }
}