using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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
        private sealed class DefaultDefinitionState
        {
            public VolumeEventSet Definition;
        }

        private static readonly ConditionalWeakTable<LevelSettings, DefaultDefinitionState> _defaultDefinitions =
            new ConditionalWeakTable<LevelSettings, DefaultDefinitionState>();

        public static IReadOnlyList<VolumeEventSet> GetDefinitions(LevelSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            return settings.VolumeEventSets
                .OfType<VolumeEventSet>()
                .Where(definition => EffectBoxUtils.IsEffectBoxEventSet(definition))
                .ToList();
        }

        public static VolumeEventSet CreateDefinition(LevelSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var definition = new VolumeEventSet
            {
                Name = EffectBoxUtils.CreateEventSetName(),
                Activators = VolumeActivators.None,
                LastUsedEvent = EventType.OnVolumeInside
            };

            foreach (var entry in definition.Events)
            {
                entry.Value.Enabled = false;
                entry.Value.Mode = EventSetMode.NodeEditor;
                entry.Value.CallCounter = 0;
            }

            settings.VolumeEventSets.Add(definition);
            SetDefaultDefinition(settings, definition);
            return definition;
        }

        public static void SetDefaultDefinition(LevelSettings settings, VolumeEventSet definition)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (definition == null || !EffectBoxUtils.IsEffectBoxEventSet(definition) ||
                !settings.VolumeEventSets.Contains(definition))
                return;

            GetState(settings).Definition = definition;
        }

        public static VolumeEventSet GetOrCreateDefaultDefinition(LevelSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var state = GetState(settings);
            if (state.Definition != null && settings.VolumeEventSets.Contains(state.Definition) &&
                EffectBoxUtils.IsEffectBoxEventSet(state.Definition))
            {
                return state.Definition;
            }

            state.Definition = GetDefinitions(settings).FirstOrDefault();
            if (state.Definition == null)
                state.Definition = CreateDefinition(settings);

            return state.Definition;
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
            SetDefaultDefinition(settings, clone);
            return clone;
        }

        private static DefaultDefinitionState GetState(LevelSettings settings)
        {
            return _defaultDefinitions.GetValue(settings, _ => new DefaultDefinitionState());
        }
    }
}
