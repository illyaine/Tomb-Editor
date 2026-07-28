using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.LevelData.VisualScripting;
using TombLib.Utils;

namespace TombLib.LevelData
{
    /// <summary>
    /// Converts editor-only Effect Box graphs into temporary TEN global loop events.
    /// The generated event sets exist only during compilation and are never written back
    /// to the PRJ2 project. TombEngine therefore needs no dedicated Effect Box runtime type.
    /// </summary>
    public static class EffectBoxRuntimeBuilder
    {
        public const string ParticleEmitterFunction = "EffectBoxParticleEmitter";
        public const string ParticleEmitterRuntimeFunction = "__EffectBoxParticleEmitterRuntime";
        public const string RuntimeEventSetPrefix = "__TEN_EFFECT_BOX_RUNTIME__";

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static List<EventSet> BuildGlobalEventSets(Level level)
        {
            if (level == null)
                throw new ArgumentNullException(nameof(level));

            var result = new List<EventSet>(level.Settings.GlobalEventSets);

            foreach (var effectBox in level.GetAllObjects()
                .OfType<BoxVolumeInstance>()
                .Where(volume => volume.IsEffectBox()))
            {
                if (string.IsNullOrWhiteSpace(effectBox.LuaName))
                {
                    _logger.Warn("Effect Box has no Lua name and cannot emit particles at runtime.");
                    continue;
                }

                var runtimeNodes = BuildRuntimeNodes(effectBox);
                if (runtimeNodes.Count == 0)
                    continue;

                var runtimeSet = new GlobalEventSet
                {
                    Name = RuntimeEventSetPrefix + GetStableIdentifier(effectBox),
                    LastUsedEvent = EventType.OnLoop
                };

                var loopEvent = runtimeSet.Events[EventType.OnLoop];
                loopEvent.Mode = EventSetMode.NodeEditor;
                loopEvent.Enabled = true;
                loopEvent.CallCounter = 0;
                loopEvent.Nodes = runtimeNodes;

                result.Add(runtimeSet);
            }

            return result;
        }

        public static bool IsRuntimeSupported(TriggerNode node)
        {
            return node is TriggerNodeAction &&
                   string.Equals(node.Function, ParticleEmitterFunction, StringComparison.Ordinal);
        }

        private static List<TriggerNode> BuildRuntimeNodes(BoxVolumeInstance effectBox)
        {
            var graphEvent = effectBox.GetGraphEvent();
            if (graphEvent == null || graphEvent.Nodes.Count == 0)
                return new List<TriggerNode>();

            var allNodes = TriggerNode.LinearizeNodes(graphEvent.Nodes);
            var sourceNodes = allNodes.Where(IsRuntimeSupported).ToList();
            int unsupportedCount = allNodes.Count - sourceNodes.Count;

            if (unsupportedCount > 0)
            {
                _logger.Warn(
                    "Effect Box '{0}' contains {1} node(s) without automatic runtime support. " +
                    "Only Effect Box particle emitters are compiled in this implementation block.",
                    effectBox.LuaName,
                    unsupportedCount);
            }

            var result = new List<TriggerNode>(sourceNodes.Count);
            string stableIdentifier = GetStableIdentifier(effectBox);

            for (int index = 0; index < sourceNodes.Count; index++)
            {
                var source = sourceNodes[index];
                var runtimeNode = new TriggerNodeAction
                {
                    Name = source.Name,
                    Size = source.Size,
                    Color = source.Color,
                    Locked = source.Locked,
                    Function = ParticleEmitterRuntimeFunction,
                    ScreenPosition = source.ScreenPosition
                };

                runtimeNode.Arguments.Add(new TriggerNodeArgument
                {
                    Name = "volumeName",
                    Value = TextExtensions.Quote(effectBox.LuaName)
                });
                runtimeNode.Arguments.Add(new TriggerNodeArgument
                {
                    Name = "emitterKey",
                    Value = TextExtensions.Quote(stableIdentifier + "_" + index)
                });
                runtimeNode.Arguments.AddRange(source.Arguments);
                result.Add(runtimeNode);
            }

            return result;
        }

        private static string GetStableIdentifier(BoxVolumeInstance effectBox)
        {
            string definitionIdentifier = "definition";
            if (effectBox.EventSet != null &&
                !string.IsNullOrWhiteSpace(effectBox.EventSet.Name) &&
                effectBox.EventSet.Name.StartsWith(EffectBoxUtils.EventSetPrefix, StringComparison.Ordinal))
            {
                definitionIdentifier = effectBox.EventSet.Name.Substring(EffectBoxUtils.EventSetPrefix.Length);
            }

            // Definitions are intentionally shared by many placed boxes. The persistent and
            // unique Lua name keeps generated event-set names and interval state independent
            // for every placement while retaining the definition identity for diagnostics.
            string instanceIdentifier = !string.IsNullOrWhiteSpace(effectBox.LuaName)
                ? effectBox.LuaName
                : unchecked((uint)effectBox.GetHashCode()).ToString();

            return definitionIdentifier + "_" + instanceIdentifier;
        }
    }
}
