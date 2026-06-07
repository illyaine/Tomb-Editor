using System;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.LevelData.ObjectParameters;

namespace TombEditor.Forms
{
    public partial class FormObjectParameters
    {
        private bool _isOcbGridMode;
        private bool _updatingOcbGrid;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SelectFirstOcbDefinitionSet();
            UpdateOcbGridMode();
        }

        private void SelectFirstOcbDefinitionSet()
        {
            if (SelectedDefinitionSet != null && SelectedDefinitionSet.OcbDefinitions.Count != 0)
                return;

            for (int i = 0; i < comboDefinitions.Items.Count; i++)
            {
                comboDefinitions.SelectedIndex = i;
                if (SelectedDefinitionSet != null && SelectedDefinitionSet.OcbDefinitions.Count != 0)
                    return;
            }
        }

        private void UpdateOcbGridMode()
        {
            ObjectParameterDefinitionSet definitionSet = SelectedDefinitionSet;
            if (definitionSet == null || definitionSet.OcbDefinitions.Count == 0)
                return;

            _isOcbGridMode = true;
            comboDefinitions.Visible = false;
            darkLabel1.Visible = false;
            comboPresets.Visible = false;
            darkLabel2.Visible = false;
            darkGroupBox1.Text = "Existing object OCB";

            ConfigureOcbGridColumns();
            ReloadOcbGrid(definitionSet);
        }

        private void ConfigureOcbGridColumns()
        {
            gridValues.Columns.Clear();
            gridValues.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Slot", Name = "Slot", FillWeight = 160, ReadOnly = true });
            gridValues.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Use", Name = "Use", FillWeight = 40, ReadOnly = false });
            gridValues.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "OCB", Name = "OcbValue", FillWeight = 55, ReadOnly = true });
            gridValues.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", Name = "Name", FillWeight = 150, ReadOnly = true });
            gridValues.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Description", Name = "Description", FillWeight = 330, ReadOnly = true });
            gridValues.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mode", Name = "Mode", FillWeight = 85, ReadOnly = true });
            gridValues.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", Name = "Status", FillWeight = 80, ReadOnly = true });

            gridValues.CurrentCellDirtyStateChanged -= gridValues_CurrentCellDirtyStateChangedOcbGrid;
            gridValues.CellValueChanged -= gridValues_CellValueChangedOcbGrid;
            gridValues.CurrentCellDirtyStateChanged += gridValues_CurrentCellDirtyStateChangedOcbGrid;
            gridValues.CellValueChanged += gridValues_CellValueChangedOcbGrid;
        }

        private void ReloadOcbGrid(ObjectParameterDefinitionSet definitionSet)
        {
            _updatingOcbGrid = true;
            gridValues.Rows.Clear();

            short currentOcb = GetCurrentObjectOcb();
            string slotText = GetSlotText(definitionSet);

            foreach (ObjectParameterOcbDefinition definition in definitionSet.OcbDefinitions)
                AddOcbRow(definition, slotText, IsDefinitionActive(definition, currentOcb));

            if (CalculateOcbFromGrid() != currentOcb)
            {
                foreach (DataGridViewRow row in gridValues.Rows)
                    row.Cells["Use"].Value = false;

                AddOcbRow(new ObjectParameterOcbDefinition
                {
                    Value = currentOcb,
                    Name = "Unknown / undocumented",
                    Description = "Current raw OCB value. No confirmed object-specific meaning is available yet.",
                    Group = "Raw",
                    Mode = ObjectParameterOcbMode.FixedValue,
                    MappingStatus = ObjectParameterMappingStatus.Unknown,
                    Warning = "Do not overwrite this value unless you know what it does."
                }, slotText, true);
            }

            ApplyOcbGridValueToHiddenState();
            _updatingOcbGrid = false;
        }

        private string GetSlotText(ObjectParameterDefinitionSet definitionSet)
        {
            string slot = _context.ObjectKey?.SlotId.HasValue == true ? "Slot " + _context.ObjectKey.SlotId.Value : "No slot";
            string name = (definitionSet.Name ?? string.Empty).Replace("TEN OCB:", string.Empty).Trim();
            return string.IsNullOrEmpty(name) ? slot : slot + " - " + name;
        }

        private short GetCurrentObjectOcb()
        {
            if (_instance is ItemInstance item)
                return item.Ocb;
            if (_instance is StaticInstance staticInstance)
                return staticInstance.Ocb;

            return 0;
        }

        private static bool IsDefinitionActive(ObjectParameterOcbDefinition definition, short currentOcb)
        {
            if (definition.Mode == ObjectParameterOcbMode.AdditiveFlags || definition.IsCombinable)
                return definition.Value > 0 && (currentOcb & definition.Value) == definition.Value;

            return definition.Value == currentOcb;
        }

        private void AddOcbRow(ObjectParameterOcbDefinition definition, string slotText, bool active)
        {
            int index = gridValues.Rows.Add(
                slotText,
                active,
                definition.Value,
                definition.Name,
                definition.Description,
                GetModeText(definition),
                definition.MappingStatus.ToString());

            gridValues.Rows[index].Tag = definition;
        }

        private static string GetModeText(ObjectParameterOcbDefinition definition)
        {
            if (definition.Mode == ObjectParameterOcbMode.AdditiveFlags || definition.IsCombinable)
                return "Checkbox flag";

            return "Single value";
        }

        private void gridValues_CurrentCellDirtyStateChangedOcbGrid(object sender, EventArgs e)
        {
            if (gridValues.IsCurrentCellDirty)
                gridValues.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void gridValues_CellValueChangedOcbGrid(object sender, DataGridViewCellEventArgs e)
        {
            if (!_isOcbGridMode || _updatingOcbGrid || e.RowIndex < 0 || e.ColumnIndex < 0 || gridValues.Columns[e.ColumnIndex].Name != "Use")
                return;

            DataGridViewRow row = gridValues.Rows[e.RowIndex];
            var definition = row.Tag as ObjectParameterOcbDefinition;
            bool active = Convert.ToBoolean(row.Cells["Use"].Value ?? false);

            if (active && definition != null && definition.Mode == ObjectParameterOcbMode.FixedValue && !definition.IsCombinable)
            {
                _updatingOcbGrid = true;
                foreach (DataGridViewRow otherRow in gridValues.Rows)
                {
                    if (otherRow == row)
                        continue;

                    var otherDefinition = otherRow.Tag as ObjectParameterOcbDefinition;
                    if (otherDefinition != null && otherDefinition.Mode == ObjectParameterOcbMode.FixedValue && !otherDefinition.IsCombinable)
                        otherRow.Cells["Use"].Value = false;
                }
                _updatingOcbGrid = false;
            }

            ApplyOcbGridValueToHiddenState();
        }

        private void ApplyOcbGridValueToHiddenState()
        {
            if (!_isOcbGridMode || SelectedDefinitionSet == null)
                return;

            short selectedOcb = CalculateOcbFromGrid();
            textProviderId.Text = SelectedDefinitionSet.ProviderId;
            textDefinitionSetId.Text = SelectedDefinitionSet.Id;
            textPresetId.Text = selectedOcb.ToString();
        }

        private short CalculateOcbFromGrid()
        {
            int flags = 0;
            int? fixedValue = null;

            if (!_isOcbGridMode)
                return GetCurrentObjectOcb();

            foreach (DataGridViewRow row in gridValues.Rows)
            {
                if (!Convert.ToBoolean(row.Cells["Use"].Value ?? false))
                    continue;

                var definition = row.Tag as ObjectParameterOcbDefinition;
                if (definition == null)
                    continue;

                if (definition.Mode == ObjectParameterOcbMode.AdditiveFlags || definition.IsCombinable)
                    flags |= definition.Value;
                else
                    fixedValue = definition.Value;
            }

            int value = fixedValue ?? 0;
            value |= flags;

            if (value < short.MinValue)
                return short.MinValue;
            if (value > short.MaxValue)
                return short.MaxValue;

            return (short)value;
        }

        private ObjectParameterMappingStatus GetMappingStatusFromGrid()
        {
            foreach (DataGridViewRow row in gridValues.Rows)
            {
                if (!Convert.ToBoolean(row.Cells["Use"].Value ?? false))
                    continue;

                if (row.Tag is ObjectParameterOcbDefinition definition && definition.MappingStatus == ObjectParameterMappingStatus.Mapped)
                    return ObjectParameterMappingStatus.Mapped;
            }

            return ObjectParameterMappingStatus.Unknown;
        }
    }
}
