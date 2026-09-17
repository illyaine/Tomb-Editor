using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TombLib.LevelData;

namespace TombEditor.Forms
{
    public partial class FormEventSetEditor
    {
        private bool _effectBoxAssignmentMode;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (GlobalMode)
                return;

            _effectBoxAssignmentMode = _instance != null && _instance.IsEffectBox();
            if (_effectBoxAssignmentMode)
                InitializeEffectBoxAssignmentMode();

            dgvEvents.RowsAdded += EffectBoxRowsAdded;
            dgvEvents.SelectionChanged += EffectBoxSelectionChanged;
            dgvEvents.CellDoubleClick += EffectBoxCellDoubleClick;
            QueueEffectBoxRowRefresh();
        }

        private void InitializeEffectBoxAssignmentMode()
        {
            Text = "Assign Effect Box definition";
            panelList.SectionHeader = "Project effect definitions";
            panelEditor.SectionHeader = "Definition preview";
            panelActivators.Visible = false;

            // The generic event-set editor can reorder its complete backing list through the
            // column header and drag-and-drop. This view deliberately contains only Effect Box
            // rows, so those handlers must not be allowed to remove hidden regular event sets.
            dgvEvents.AllowUserToDragDropRows = false;
            dgvEvents.ColumnHeaderMouseClick -= dgvEvents_ColumnHeaderMouseClick;
            dgvEvents.DragDrop -= dgvEvents_DragDrop;

            if (dgvEvents.Columns.Count > 0)
                dgvEvents.Columns[0].HeaderText = "Effect definitions";

            butNewEventSet.Click -= butNewEventSet_Click;
            butCloneEventSet.Click -= butCloneEventSet_Click;
            butDeleteEventSet.Click -= butDeleteEventSet_Click;
            butUnassignEventSet.Click -= butUnassignEventSet_Click;

            butNewEventSet.Click += EffectBoxNewDefinition_Click;
            butCloneEventSet.Click += EffectBoxCloneDefinition_Click;
            butDeleteEventSet.Click += EffectBoxDeleteDefinition_Click;
            butUnassignEventSet.Click += EffectBoxEditDefinition_Click;

            butNewEventSet.DialogResult = DialogResult.None;
            butCloneEventSet.DialogResult = DialogResult.None;
            butDeleteEventSet.DialogResult = DialogResult.None;
            butUnassignEventSet.DialogResult = DialogResult.None;

            butUnassignEventSet.Image = Properties.Resources.general_edit_16;
            toolTip.SetToolTip(butNewEventSet, "Create a new project-wide Effect Box definition");
            toolTip.SetToolTip(butCloneEventSet, "Copy the selected Effect Box definition");
            toolTip.SetToolTip(butDeleteEventSet, "Delete the selected definition and reassign its boxes");
            toolTip.SetToolTip(butUnassignEventSet, "Edit the selected definition");

            triggerManager.Enabled = false;
            cbEvents.Enabled = false;
            tbName.Enabled = false;
        }

        private void EffectBoxRowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            QueueEffectBoxRowRefresh();
        }

        private void EffectBoxSelectionChanged(object sender, EventArgs e)
        {
            if (_lockSelectionChange || !IsHandleCreated || IsDisposed)
                return;

            BeginInvoke(new Action(() =>
            {
                if (IsDisposed)
                    return;

                if (_effectBoxAssignmentMode &&
                    SelectedSet is VolumeEventSet selectedDefinition &&
                    EffectBoxUtils.IsEffectBoxEventSet(selectedDefinition))
                {
                    _instance.EventSet = selectedDefinition;
                    _editor.ObjectChange(_instance, ObjectChangeType.Change);
                    RefreshEffectBoxRows();
                }

                UpdateEffectBoxSelectionState();
            }));
        }

        private void EffectBoxCellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvEvents.Rows.Count)
                return;

            var eventSet = dgvEvents.Rows[e.RowIndex].Tag as EventSet;
            if (!EffectBoxUtils.IsEffectBoxEventSet(eventSet))
                return;

            if (_effectBoxAssignmentMode)
            {
                SelectedSet = eventSet;
                EditSelectedEffectDefinition();
                return;
            }

            var effectBox = FindEffectBox(eventSet);
            if (effectBox == null)
                return;

            TombEditor.EffectBoxEditorLauncher.Show(this, effectBox);
            RefreshEffectBoxRows();
            UpdateEffectBoxSelectionState();
        }

        private void EffectBoxNewDefinition_Click(object sender, EventArgs e)
        {
            if (!_effectBoxAssignmentMode)
                return;

            var definition = EffectBoxDefinitionUtils.CreateDefinition(_editor.Level.Settings);
            RepopulateEffectBoxDefinitions(definition);
            EditSelectedEffectDefinition();
        }

        private void EffectBoxCloneDefinition_Click(object sender, EventArgs e)
        {
            if (!_effectBoxAssignmentMode || SelectedSet is not VolumeEventSet source ||
                !EffectBoxUtils.IsEffectBoxEventSet(source))
                return;

            var clone = EffectBoxDefinitionUtils.CloneDefinition(_editor.Level.Settings, source);
            RepopulateEffectBoxDefinitions(clone);
            _editor.EventSetsChange();
        }

        private void EffectBoxDeleteDefinition_Click(object sender, EventArgs e)
        {
            if (!_effectBoxAssignmentMode || SelectedSet is not VolumeEventSet selected ||
                !EffectBoxUtils.IsEffectBoxEventSet(selected))
                return;

            var definitions = EffectBoxDefinitionUtils.GetDefinitions(_editor.Level.Settings);
            if (definitions.Count <= 1)
            {
                MessageBox.Show(this,
                    "The last Effect Box definition cannot be deleted. Create another definition first.",
                    "Delete Effect Box definition", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var affectedBoxes = _editor.Level.GetAllObjects()
                .OfType<VolumeInstance>()
                .Where(volume => ReferenceEquals(volume.EventSet, selected))
                .ToList();

            if (MessageBox.Show(this,
                    "Delete this project-wide effect definition? " + affectedBoxes.Count +
                    (affectedBoxes.Count == 1 ? " placed box will" : " placed boxes will") +
                    " be reassigned to another definition.",
                    "Delete Effect Box definition", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var replacement = definitions.First(definition => !ReferenceEquals(definition, selected));
            foreach (var volume in affectedBoxes)
            {
                volume.EventSet = replacement;
                _editor.ObjectChange(volume, ObjectChangeType.Change);
            }

            _editor.Level.Settings.VolumeEventSets.Remove(selected);
            RepopulateEffectBoxDefinitions(replacement);
            _editor.EventSetsChange();
        }

        private void EffectBoxEditDefinition_Click(object sender, EventArgs e)
        {
            if (_effectBoxAssignmentMode)
                EditSelectedEffectDefinition();
        }

        private void EditSelectedEffectDefinition()
        {
            if (!_effectBoxAssignmentMode || _instance == null ||
                SelectedSet is not VolumeEventSet definition ||
                !EffectBoxUtils.IsEffectBoxEventSet(definition))
                return;

            _instance.EventSet = definition;
            _editor.ObjectChange(_instance, ObjectChangeType.Change);
            TombEditor.EffectBoxEditorLauncher.Show(this, _instance);

            RepopulateEffectBoxDefinitions(_instance.EventSet);
            UpdateEffectBoxSelectionState();
        }

        private void RepopulateEffectBoxDefinitions(EventSet selection)
        {
            _lockSelectionChange = true;
            try
            {
                PopulateEventSetList();
                RefreshEffectBoxRows();

                // PopulateEventSetList rebuilds every row while SelectedSet still points to
                // the same object. Reset the cache so the property setter selects the new row.
                _selectedSet = null;
                SelectedSet = selection;
            }
            finally
            {
                _lockSelectionChange = false;
            }

            RefreshEffectBoxRows();
        }

        private void QueueEffectBoxRowRefresh()
        {
            if (!IsHandleCreated || IsDisposed)
                return;

            BeginInvoke(new Action(() =>
            {
                if (IsDisposed)
                    return;

                RefreshEffectBoxRows();
                UpdateEffectBoxSelectionState();
            }));
        }

        private void RefreshEffectBoxRows()
        {
            if (IsDisposed)
                return;

            if (_effectBoxAssignmentMode)
            {
                bool wasLocked = _lockSelectionChange;
                _lockSelectionChange = true;
                try
                {
                    for (int index = dgvEvents.Rows.Count - 1; index >= 0; index--)
                    {
                        var rowSet = dgvEvents.Rows[index].Tag as EventSet;
                        if (!EffectBoxUtils.IsEffectBoxEventSet(rowSet))
                            dgvEvents.Rows.RemoveAt(index);
                    }
                }
                finally
                {
                    _lockSelectionChange = wasLocked;
                }
            }

            int definitionIndex = 0;
            foreach (DataGridViewRow row in dgvEvents.Rows)
            {
                var eventSet = row.Tag as EventSet;
                if (!EffectBoxUtils.IsEffectBoxEventSet(eventSet))
                    continue;

                definitionIndex++;
                var graph = (eventSet as VolumeEventSet)?.Events[EventType.OnVolumeInside];
                var firstRoot = graph?.Nodes.FirstOrDefault();
                var name = firstRoot != null && !string.IsNullOrWhiteSpace(firstRoot.Name)
                    ? firstRoot.Name
                    : "Empty definition";

                int usageCount = _editor.Level.GetAllObjects()
                    .OfType<VolumeInstance>()
                    .Count(volume => ReferenceEquals(volume.EventSet, eventSet));
                bool assigned = _instance != null && ReferenceEquals(_instance.EventSet, eventSet);

                row.Cells[0].Value = (assigned ? "● " : string.Empty) + definitionIndex + ". " + name +
                    " — " + usageCount + (usageCount == 1 ? " placed box" : " placed boxes");
                row.Cells[0].ToolTipText = "Project-wide Effect Box definition. Double-click to edit it.";
                row.DefaultCellStyle.ForeColor = assigned ? Color.Gold : Color.Goldenrod;
                row.DefaultCellStyle.SelectionForeColor = Color.Gold;
            }
        }

        private void UpdateEffectBoxSelectionState()
        {
            if (IsDisposed)
                return;

            var selectedEventSet = dgvEvents.SelectedRows.Count == 0
                ? null
                : dgvEvents.SelectedRows[0].Tag as EventSet;
            bool managed = EffectBoxUtils.IsEffectBoxEventSet(selectedEventSet);

            if (_effectBoxAssignmentMode)
            {
                butNewEventSet.Enabled = true;
                butCloneEventSet.Enabled = managed;
                butDeleteEventSet.Enabled = managed &&
                    EffectBoxDefinitionUtils.GetDefinitions(_editor.Level.Settings).Count > 1;
                butUnassignEventSet.Enabled = managed;

                tbName.Enabled = false;
                triggerManager.Enabled = false;
                cbEvents.Enabled = false;
                cbActivatorLara.Enabled = false;
                cbActivatorNPC.Enabled = false;
                cbActivatorOtherMoveables.Enabled = false;
                cbActivatorStatics.Enabled = false;
                cbActivatorFlyBy.Enabled = false;
                lblActivators.Enabled = false;
                return;
            }

            if (!managed)
            {
                UpdateUI();
                return;
            }

            tbName.Enabled = false;
            triggerManager.Enabled = false;
            cbEvents.Enabled = false;
            butUnassignEventSet.Enabled = false;
            butCloneEventSet.Enabled = false;
            butDeleteEventSet.Enabled = false;

            cbActivatorLara.Enabled = false;
            cbActivatorNPC.Enabled = false;
            cbActivatorOtherMoveables.Enabled = false;
            cbActivatorStatics.Enabled = false;
            cbActivatorFlyBy.Enabled = false;
            lblActivators.Enabled = false;
        }

        private VolumeInstance FindEffectBox(EventSet eventSet)
        {
            if (!EffectBoxUtils.IsEffectBoxEventSet(eventSet))
                return null;

            return _editor.Level.GetAllObjects()
                .OfType<VolumeInstance>()
                .FirstOrDefault(volume => volume.IsEffectBox() && ReferenceEquals(volume.EventSet, eventSet));
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (!GlobalMode)
            {
                dgvEvents.RowsAdded -= EffectBoxRowsAdded;
                dgvEvents.SelectionChanged -= EffectBoxSelectionChanged;
                dgvEvents.CellDoubleClick -= EffectBoxCellDoubleClick;
            }

            if (_effectBoxAssignmentMode)
            {
                butNewEventSet.Click -= EffectBoxNewDefinition_Click;
                butCloneEventSet.Click -= EffectBoxCloneDefinition_Click;
                butDeleteEventSet.Click -= EffectBoxDeleteDefinition_Click;
                butUnassignEventSet.Click -= EffectBoxEditDefinition_Click;
            }

            base.OnHandleDestroyed(e);
        }
    }
}
