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
        private DarkGroupBox _inlineOcbGroup;
        private DataGridView _inlineOcbGrid;
        private bool _inlineOcbLayoutApplied;
        private bool _inlineOcbIsUpdating;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (comboDefinitions.Items.Count > 1 && SelectedDefinitionSet == null)
                comboDefinitions.SelectedIndex = 1;

            comboDefinitions.SelectedIndexChanged += comboDefinitions_SelectedIndexChangedInlineOcbList;
            gridValues.CellValueChanged += gridValues_CellValueChangedInlineOcbList;
            gridValues.CurrentCellDirtyStateChanged += gridValues_CurrentCellDirtyStateChangedInlineOcbList;

            UpdateInlineOcbList();
        }

        private void comboDefinitions_SelectedIndexChangedInlineOcbList(object sender, EventArgs e)
        {
            UpdateInlineOcbList();
        }

        private void gridValues_CurrentCellDirtyStateChangedInlineOcbList(object sender, EventArgs e)
        {
            if (gridValues.IsCurrentCellDirty)
                gridValues.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void gridValues_CellValueChangedInlineOcbList(object sender, DataGridViewCellEventArgs e)
        {
            if (_inlineOcbIsUpdating || e.RowIndex < 0 || e.ColumnIndex < 0 || gridValues.Columns[e.ColumnIndex].Name != "Value")
                return;

            string parameterId = Convert.ToString(gridValues.Rows[e.RowIndex].Cells["ParameterId"].Value)?.Trim() ?? string.Empty;
            if (string.Equals(parameterId, "ocb.raw", StringComparison.OrdinalIgnoreCase))
                UpdateInlineOcbList();
        }

        private void EnsureInlineOcbList()
        {
            if (_inlineOcbGroup != null)
                return;

            _inlineOcbGroup = new DarkGroupBox
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(darkGroupBox1.Left, darkGroupBox1.Top),
                Name = "inlineOcbGroup",
                Size = new Size(darkGroupBox1.Width, 145),
                TabStop = false,
                Text = "Known OCB codes for this object"
            };

            _inlineOcbGrid = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.FromArgb(43, 43, 43),
                BorderStyle = BorderStyle.FixedSingle,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                Location = new Point(7, 19),
                MultiSelect = false,
                Name = "inlineOcbGrid",
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Size = new Size(_inlineOcbGroup.Width - 13, _inlineOcbGroup.Height - 26),
                TabIndex = 0
            };

            _inlineOcbGrid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Use", Name = "Use", FillWeight = 40, ReadOnly = false });
            _inlineOcbGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Slot", Name = "Slot", FillWeight = 140, ReadOnly = true });
            _inlineOcbGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "OCB", Name = "OcbValue", FillWeight = 55, ReadOnly = true });
            _inlineOcbGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", Name = "Name", FillWeight = 150, ReadOnly = true });
            _inlineOcbGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Description", Name = "Description", FillWeight = 300, ReadOnly = true });
            _inlineOcbGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mode", Name = "Mode", FillWeight = 85, ReadOnly = true });
            _inlineOcbGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", Name = "Status", FillWeight = 80, ReadOnly = true });

            _inlineOcbGrid.CurrentCellDirtyStateChanged += inlineOcbGrid_CurrentCellDirtyStateChanged;
            _inlineOcbGrid.CellValueChanged += inlineOcbGrid_CellValueChanged;

            _inlineOcbGroup.Controls.Add(_inlineOcbGrid);
            Controls.Add(_inlineOcbGroup);
            _inlineOcbGroup.BringToFront();
        }

        private void ApplyInlineOcbLayout()
        {
            if (_inlineOcbLayoutApplied)
                return;

            int offset = _inlineOcbGroup.Height + 8;
            darkGroupBox1.Top += offset;
            darkGroupBox1.Height = Math.Max(120, darkGroupBox1.Height - offset);
            _inlineOcbLayoutApplied = true;
        }

        private void UpdateInlineOcbList()
        {
            ObjectParameterDefinitionSet definitionSet = SelectedDefinitionSet;
            bool hasDefinitions = definitionSet != null && definitionSet.OcbDefinitions.Count != 0;

            comboPresets.Visible = !hasDefinitions;
            darkLabel2.Visible = !hasDefinitions;
            butOcbCodes.Visible = false;

            if (!hasDefinitions)
            {
                if (_inlineOcbGroup != null)
                    _inlineOcbGroup.Visible = false;
                return;
            }

            EnsureInlineOcbList();
            ApplyInlineOcbLayout();

            _inlineOcbGroup.Visible = true;
            _inlineOcbIsUpdating = true;
            _inlineOcbGrid.Rows.Clear();

            short currentOcb = GetRawOcbValueForInlineList();
            string slotText = GetInlineOcbSlotText(definitionSet);

            foreach (ObjectParameterOcbDefinition definition in definitionSet.OcbDefinitions)
                AddInlineOcbRow(definition, slotText, IsInlineOcbDefinitionActive(definition, currentOcb));

            if (CalculateInlineOcbValueFromRows() != currentOcb)
            {
                foreach (DataGridViewRow row in _inlineOcbGrid.Rows)
                    row.Cells["Use"].Value = false;

                AddInlineOcbRow(new ObjectParameterOcbDefinition
                {
                    Value = currentOcb,
                    Name = "Unknown / undocumented",
                    Description = "The current raw OCB value is preserved. No confirmed object-specific meaning is available yet.",
                    Group = "Raw",
                    Mode = ObjectParameterOcbMode.FixedValue,
                    MappingStatus = ObjectParameterMappingStatus.Unknown,
                    Warning = "Do not overwrite this value unless you know what it does."
                }, slotText, true);
            }

            _inlineOcbIsUpdating = false;
            ApplyInlineOcbValueToRawRow(false);
        }

        private string GetInlineOcbSlotText(ObjectParameterDefinitionSet definitionSet)
        {
            string slot = _context.ObjectKey?.SlotId.HasValue == true ? "Slot " + _context.ObjectKey.SlotId.Value : "No slot";
            return slot + " - " + definitionSet.Name;
        }

        private short GetRawOcbValueForInlineList()
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

        private static bool IsInlineOcbDefinitionActive(ObjectParameterOcbDefinition definition, short currentOcb)
        {
            if (definition.Mode == ObjectParameterOcbMode.AdditiveFlags || definition.IsCombinable)
                return definition.Value > 0 && (currentOcb & definition.Value) == definition.Value;

            return definition.Value == currentOcb;
        }

        private void AddInlineOcbRow(ObjectParameterOcbDefinition definition, string slotText, bool isActive)
        {
            int index = _inlineOcbGrid.Rows.Add(
                isActive,
                slotText,
                definition.Value,
                definition.Name,
                definition.Description,
                GetInlineOcbModeText(definition),
                definition.MappingStatus.ToString());

            _inlineOcbGrid.Rows[index].Tag = definition;
        }

        private static string GetInlineOcbModeText(ObjectParameterOcbDefinition definition)
        {
            if (definition.Mode == ObjectParameterOcbMode.AdditiveFlags || definition.IsCombinable)
                return "Checkbox flag";

            return "Single value";
        }

        private void inlineOcbGrid_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (_inlineOcbGrid.IsCurrentCellDirty)
                _inlineOcbGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void inlineOcbGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_inlineOcbIsUpdating || e.RowIndex < 0 || e.ColumnIndex < 0 || _inlineOcbGrid.Columns[e.ColumnIndex].Name != "Use")
                return;

            DataGridViewRow row = _inlineOcbGrid.Rows[e.RowIndex];
            var definition = row.Tag as ObjectParameterOcbDefinition;
            bool isChecked = Convert.ToBoolean(row.Cells["Use"].Value ?? false);

            if (isChecked && definition != null && definition.Mode == ObjectParameterOcbMode.FixedValue && !definition.IsCombinable)
            {
                _inlineOcbIsUpdating = true;
                foreach (DataGridViewRow otherRow in _inlineOcbGrid.Rows)
                {
                    if (otherRow == row)
                        continue;

                    var otherDefinition = otherRow.Tag as ObjectParameterOcbDefinition;
                    if (otherDefinition != null && otherDefinition.Mode == ObjectParameterOcbMode.FixedValue && !otherDefinition.IsCombinable)
                        otherRow.Cells["Use"].Value = false;
                }
                _inlineOcbIsUpdating = false;
            }

            ApplyInlineOcbValueToRawRow(true);
        }

        private void ApplyInlineOcbValueToRawRow(bool updateHelp)
        {
            if (_inlineOcbGrid == null || SelectedDefinitionSet == null)
                return;

            short selectedOcb = CalculateInlineOcbValueFromRows();
            ObjectParameterDefinition parameter = FindParameterDefinition("ocb.raw");
            ObjectParameterSource source = SelectedDefinitionSet.Source;
            ObjectParameterMappingStatus status = GetInlineOcbMappingStatus();

            _inlineOcbIsUpdating = true;
            AddOrUpdateValueRow("ocb.raw", selectedOcb.ToString(), parameter, source, status);
            textProviderId.Text = SelectedDefinitionSet.ProviderId;
            textDefinitionSetId.Text = SelectedDefinitionSet.Id;
            textPresetId.Text = string.Empty;
            _inlineOcbIsUpdating = false;

            if (updateHelp)
                UpdateHelpPanel();
        }

        private ObjectParameterMappingStatus GetInlineOcbMappingStatus()
        {
            foreach (DataGridViewRow row in _inlineOcbGrid.Rows)
            {
                if (!Convert.ToBoolean(row.Cells["Use"].Value ?? false))
                    continue;

                if (row.Tag is ObjectParameterOcbDefinition definition && definition.MappingStatus == ObjectParameterMappingStatus.Mapped)
                    return ObjectParameterMappingStatus.Mapped;
            }

            return ObjectParameterMappingStatus.Unknown;
        }

        private short CalculateInlineOcbValueFromRows()
        {
            int flags = 0;
            int? fixedValue = null;

            if (_inlineOcbGrid == null)
                return GetRawOcbValueForInlineList();

            foreach (DataGridViewRow row in _inlineOcbGrid.Rows)
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
