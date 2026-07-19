using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TombLib.LevelData;

namespace TombEditor.Forms
{
    public partial class FormEventSetEditor
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_instance != null && _instance.IsEffectBox())
            {
                BeginInvoke(new Action(OpenEffectBoxEditor));
                return;
            }

            if (GlobalMode)
                return;

            dgvEvents.RowsAdded += EffectBoxRowsAdded;
            dgvEvents.SelectionChanged += EffectBoxSelectionChanged;
            dgvEvents.CellDoubleClick += EffectBoxCellDoubleClick;
            QueueEffectBoxRowRefresh();
        }

        private void OpenEffectBoxEditor()
        {
            if (IsDisposed || _instance == null || !_instance.IsEffectBox())
                return;

            var owner = Owner as IWin32Window;
            Hide();

            using (var form = new FormEffectBoxEditor(_instance))
                DialogResult = form.ShowDialog(owner);

            Close();
        }

        private void EffectBoxRowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            QueueEffectBoxRowRefresh();
        }

        private void EffectBoxSelectionChanged(object sender, EventArgs e)
        {
            if (IsHandleCreated && !IsDisposed)
                BeginInvoke(new Action(UpdateEffectBoxSelectionState));
        }

        private void EffectBoxCellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvEvents.Rows.Count)
                return;

            var eventSet = dgvEvents.Rows[e.RowIndex].Tag as EventSet;
            var effectBox = FindEffectBox(eventSet);
            if (effectBox == null)
                return;

            using (var form = new FormEffectBoxEditor(effectBox))
                form.ShowDialog(this);

            RefreshEffectBoxRows();
            UpdateEffectBoxSelectionState();
        }

        private void QueueEffectBoxRowRefresh()
        {
            if (!IsHandleCreated || IsDisposed)
                return;

            BeginInvoke(new Action(() =>
            {
                RefreshEffectBoxRows();
                UpdateEffectBoxSelectionState();
            }));
        }

        private void RefreshEffectBoxRows()
        {
            if (IsDisposed)
                return;

            foreach (DataGridViewRow row in dgvEvents.Rows)
            {
                var eventSet = row.Tag as EventSet;
                var effectBox = FindEffectBox(eventSet);
                if (effectBox == null)
                    continue;

                string name = string.IsNullOrWhiteSpace(effectBox.LuaName)
                    ? "Effect box graph"
                    : "Effect box graph — " + effectBox.LuaName;

                row.Cells[0].Value = name;
                row.Cells[0].ToolTipText = "Managed by the Effect Box Editor. Double-click to open it.";
                row.DefaultCellStyle.ForeColor = Color.Goldenrod;
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
            bool managed = FindEffectBox(selectedEventSet) != null;

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

            base.OnHandleDestroyed(e);
        }
    }
}
