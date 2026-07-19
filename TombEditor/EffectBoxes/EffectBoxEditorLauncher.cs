using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TombEditor.Forms;
using TombLib.LevelData;
using TombLib.LevelData.VisualScripting;

namespace TombEditor
{
    /// <summary>
    /// Opens the Effect Box component editor against a temporary working definition.
    /// Definitions remain shared, while every supported emitter is stored as an independent
    /// parallel component rather than as an executable action sequence.
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
            NormalizeParallelComponents(workingDefinition.Events[EventType.OnVolumeInside]);

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
                    NormalizeParallelComponents(editedDefinition.Events[EventType.OnVolumeInside]);
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
                settings.VolumeEventSets.RemoveAll(eventSet =>
                    string.Equals(eventSet.Name, workingDefinition.Name, StringComparison.Ordinal));

                editor.ObjectChange(instance, ObjectChangeType.Change);
                editor.EventSetsChange();
            }

            return result;
        }

        private static void NormalizeParallelComponents(Event graphEvent)
        {
            if (graphEvent == null)
                return;

            var components = new List<TriggerNode>();
            foreach (var root in graphEvent.Nodes.ToList())
            {
                for (var node = root; node != null;)
                {
                    var next = node.Next;
                    node.Previous = null;
                    node.Next = null;

                    if (EffectBoxRuntimeBuilder.IsRuntimeSupported(node))
                        components.Add(node);

                    node = next;
                }
            }

            graphEvent.Nodes = components;
        }

        private static void UpdatePresentation(Control root)
        {
            foreach (Control control in root.Controls)
            {
                if (control is TableLayoutPanel header &&
                    header.Controls.OfType<Label>().Any(label => label.Text == "Lua name:") &&
                    header.Controls.OfType<Label>().Any(label => label.Text == "Box state:"))
                {
                    HideInstanceColumns(header);
                }

                if (control is Label label)
                {
                    if (label.Text.StartsWith("Persistent editor graph", StringComparison.Ordinal))
                        label.Text = "Project-wide definition — components run in parallel";
                    else if (label.Text == "Effect entries")
                        label.Text = "Effect components";
                    else if (label.Text == "Effect composition")
                        label.Text = "Component parameters";
                    else if (label.Text == "Available TEN effects")
                        label.Text = "Available particle emitters";
                }

                if (control is Button button)
                {
                    if (button.Text == "Create")
                        button.Text = "Add component";
                    else if (button.Text == "Delete")
                        button.Text = "Delete component";
                    else if (button.Text == "Link nodes" || button.Text == "Delete node")
                        button.Visible = false;
                    else if (button.Text == "Clear graph")
                        button.Text = "Clear components";
                    else if (button.Text == "Add selected effect")
                        button.Text = "Add selected emitter";
                }

                if (control is TextBox textBox && textBox.PlaceholderText == "Entry name")
                    textBox.PlaceholderText = "Component name";

                if (control is TreeView tree)
                    RemoveUnsupportedFunctions(tree.Nodes);

                if (control.HasChildren)
                    UpdatePresentation(control);
            }
        }

        private static void RemoveUnsupportedFunctions(TreeNodeCollection nodes)
        {
            for (int index = nodes.Count - 1; index >= 0; index--)
            {
                var node = nodes[index];
                if (node.Tag is NodeFunction function &&
                    !string.Equals(function.Signature, EffectBoxRuntimeBuilder.ParticleEmitterFunction,
                        StringComparison.Ordinal))
                {
                    nodes.RemoveAt(index);
                    continue;
                }

                if (node.Nodes.Count > 0)
                {
                    RemoveUnsupportedFunctions(node.Nodes);
                    if (node.Tag == null && node.Nodes.Count == 0)
                        nodes.RemoveAt(index);
                }
            }
        }

        private static void HideInstanceColumns(TableLayoutPanel header)
        {
            foreach (Control control in header.Controls)
            {
                int column = header.GetColumn(control);
                if (column >= 0 && column <= 3)
                    control.Visible = false;
            }

            for (int column = 0; column <= 3 && column < header.ColumnStyles.Count; column++)
            {
                header.ColumnStyles[column].SizeType = SizeType.Absolute;
                header.ColumnStyles[column].Width = 0;
            }
        }
    }
}
