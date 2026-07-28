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
            graphEvent.CallCounter = 0;
            return graphEvent;
        }

        /// <summary>
        /// Returns the existing TEN effects and editor-side starting presets for the
        /// supported Effect Box particle emitter. Presets use the same function signature
        /// and argument layout, so they become normal editable NodeEditor nodes after insertion.
        /// They do not add or modify any TombEngine particle API.
        /// </summary>
        public static IReadOnlyList<NodeFunction> GetAvailableEffectFunctions()
        {
            var existingFunctions = ScriptingUtils.NodeFunctions
                .Where(IsEffectFunction)
                .ToList();

            var presets = CreateParticleEmitterPresets(existingFunctions);

            return existingFunctions
                .Concat(presets)
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
                    ? GetEmitterNodeColor(function.Section)
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

        private static Vector3 GetEmitterNodeColor(string section)
        {
            section = section ?? string.Empty;

            if (section.IndexOf("Fire", StringComparison.OrdinalIgnoreCase) >= 0)
                return new Vector3(0.62f, 0.22f, 0.04f);
            if (section.IndexOf("Water", StringComparison.OrdinalIgnoreCase) >= 0)
                return new Vector3(0.08f, 0.34f, 0.56f);
            if (section.IndexOf("Weather", StringComparison.OrdinalIgnoreCase) >= 0)
                return new Vector3(0.42f, 0.48f, 0.56f);
            if (section.IndexOf("Magic", StringComparison.OrdinalIgnoreCase) >= 0)
                return new Vector3(0.38f, 0.12f, 0.56f);

            return new Vector3(0.62f, 0.48f, 0.06f);
        }

        private static IReadOnlyList<NodeFunction> CreateParticleEmitterPresets(IReadOnlyList<NodeFunction> functions)
        {
            var emitter = functions.FirstOrDefault(function =>
                string.Equals(function.Signature, EffectBoxRuntimeBuilder.ParticleEmitterFunction, StringComparison.Ordinal));

            if (emitter == null)
                return Array.Empty<NodeFunction>();

            return new List<NodeFunction>
            {
                CreateEmitterPreset(
                    emitter,
                    "Basic particle emitter",
                    "Presets - General",
                    "Neutral Effect Box particle emitter with conservative values.",
                    Defaults(
                        ("distribution", "1"),
                        ("burstCount", "1"),
                        ("intervalMin", "0.25"),
                        ("intervalMax", "0.25"),
                        ("velocity", "TEN.Vec3(0, 0, 0)"),
                        ("velocitySpread", "TEN.Vec3(0, 0, 0)"),
                        ("startSize", "10"),
                        ("endSize", "0"),
                        ("lifeMin", "2"),
                        ("lifeMax", "2"))),

                CreateEmitterPreset(
                    emitter,
                    "Fire and embers",
                    "Presets - Fire and smoke",
                    "Fast additive particles emitted from the bottom face of the box. Adjust the sprite sequence to a flame or spark sprite.",
                    Defaults(
                        ("distribution", "3"),
                        ("burstCount", "2"),
                        ("intervalMin", "0.08"),
                        ("intervalMax", "0.18"),
                        ("velocity", "TEN.Vec3(0, -120, 0)"),
                        ("velocitySpread", "TEN.Vec3(28, 45, 28)"),
                        ("rotateVelocity", "true"),
                        ("startColor", "TEN.Color(255, 190, 48)"),
                        ("endColor", "TEN.Color(180, 24, 4)"),
                        ("blendID", "2"),
                        ("startSize", "22"),
                        ("endSize", "4"),
                        ("lifeMin", "0.45"),
                        ("lifeMax", "0.9"),
                        ("rotVel", "180"))),

                CreateEmitterPreset(
                    emitter,
                    "Smoke plume",
                    "Presets - Fire and smoke",
                    "Slow alpha-blended particles that grow while rising from the bottom face of the box.",
                    Defaults(
                        ("distribution", "3"),
                        ("burstCount", "1"),
                        ("intervalMin", "0.2"),
                        ("intervalMax", "0.45"),
                        ("velocity", "TEN.Vec3(0, -45, 0)"),
                        ("velocitySpread", "TEN.Vec3(15, 20, 15)"),
                        ("rotateVelocity", "true"),
                        ("startColor", "TEN.Color(150, 150, 150)"),
                        ("endColor", "TEN.Color(45, 45, 45)"),
                        ("blendID", "7"),
                        ("startSize", "24"),
                        ("endSize", "72"),
                        ("lifeMin", "2.5"),
                        ("lifeMax", "4"),
                        ("wind", "true"),
                        ("rotVel", "25"))),

                CreateEmitterPreset(
                    emitter,
                    "Spark shower",
                    "Presets - Fire and smoke",
                    "Short-lived additive spark burst with strong random velocity spread.",
                    Defaults(
                        ("distribution", "1"),
                        ("burstCount", "5"),
                        ("intervalMin", "0.15"),
                        ("intervalMax", "0.4"),
                        ("velocity", "TEN.Vec3(0, -80, 0)"),
                        ("velocitySpread", "TEN.Vec3(180, 100, 180)"),
                        ("rotateVelocity", "true"),
                        ("startColor", "TEN.Color(255, 240, 120)"),
                        ("endColor", "TEN.Color(220, 55, 8)"),
                        ("blendID", "2"),
                        ("startSize", "8"),
                        ("endSize", "1"),
                        ("lifeMin", "0.45"),
                        ("lifeMax", "1.1"),
                        ("rotVel", "240"))),

                CreateEmitterPreset(
                    emitter,
                    "Water droplets",
                    "Presets - Water",
                    "Small droplets emitted from random positions on the top face of the box.",
                    Defaults(
                        ("distribution", "2"),
                        ("burstCount", "1"),
                        ("intervalMin", "0.05"),
                        ("intervalMax", "0.25"),
                        ("velocity", "TEN.Vec3(0, 220, 0)"),
                        ("velocitySpread", "TEN.Vec3(8, 25, 8)"),
                        ("rotateVelocity", "true"),
                        ("startColor", "TEN.Color(180, 225, 255)"),
                        ("endColor", "TEN.Color(80, 150, 220)"),
                        ("blendID", "7"),
                        ("startSize", "5"),
                        ("endSize", "2"),
                        ("lifeMin", "1"),
                        ("lifeMax", "2"))),

                CreateEmitterPreset(
                    emitter,
                    "Local rain",
                    "Presets - Water",
                    "Dense fast droplets emitted across the top face of the Effect Box.",
                    Defaults(
                        ("distribution", "2"),
                        ("burstCount", "8"),
                        ("intervalMin", "0.03"),
                        ("intervalMax", "0.08"),
                        ("velocity", "TEN.Vec3(0, 500, 0)"),
                        ("velocitySpread", "TEN.Vec3(30, 80, 30)"),
                        ("rotateVelocity", "true"),
                        ("startColor", "TEN.Color(190, 225, 255)"),
                        ("endColor", "TEN.Color(100, 155, 210)"),
                        ("blendID", "7"),
                        ("startSize", "3"),
                        ("endSize", "2"),
                        ("lifeMin", "1"),
                        ("lifeMax", "2"))),

                CreateEmitterPreset(
                    emitter,
                    "Underwater bubbles",
                    "Presets - Water",
                    "Growing translucent bubbles emitted from the bottom face of the box.",
                    Defaults(
                        ("distribution", "3"),
                        ("burstCount", "2"),
                        ("intervalMin", "0.15"),
                        ("intervalMax", "0.4"),
                        ("velocity", "TEN.Vec3(0, -70, 0)"),
                        ("velocitySpread", "TEN.Vec3(20, 30, 20)"),
                        ("rotateVelocity", "true"),
                        ("startColor", "TEN.Color(205, 240, 255)"),
                        ("endColor", "TEN.Color(120, 190, 225)"),
                        ("blendID", "7"),
                        ("startSize", "5"),
                        ("endSize", "14"),
                        ("lifeMin", "1.5"),
                        ("lifeMax", "3"))),

                CreateEmitterPreset(
                    emitter,
                    "Snow or ash",
                    "Presets - Weather",
                    "Soft slow particles distributed across the top face and affected by room wind.",
                    Defaults(
                        ("distribution", "2"),
                        ("burstCount", "4"),
                        ("intervalMin", "0.1"),
                        ("intervalMax", "0.25"),
                        ("velocity", "TEN.Vec3(0, 35, 0)"),
                        ("velocitySpread", "TEN.Vec3(25, 15, 25)"),
                        ("rotateVelocity", "true"),
                        ("startColor", "TEN.Color(255, 255, 255)"),
                        ("endColor", "TEN.Color(190, 215, 240)"),
                        ("blendID", "7"),
                        ("startSize", "8"),
                        ("endSize", "6"),
                        ("lifeMin", "3"),
                        ("lifeMax", "6"),
                        ("wind", "true"),
                        ("rotVel", "20"))),

                CreateEmitterPreset(
                    emitter,
                    "Magic ambient particles",
                    "Presets - Magic",
                    "Additive colored particles distributed throughout the box with gentle random movement.",
                    Defaults(
                        ("distribution", "1"),
                        ("burstCount", "3"),
                        ("intervalMin", "0.08"),
                        ("intervalMax", "0.2"),
                        ("velocity", "TEN.Vec3(0, -10, 0)"),
                        ("velocitySpread", "TEN.Vec3(50, 50, 50)"),
                        ("rotateVelocity", "true"),
                        ("startColor", "TEN.Color(210, 110, 255)"),
                        ("endColor", "TEN.Color(70, 25, 180)"),
                        ("blendID", "2"),
                        ("startSize", "14"),
                        ("endSize", "4"),
                        ("lifeMin", "1.2"),
                        ("lifeMax", "2.4"),
                        ("rotVel", "90"))),

                CreateEmitterPreset(
                    emitter,
                    "Firework spark burst",
                    "Presets - Fireworks",
                    "A repeated radial spark burst. This provides the explosion portion of a firework; a rising rocket phase can be assembled later as a separate emitter.",
                    Defaults(
                        ("distribution", "0"),
                        ("burstCount", "32"),
                        ("intervalMin", "2"),
                        ("intervalMax", "5"),
                        ("velocity", "TEN.Vec3(0, -220, 0)"),
                        ("velocitySpread", "TEN.Vec3(380, 280, 380)"),
                        ("rotateVelocity", "true"),
                        ("startColor", "TEN.Color(255, 220, 90)"),
                        ("endColor", "TEN.Color(255, 40, 120)"),
                        ("blendID", "2"),
                        ("startSize", "9"),
                        ("endSize", "1"),
                        ("lifeMin", "1"),
                        ("lifeMax", "2.5"),
                        ("rotVel", "360")))
            };
        }

        private static NodeFunction CreateEmitterPreset(NodeFunction source, string name, string section,
            string description, IReadOnlyDictionary<string, string> defaults)
        {
            var preset = new NodeFunction
            {
                Name = name,
                Description = description + " All values remain directly editable after insertion.",
                Section = section,
                Conditional = false,
                Signature = source.Signature
            };

            foreach (var sourceArgument in source.Arguments)
            {
                var argument = new ArgumentLayout
                {
                    Name = sourceArgument.Name,
                    Type = sourceArgument.Type,
                    DefaultValue = defaults.TryGetValue(sourceArgument.Name, out var value)
                        ? value
                        : sourceArgument.DefaultValue,
                    Description = sourceArgument.Description,
                    NewLine = sourceArgument.NewLine,
                    Width = sourceArgument.Width
                };

                argument.CustomEnumeration.AddRange(sourceArgument.CustomEnumeration);
                preset.Arguments.Add(argument);
            }

            return preset;
        }

        private static IReadOnlyDictionary<string, string> Defaults(params (string Name, string Value)[] values)
        {
            return values.ToDictionary(value => value.Name, value => value.Value, StringComparer.OrdinalIgnoreCase);
        }
    }
}
