using Modules;
using UnityEngine;

namespace CombatSystem
{
    public struct DamageData
    {
        public float DamageAmount;
        public bool IsCritical;
        public ModuleOwner Dealer;
        public Vector2 DirectedKbForce;
    }
}