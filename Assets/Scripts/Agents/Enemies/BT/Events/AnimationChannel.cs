using System;
using Systems.AnimationSystems;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace Agents.Enemies.BT.Events
{
#if UNITY_EDITOR
    [CreateAssetMenu(menuName = "Behavior/Event Channels/AnimationChannel")]
#endif
    [Serializable, GeneratePropertyBag]
    [EventChannelDescription(name: "AnimationChannel", message: "Set animtion to [Clip]", category: "Events", id: "ea2b8e44165f89697989baa225ac89fa")]
    public sealed partial class AnimationChannel : EventChannel<AnimParamSO> { }
}

