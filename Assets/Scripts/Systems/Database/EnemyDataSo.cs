using Gamelib.ObjectPool.Runtime;
using UnityEngine;

namespace Systems.Database
{
    [CreateAssetMenu(fileName = "Enemy Data", menuName = "Enemy/Enemy Data", order = 5)]
    public class EnemyDataSo : IndexedAsset
    {
        public string enemyName;
        public PoolItemSo enemyPoolItem;
        public RuntimeAnimatorController animatorController;
    }
}