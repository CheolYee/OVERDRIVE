using Gamelib.EventSystem;
using Systems.Database;
using Systems.GameEvents;
using UnityEngine;

namespace Agents.Enemies
{
    public class ItemDropper : MonoBehaviour
    {
        [SerializeField] private DropTableSo itemDropTable;
        
        [field: SerializeField] public EventChannelSO PlayerChannel { get; private set; }

        public void DropItem()
        {
        }
    }
}