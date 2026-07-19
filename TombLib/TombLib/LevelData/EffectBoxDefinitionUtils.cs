using System;
using System.Collections.Generic;
using System.Linq;

namespace TombLib.LevelData
{
    /// <summary>
    /// Manages project-wide Effect Box definitions.
    ///
    /// A definition is stored as a regular VolumeEventSet and can be referenced by any
    /// number of Effect Box instances in any room. Box instances only own their transform,
    /// active state and Lua name; the authored effect graph remains global to the project.
    /// </summary>
    public static class EffectBoxDefinitionUtils
    {
        public static IReadOnlyList<VolumeEventSet> GetDefinitions(LevelSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            return settings.VolumeEventSets
                .OfType<VolumeEventSet>()
                .Where(EffectBoxUtils.IsEffectBoxEventSet)
                .ToList();
        }

        public static VolumeEventSet CreateDefinition(LevelSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            // EffectBoxUtils.Create already establishes the complete private event-set
            // contract. The temporary instance is deliberately discarded: the definition
            // itself is project-wide and can subsequently be assigned to many boxes.
            return (VolumeEventSet)EffectBoxUtils.Create(settings).EventSet;
        }

        public static VolumeEventSet GetOrCreateDefaultDefinition(LevelSettings settings)
        {
            return GetDefinitions(settings).FirstOrDefault() ?? CreateDefinition(settings);
        }

        public static BoxVolumeInstance CreateInstance(LevelSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            return new BoxVolumeInstance
            {
                Enabled = true,
                DetectInAdjacentRooms = false,
                EventSet = GetOrCreateDefaultDefinition(settings)
            };
        }

        public static VolumeEventSet CloneDefinition(LevelSettings settings, VolumeEventSet source)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (source == null || !EffectBoxUtils.IsEffectBoxEventSet(source))
                throw new ArgumentException("The supplied event set is not an Effect Box definition.", nameof(source));

            var clone = (VolumeEventSet)source.Clone();
            clone.Name = EffectBoxUtils.CreateEventSetName();
            settings.VolumeEventSets.Add(clone);
            return clone;
        }
    }
}
