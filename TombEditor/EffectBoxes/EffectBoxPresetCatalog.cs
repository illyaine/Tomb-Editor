using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TombLib.LevelData.VisualScripting;
using TombLib.Utils;

namespace TombEditor
{
    internal sealed class EffectBoxPreset
    {
        private readonly IReadOnlyDictionary<string, string> _argumentValues;

        public string Name { get; }
        public string Category { get; }
        public string Description { get; }
        public NodeFunction Function { get; }
        public Vector3 NodeColor { get; }

        public EffectBoxPreset(string name, string category, string description,
            NodeFunction function, Vector3 nodeColor, IReadOnlyDictionary<string, string> argumentValues)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Category = category ?? "Particles";
            Description = description ?? string.Empty;
            Function = function ?? throw new ArgumentNullException(nameof(function));
            NodeColor = nodeColor;
            _argumentValues = argumentValues ?? new Dictionary<string, string>();
        }

        public void Apply(TriggerNode node, string volumeLuaName)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            node.Name = Name;
            node.Color = NodeColor;

            for (int i = 0; i < node.Arguments.Count; i++)
            {
                var argument = node.Arguments[i];

                if (!string.IsNullOrWhiteSpace(volumeLuaName) &&
                    string.Equals(argument.Name, "volume", StringComparison.OrdinalIgnoreCase))
                {
                    argument.Value = TextExtensions.Quote(volumeLuaName);
                }
                else
                {
                    var configuredValue = _argumentValues
                        .FirstOrDefault(pair => string.Equals(pair.Key, argument.Name, StringComparison.OrdinalIgnoreCase));

                    if (!string.IsNullOrEmpty(configuredValue.Key))
                        argument.Value = configuredValue.Value;
                }

                node.Arguments[i] = argument;
            }
        }

        public override string ToString() => Name;
    }

    internal static class EffectBoxPresetCatalog
    {
        public static IReadOnlyList<EffectBoxPreset> Create(IEnumerable<NodeFunction> functions)
        {
            var volumeParticle = functions?.FirstOrDefault(IsVolumeParticleFunction);
            if (volumeParticle == null)
                return Array.Empty<EffectBoxPreset>();

            return new List<EffectBoxPreset>
            {
                CreateParticlePreset(
                    "Basic volume particle",
                    "General",
                    "Neutral starting point for an existing TEN particle emitted from the Effect Box volume.",
                    volumeParticle,
                    new Vector3(0.48f, 0.39f, 0.08f),
                    0, "TEN.Vec3(0, 0, 0)", 0, 0,
                    "TEN.Color(255, 255, 255)", "TEN.Color(255, 255, 255)",
                    7, 16, 8, 1.5f),

                CreateParticlePreset(
                    "Fire particle",
                    "Fire and smoke",
                    "Warm additive particle preset intended as a starting point for flames or embers.",
                    volumeParticle,
                    new Vector3(0.60f, 0.20f, 0.04f),
                    0, "TEN.Vec3(0, -12, 0)", 0, 8,
                    "TEN.Color(255, 190, 48)", "TEN.Color(180, 24, 4)",
                    2, 22, 4, 1.0f),

                CreateParticlePreset(
                    "Smoke particle",
                    "Fire and smoke",
                    "Slow alpha-blended particle preset with increasing size for smoke-like effects.",
                    volumeParticle,
                    new Vector3(0.28f, 0.28f, 0.28f),
                    0, "TEN.Vec3(0, -3, 0)", 0, 1,
                    "TEN.Color(150, 150, 150)", "TEN.Color(45, 45, 45)",
                    7, 24, 64, 3.0f),

                CreateParticlePreset(
                    "Spark particle",
                    "Fire and smoke",
                    "Small bright additive particle preset for sparks and short-lived glowing debris.",
                    volumeParticle,
                    new Vector3(0.64f, 0.48f, 0.05f),
                    0, "TEN.Vec3(0, -16, 0)", 18, 12,
                    "TEN.Color(255, 240, 120)", "TEN.Color(220, 55, 8)",
                    2, 8, 2, 0.8f),

                CreateParticlePreset(
                    "Water droplet",
                    "Water",
                    "Small downward-moving alpha-blended particle preset for drips or light rain.",
                    volumeParticle,
                    new Vector3(0.08f, 0.34f, 0.56f),
                    0, "TEN.Vec3(0, 24, 0)", 24, 0,
                    "TEN.Color(180, 225, 255)", "TEN.Color(80, 150, 220)",
                    7, 5, 2, 1.2f),

                CreateParticlePreset(
                    "Bubble particle",
                    "Water",
                    "Growing translucent particle preset for underwater bubbles.",
                    volumeParticle,
                    new Vector3(0.10f, 0.42f, 0.52f),
                    0, "TEN.Vec3(0, -6, 0)", 0, 1,
                    "TEN.Color(205, 240, 255)", "TEN.Color(120, 190, 225)",
                    7, 6, 14, 2.0f),

                CreateParticlePreset(
                    "Snow particle",
                    "Weather",
                    "Soft slow particle preset intended as a starting point for snow or ash.",
                    volumeParticle,
                    new Vector3(0.52f, 0.56f, 0.62f),
                    0, "TEN.Vec3(0, 4, 0)", 3, 1,
                    "TEN.Color(255, 255, 255)", "TEN.Color(190, 215, 240)",
                    7, 8, 6, 3.0f),

                CreateParticlePreset(
                    "Magic glow particle",
                    "Magic",
                    "Additive colored particle preset for magical glows and ambient energy.",
                    volumeParticle,
                    new Vector3(0.38f, 0.12f, 0.56f),
                    0, "TEN.Vec3(0, -2, 0)", 0, 4,
                    "TEN.Color(210, 110, 255)", "TEN.Color(70, 25, 180)",
                    2, 14, 4, 1.8f)
            };
        }

        private static bool IsVolumeParticleFunction(NodeFunction function)
        {
            if (function == null)
                return false;

            return string.Equals(function.Signature, "ParticleEmitterVolume", StringComparison.OrdinalIgnoreCase) ||
                   (function.Name ?? string.Empty).IndexOf("Particle generator (volume)", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static EffectBoxPreset CreateParticlePreset(string name, string category, string description,
            NodeFunction function, Vector3 color, int spriteId, string velocity, float gravity, float rotation,
            string startColor, string endColor, int blendId, float startSize, float endSize, float life)
        {
            return new EffectBoxPreset(name, category, description, function, color,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["spriteID"] = spriteId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["velocity"] = velocity,
                    ["gravity"] = gravity.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["rotation"] = rotation.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["startColor"] = startColor,
                    ["endColor"] = endColor,
                    ["blendID"] = blendId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["startSize"] = startSize.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["endSize"] = endSize.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["life"] = life.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["poison"] = "false",
                    ["damage"] = "false"
                });
        }
    }
}
