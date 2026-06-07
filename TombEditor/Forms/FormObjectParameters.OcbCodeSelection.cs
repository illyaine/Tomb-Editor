using DarkUI.Controls;
using System;
using System.Drawing;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.LevelData.ObjectParameters;

namespace TombEditor.Forms
{
    public partial class FormObjectParameters
    {
        private DarkDataGridView _ocbGrid;
        private bool _updatingOcbGrid;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SelectFirstOcbDefinitionSet();
            gridValues.CellValueChanged += gridValues_CellValueChangedOcbGrid;
            gridValues.CurrentCellDirtyStateChanged += gridValues_CurrentCellDirtyStateChangedOcbGrid;
            UpdateOcbGridVisibility();
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

        private void gridValues_CurrentCellDirtyStateChangedOcbGrid(object sender, EventArgs e)
        {
            if (gridValues.IsCurrentCellDirty)
                gridValues.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void gridValues_CellValueChangedOcbGrid(object sender, DataGridViewCellEventArgs e)
        {
            if (_updatingOcbGrid || e.RowIndex < 0 || e.ColumnIndex < 0 || gridValues.Columns[e.ColumnIndex].Name != "Value")
                return;

            string parameterId = Convert.ToString(gridValues.Rows[e.RowIndex].Cells["ParameterId"].Value)?.Trim() ?? string.Empty;
            if (string.Equals(parameterId, "ocb.raw", StringComparison.OrdinalIgnoreCase))
                UpdateOcbGridVisibility();
        }

        private void EnsureOcbGrid()
        {
            if (_ocbGrid != null)
                return;

            _ocbGrid = new DarkDataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToDragDropRows = false,
                AllowUserToPasteCells = false,
                AllowUserToResizeRows = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.Single,
                ColumnHeadersHeight = 24,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                EditMode = DataGridViewEditMode.EditOnEnter,
                Location = gridValues.Location,
                MultiSelect = false,
                Name = "gridOcbCodes",
                PreventScrollOnCtrl = false,
                RowHeadersVisible = false,
                ScrollBars = ScrollBars.Both,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Size = gridValues.Size,
                TabIndex = gridValues.TabIndex,
                UseAlternativeDragDropMethod = false
            };

            _ocbGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Slot", Name = "Slot", FillWeight = 160, ReadOnly = true });
            _ocbGrid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Use", Name = "Use", FillWeight = 40, ReadOnly = false });
            _ocbGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "OCB", Name = "OcbValue", FillWeight = 55, ReadOnly = true });
            _ocbGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", Name = "Name", FillWeight = 150, ReadOnly = true });
            _ocbGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Description", Name = "Description", FillWeight = 330, ReadOnly = true });
            _ocbGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mode", Name = "Mode", FillWeight = 85, ReadOnly = true });
            _ocbGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", Name = "Status", FillWeight = 80, ReadOnly = true });

            _ocbGrid.CurrentCellDirtyStateChanged += ocbGrid_CurrentCellDirtyStateChanged;
            _ocbGrid.CellValueChanged += ocbGrid_CellValueChanged;

            darkGroupBox1.Controls.Add(_ocbGrid);
            _ocbGrid.BringToFront();
        }

        private void UpdateOcbGridVisibility()
        {
            ObjectParameterDefinitionSet definitionSet = SelectedDefinitionSet;
            bool hasOcbDefinitions = definitionSet != null && definitionSet.OcbDefinitions.Count != 0;

            comboDefinitions.Visible = !hasOcbDefinitions;
            darkLabel1.Visible = !hasOcbDefinitions;
            comboPresets.Visible = false;
            darkLabel2.Visible = false;
            butOcbCodes.Visible = false;
            gridValues.Visible = !hasOcbDefinitions;
            darkGroupBox1.Text = hasOcbDefinitions ? "Known OCB codes for this object" : "Object codes && parameters";

            if (!hasOcbDefinitions)
            {
                if (_ocbGrid != null)
                    _ocbGrid.Visible = false;
                return;
            }

            EnsureOcbGrid();
            ReloadOcbGrid(definitionSet);
        }

        private void ReloadOcbGrid(ObjectParameterDefinitionSet definitionSet)
        {
            _updatingOcbGrid = true;
            _ocbGrid.Visible = true;
            _ocbGrid.Rows.Clear();

            short currentOcb = GetRawOcbValue();
            string slotText = GetSlotText(definitionSet);

            foreach (ObjectParameterOcbDefinition definition in definitionSet.OcbDefinitions)
                AddOcbRow(definition, slotText, IsDefinitionActive(definition, currentOcb));

            if (CalculateOcbFromGrid() != currentOcb)
            {
                foreach (DataGridViewRow row in _ocbGrid.Rows)
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

            _updatingOcbGrid = false;
            ApplyOcbGridValueToRawRow(false);
        }

        private string GetSlotText(ObjectParameterDefinitionSet definitionSet)
        {
            string slot = _context.ObjectKey?.SlotId.HasValue == true ? "Slot " + _context.ObjectKey.SlotId.Value : "No slot";
            string name = (definitionSet.Name ?? string.Empty).Replace("TEN OCB:", string.Empty).Trim();
            return string.IsNullOrEmpty(name) ? slot : slot + " - " + name;
        }

        private short GetRawOcbValue()
        {
            foreach (DataGridViewRow row in gridValues.Rows)
            {
                if (row.IsNewRow)
                    continue;

                string parameterId = Convert.ToString(row.Cells["ParameterId"].Value)?.Trim() ?? string.Empty;
                if (!string.Equals(parameterId, "ocb.raw", StringComparison.OrdinalIgnoreCase))
                    continue;

                string value = Convert.ToString(row.Cells["Value"].Value)?.Trim() ?? string.Empty;
                if (short.TryParse(value, out short parsedOcb))
                    return parsedOcb;
            }

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
            int index = _ocbGrid.Rows.Add(
                slotText,
                active,
                definition.Value,
                definition.Name,
                definition.Description,
                GetModeText(definition),
                definition.MappingStatus.ToString());

            _ocbGrid.Rows[index].Tag = definition;
        }

        private static string GetModeText(ObjectParameterOcbDefinition definition)
        {
            if (definition.Mode == ObjectParameterOcbMode.AdditiveFlags || definition.IsCombinable)
                return "Checkbox flag";

            return "Single value";
        }

        private void ocbGrid_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (_ocbGrid.IsCurrentCellDirty)
                _ocbGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void ocbGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_updatingOcbGrid || e.RowIndex < 0 || e.ColumnIndex < 0 || _ocbGrid.Columns[e.ColumnIndex].Name != "Use")
                return;

            DataGridViewRow row = _ocbGrid.Rows[e.RowIndex];
            var definition = row.Tag as ObjectParameterOcbDefinition;
            bool active = Convert.ToBoolean(row.Cells["Use"].Value ?? false);

            if (active && definition != null && definition.Mode == ObjectParameterOcbMode.FixedValue && !definition.IsCombinable)
            {
                _updatingOcbGrid = true;
                foreach (DataGridViewRow otherRow in _ocbGrid.Rows)
                {
                    if (otherRow == row)
                        continue;

                    var otherDefinition = otherRow.Tag as ObjectParameterOcbDefinition;
                    if (otherDefinition != null && otherDefinition.Mode == ObjectParameterOcbMode.FixedValue && !otherDefinition.IsCombinable)
                        otherRow.Cells["Use"].Value = false;
                }
                _updatingOcbGrid = false;
            }

            ApplyOcbGridValueToRawRow(true);
        }

        private void ApplyOcbGridValueToRawRow(bool updateHelp)
        {
            if (_ocbGrid == null || SelectedDefinitionSet == null)
                return;

            short selectedOcb = CalculateOcbFromGrid();
            ObjectParameterDefinition parameter = FindParameterDefinition("ocb.raw");
            ObjectParameterSource source = SelectedDefinitionSet.Source;
            ObjectParameterMappingStatus status = GetMappingStatusFromGrid();

            _updatingOcbGrid = true;
            AddOrUpdateValueRow("ocb.raw", selectedOcb.ToString(), parameter, source, status);
            textProviderId.Text = SelectedDefinitionSet.ProviderId;
            textDefinitionSetId.Text = SelectedDefinitionSet.Id;
            textPresetId.Text = string.Empty;
            _updatingOcbGrid = false;

            if (updateHelp)
                UpdateHelpPanel();
        }

        private ObjectParameterMappingStatus GetMappingStatusFromGrid()
        {
            foreach (DataGridViewRow row in _ocbGrid.Rows)
            {
                if (!Convert.ToBoolean(row.Cells["Use"].Value ?? false))
                    continue;

                if (row.Tag is ObjectParameterOcbDefinition definition && definition.MappingStatus == ObjectParameterMappingStatus.Mapped)
                    return ObjectParameterMappingStatus.Mapped;
            }

            return ObjectParameterMappingStatus.Unknown;
        }

        private short CalculateOcbFromGrid()
        {
            int flags = 0;
            int? fixedValue = null;

            if (_ocbGrid == null)
                return GetRawOcbValue();

            foreach (DataGridViewRow row in _ocbGrid.Rows)
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
    }
}
