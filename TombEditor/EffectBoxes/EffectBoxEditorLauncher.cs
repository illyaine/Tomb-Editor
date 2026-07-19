using System;
using System.Linq;
using System.Windows.Forms;
using TombEditor.Forms;
using TombLib.LevelData;

namespace TombEditor
{
    /// <summary>
    /// Opens the existing Effect Box graph editor against a temporary working definition.
    /// This preserves the editor's OK/Cancel behavior while keeping the real definition
    /// shared by every Effect Box instance which references it.
    /// </summary>
    internal static class EffectBoxEditorLauncher
    {
        public static DialogResult Show(IWin32Window owner, VolumeInstance instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (!instance.IsEffectBox() || instance.EventSet is not VolumeEventSet originalDefinition)
                throw new ArgumentException("The supplied volume is not an Effect Box.", nameof(instance));

            var editor = Editor.Instance;
            var settings = editor.Level.Settings;
            var workingDefinition = (VolumeEventSet)originalDefinition.Clone();
            workingDefinition.Name = EffectBoxUtils.CreateEventSetName();

            settings.VolumeEventSets.Add(workingDefinition);
            instance.EventSet = workingDefinition;

            DialogResult result = DialogResult.Cancel;
            try
            {
                using (var form = new FormEffectBoxEditor(instance))
                {
                    form.Text = "Effect definition editor";
                    UpdatePresentation(form);
                    result = form.ShowDialog(owner);
                }

                if (result == DialogResult.OK && instance.EventSet is VolumeEventSet editedDefinition)
                {
                    originalDefinition.Activators = VolumeActivators.None;
                    originalDefinition.LastUsedEvent = EventType.OnVolumeInside;
                    originalDefinition.Events = editedDefinition.Events.ToDictionary(
                        pair => pair.Key,
                        pair => pair.Value.Clone());
                }
            }
            finally
            {
                instance.EventSet = originalDefinition;

                // Cancel restores a cloned working set into the settings list, while OK leaves
                // the original working object there. Remove either form by its unique name.
                settings.VolumeEventSets.RemoveAll(eventSet =>
                    string.Equals(eventSet.Name, workingDefinition.Name, StringComparison.Ordinal));

                editor.ObjectChange(instance, ObjectChangeType.Change);
                editor.EventSetsChange();
            }

            return result;
        }

        private static void UpdatePresentation(Control root)
        {
            foreach (Control control in root.Controls)
            {
                if (control is Label label &&
                    label.Text.StartsWith("Persistent editor graph", StringComparison.Ordinal))
                {
                    label.Text = "Project-wide definition — every placed box executes independently";
                }

                if (control.HasChildren)
                    UpdatePresentation(control);
            }
        }
    }
}
