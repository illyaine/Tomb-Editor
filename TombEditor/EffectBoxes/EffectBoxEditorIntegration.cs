using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TombEditor.Forms;
using TombLib.LevelData;
using TombLib.LevelData.VisualScripting;
using TombLib.Utils;

namespace TombEditor
{
    /// <summary>
    /// Central integration point for editor-only effect boxes.
    /// Effect boxes deliberately reuse TEN box volumes and volume event-set serialization.
    /// The reserved event-set prefix identifies them without changing the PRJ2 format.
    /// </summary>
    public static class EffectBoxEditorIntegration
    {
        public const string EventSetPrefix = "__TEN_EFFECT_BOX__";

        private static readonly string[] EffectKeywords =
        {
            "effect", "particle", "spark", "fire", "flame", "smoke", "light", "lightning",
            "shockwave", "explosion", "blood", "bubble", "splash", "ripple", "weather",
            "rain", "snow", "fog", "mist", "flash", "earthquake", "sound"
        };

        public static bool IsEffectBox(ObjectInstance instance) =>
            instance is BoxVolumeInstance box && IsEffectBox(box);

        public static bool IsEffectBox(VolumeInstance instance) =>
            instance?.EventSet is VolumeEventSet set &&
            !string.IsNullOrEmpty(set.Name) &&
            set.Name.StartsWith(EventSetPrefix, StringComparison.Ordinal);

        public static BoxVolumeInstance CreateEffectBox(Level level)
        {
            if (level == null)
                throw new ArgumentNullException(nameof(level));

            var eventSet = CreateEventSet(level);
            var box = new BoxVolumeInstance
            {
                // Runtime execution is intentionally disabled in the first implementation stage.
                // The graph is already persisted and can be activated by the later TEN runtime bridge.
                Enabled = false,
                DetectInAdjacentRooms = false,
                EventSet = eventSet
            };

            return box;
        }

        public static VolumeEventSet EnsureEffectSet(Level level, BoxVolumeInstance box)
        {
            if (level == null)
                throw new ArgumentNullException(nameof(level));
            if (box == null)
                throw new ArgumentNullException(nameof(box));

            if (box.EventSet is VolumeEventSet existing && IsEffectBox(box))
            {
                if (!level.Settings.VolumeEventSets.Contains(existing))
                    level.Settings.VolumeEventSets.Add(existing);
                return existing;
            }

            var eventSet = CreateEventSet(level);
            box.EventSet = eventSet;
            box.Enabled = false;
            box.DetectInAdjacentRooms = false;
            return eventSet;
        }

        public static void BeginPlacement(Editor editor, IWin32Window owner)
        {
            if (editor?.Level == null)
                return;

            if (!editor.Level.IsTombEngine)
            {
                editor.SendMessage("Effect boxes are available only for Tomb Engine levels.", PopupType.Warning);
                return;
            }

            editor.Action = new EditorActionPlace(false, (location, room) => CreateEffectBox(editor.Level));
        }

        public static void Place(Editor editor, IWin32Window owner, Room room, VectorInt2 sector)
        {
            if (editor?.Level == null || room == null)
                return;

            if (!editor.Level.IsTombEngine)
            {
                editor.SendMessage("Effect boxes are available only for Tomb Engine levels.", PopupType.Warning);
                return;
            }

            var box = CreateEffectBox(editor.Level);
            EditorActions.PlaceObject(room, sector, box);
            OpenEditor(owner, box);
        }

        public static bool OpenEditor(IWin32Window owner, ObjectInstance instance)
        {
            if (!(instance is BoxVolumeInstance box) || !IsEffectBox(box))
                return false;

            EnsureEffectSet(Editor.Instance.Level, box);
            using (var form = new FormEffectBoxEditor(box))
                form.ShowDialog(owner);

            return true;
        }

        public static IReadOnlyList<NodeFunction> GetAvailableEffects()
        {
            return ScriptingUtils.NodeFunctions
                .Where(function => !function.Conditional && IsEffectFunction(function))
                .OrderBy(function => function.Section)
                .ThenBy(function => function.Name)
                .ToList();
        }

        private static bool IsEffectFunction(NodeFunction function)
        {
            string text = string.Join(" ", function.Section, function.Name, function.Signature, function.Description)
                .ToLowerInvariant();
            return EffectKeywords.Any(keyword => text.Contains(keyword));
        }

        private static VolumeEventSet CreateEventSet(Level level)
        {
            var set = new VolumeEventSet
            {
                Name = GetUniqueEventSetName(level),
                Activators = VolumeActivators.None,
                LastUsedEvent = EventType.OnVolumeInside
            };

            var effectEvent = set.Events[EventType.OnVolumeInside];
            effectEvent.Mode = EventSetMode.NodeEditor;
            effectEvent.Enabled = true;
            effectEvent.CallCounter = 0;

            level.Settings.VolumeEventSets.Add(set);
            return set;
        }

        private static string GetUniqueEventSetName(Level level)
        {
            int index = 1;
            string result;
            do
            {
                result = EventSetPrefix + index.ToString("D4");
                index++;
            }
            while (level.Settings.VolumeEventSets.Any(set => set.Name == result));

            return result;
        }
    }
}
