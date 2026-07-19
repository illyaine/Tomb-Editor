using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TombLib.LevelData.VisualScripting;
using TombLib.Utils;

namespace TombLib.LevelData
{
    /// <summary>
    /// Defines the editor-side contract for TEN Effect Boxes.
    ///
    /// Effect Boxes reuse box-volume transforms and existing event/node serialization.
    /// Their private volume event set stores the authoring graph and has no activators, so
    /// it cannot behave like a normal trigger volume. During TEN compilation, supported
    /// Effect Box nodes are converted into temporary global loop events. The project data
    /// stays editor-owned and TombEngine requires no dedicated Effect Box object type.
    /// </summary>
    public static class EffectBoxUtils
    {
        public const string EventSetPrefix = "__TEN_EFFECT_BOX__";

        private static readonly string[] _effectKeywords =
        {
            "particle", "effect", "lightning", "shockwave", "spark", "smoke",
            "fire", "flame", "blood", "bubble", "ripple", "splash", "weather",
            "rain", "snow", "fog", "mist", "flash", "earthquake", "rumble",
            "light", "sound", "camera shake", "screen"
        };

        public static bool IsEffectBoxEventSet(EventSet eventSet)
        {
            return eventSet != null &&
                   !string.IsNullOrEmpty(eventSet.Name) &&
                   eventSet.Name.StartsWith(EventSetPrefix, StringComparison.Ordinal);
        }

        public static bool IsEffectBox(this VolumeInstance volume)
        {
            return volume is BoxVolumeInstance && IsEffectBoxEventSet(volume.EventSet);
        }

        public static string CreateEventSetName()
        {
            return EventSetPrefix + Guid.NewGuid().ToString("N");
        }

        public static BoxVolumeInstance Create(LevelSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var eventSet = new VolumeEventSet
            {
                Name = CreateEventSetName(),
                Activators = VolumeActivators.None,
                LastUsedEvent = EventType.OnVolumeInside
            };

            foreach (var entry in eventSet.Events)
            {
                entry.Value.Enabled = false;
                entry.Value.Mode = EventSetMode.NodeEditor;
                entry.Value.CallCounter = 0;
            }

            settings.VolumeEventSets.Add(eventSet);

            return new BoxVolumeInstance
            {
                Enabled = true,
                DetectInAdjacentRooms = false,
                EventSet = eventSet
            };
        }

        public static Event GetGraphEvent(this VolumeInstance volume)
        {
            if (!volume.IsEffectBox())
                return null;

            var set = (VolumeEventSet)volume.EventSet;
            set.Activators = VolumeActivators.None;
            set.LastUsedEvent = EventType.OnVolumeInside;

            var graphEvent = set.Events[EventType.OnVolumeInside];
            graphEvent.Enabled = false;
            graphEvent.Mode = EventSetMode.NodeEditor;
            return graphEvent;
        }

        public static IReadOnlyList<NodeFunction> GetAvailableEffectFunctions()
        {
            return ScriptingUtils.NodeFunctions
                .Where(IsEffectFunction)
                .OrderByDescending(function =>
                    string.Equals(function.Signature, EffectBoxRuntimeBuilder.ParticleEmitterFunction, StringComparison.Ordinal))
                .ThenBy(function => function.Section)
                .ThenBy(function => function.Name)
                .ToList();
        }

        public static bool IsEffectFunction(NodeFunction function)
        {
            if (function == null || function.Conditional)
                return false;

            var searchableText = string.Join(" ",
                function.Name ?? string.Empty,
                function.Section ?? string.Empty,
                function.Signature ?? string.Empty,
                function.Description ?? string.Empty).ToLowerInvariant();

            return _effectKeywords.Any(searchableText.Contains);
        }

        public static TriggerNodeAction CreateNode(NodeFunction function, int sequenceNumber)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            bool isRuntimeEmitter = string.Equals(
                function.Signature,
                EffectBoxRuntimeBuilder.ParticleEmitterFunction,
                StringComparison.Ordinal);

            var node = new TriggerNodeAction
            {
                Name = string.IsNullOrWhiteSpace(function.Name)
                    ? "Effect " + sequenceNumber
                    : function.Name,
                Function = function.Signature,
                Size = TriggerNode.DefaultSize,
                Color = isRuntimeEmitter
                    ? new Vector3(0.62f, 0.48f, 0.06f)
                    : new Vector3(0.48f, 0.39f, 0.08f)
            };

            node.FixArguments(function);
            return node;
        }

        public static string GetDisplayName(this VolumeInstance volume)
        {
            if (!volume.IsEffectBox())
                return volume?.ShortName() ?? "Effect box";

            return string.IsNullOrWhiteSpace(volume.LuaName)
                ? "Effect box"
                : "Effect box '" + volume.LuaName + "'";
        }
    }
}
