using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace Agents.Enemies.BT.Events
{
#if UNITY_EDITOR
    [CreateAssetMenu(menuName = "Behavior/Event Channels/StateChannel")]
#endif
    [Serializable, GeneratePropertyBag]
    [EventChannelDescription(name: "StateChannel", message: "Set [CurrentState]", category: "Events", id: "c20637828320bd0906e9aa420817e620")]
    public sealed partial class StateChannel : EventChannel<EnemyState> { }
}

