using DarkUI.Forms;
using System;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.LevelData.ObjectParameters;

namespace TombEditor.Forms
{
    public partial class FormObjectOcbCodes : DarkForm
    {
        private readonly ObjectInstance _instance;
        private readonly ObjectParameterDefinitionSet _definitionSet;
        private bool _isUpdating;

        public short? SelectedOcbValue { get; private set; }
        public ObjectParameterMappingStatus SelectedMappingStatus { get; private set; } = ObjectParameterMappingStatus.Unknown;

        public FormObjectOcbCodes(ObjectInstance instance, ObjectParameterDefinitionSet definitionSet)
        {
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
            _definitionSet = definitionSet;

            InitializeComponent();
            InitializeGrid();
            labelObject.Text = GetObjectCaption();
            LoadOcbDefinitions();
        }

        private void InitializeGrid()
        {
            gridOcbCodes.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Use", Name = "Use", FillWeight = 45, ReadOnly = false });
            gridOcbCodes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "OCB", Name = "OcbValue", FillWeight = 60, ReadOnly = true });
            gridOcbCodes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", Name = "Name", FillWeight = 150, ReadOnly = true });
            gridOcbCodes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Description", Name = "Description", FillWeight = 260, ReadOnly = true });
            gridOcbCodes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mode", Name = "Mode", FillWeight = 85, ReadOnly = true });
            gridOcbCodes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Group", Name = "Group", FillWeight = 90, ReadOnly = true });
            gridOcbCodes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", Name = "Status", FillWeight = 80, ReadOnly = true });
            gridOcbCodes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Warning", Name = "Warning", FillWeight = 150, ReadOnly = true });

            gridOcbCodes.CurrentCellDirtyStateChanged += gridOcbCodes_CurrentCellDirtyStateChanged;
            gridOcbCodes.CellValueChanged += gridOcbCodes_CellValueChanged;
        }

        private string GetObjectCaption()
        {
            ObjectParameterObjectKey key = ObjectParameterStorage.GetObjectKey(_instance);
            string slot = key.SlotId.HasValue ? "Slot " + key.SlotId.Value : "No slot";
            string itemIndex = key.ObjectIndex.HasValue ? "ItemIndex " + key.ObjectIndex.Value : "No item index";
            string luaName = string.IsNullOrWhiteSpace(key.LuaName) ? "No Lua name" : "Lua name " + key.LuaName;
            string ocb = key.Ocb.HasValue ? "Current OCB " + key.Ocb.Value : "No OCB";
            return key.ObjectTypeId + "   |   " + slot + "   |   " + itemIndex + "   |   " + luaName + "   |   " + ocb;
        }

        private void LoadOcbDefinitions()
        {
            _isUpdating = true;
            gridOcbCodes.Rows.Clear();

            short? currentOcb = GetCurrentObjectOcb();

            if (_definitionSet != null)
            {
                foreach (ObjectParameterOcbDefinition definition in _definitionSet.OcbDefinitions)
                    AddDefinitionRow(definition, IsDefinitionActive(definition, currentOcb));
            }

            if (currentOcb.HasValue && CalculateSelectedOcbFromRows() != currentOcb.Value)
            {
                foreach (DataGridViewRow row in gridOcbCodes.Rows)
                    row.Cells["Use"].Value = false;

                AddDefinitionRow(new ObjectParameterOcbDefinition
                {
                    Value = currentOcb.Value,
                    Name = "Unknown / undocumented",
                    Description = "The current raw OCB value is preserved. No confirmed object-specific meaning is available yet.",
                    Group = "Raw",
                    Mode = ObjectParameterOcbMode.FixedValue,
                    MappingStatus = ObjectParameterMappingStatus.Unknown,
                    Warning = "Do not overwrite this value unless you know what it does."
                }, true);
            }

            _isUpdating = false;
            UpdateSelectedOcbFromGrid();
        }

        private short? GetCurrentObjectOcb()
        {
            if (_instance is ItemInstance item)
                return item.Ocb;
            if (_instance is StaticInstance staticInstance)
                return staticInstance.Ocb;
            return null;
        }

        private static bool IsDefinitionActive(ObjectParameterOcbDefinition definition, short? currentOcb)
        {
            if (!currentOcb.HasValue)
                return false;

            if (definition.Mode == ObjectParameterOcbMode.AdditiveFlags || definition.IsCombinable)
                return definition.Value > 0 && (currentOcb.Value & definition.Value) == definition.Value;

            return definition.Value == currentOcb.Value;
        }

        private void AddDefinitionRow(ObjectParameterOcbDefinition definition, bool isActive)
        {
            int index = gridOcbCodes.Rows.Add(
                isActive,
                definition.Value,
                definition.Name,
                definition.Description,
                GetModeText(definition),
                definition.Group,
                definition.MappingStatus.ToString(),
                definition.Warning);

            gridOcbCodes.Rows[index].Tag = definition;
        }

        private static string GetModeText(ObjectParameterOcbDefinition definition)
        {
            if (definition.Mode == ObjectParameterOcbMode.AdditiveFlags || definition.IsCombinable)
                return "Checkbox flag";

            return "Single value";
        }

        private void gridOcbCodes_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (gridOcbCodes.IsCurrentCellDirty)
                gridOcbCodes.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void gridOcbCodes_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_isUpdating || e.RowIndex < 0 || e.ColumnIndex < 0 || gridOcbCodes.Columns[e.ColumnIndex].Name != "Use")
                return;

            var row = gridOcbCodes.Rows[e.RowIndex];
            var definition = row.Tag as ObjectParameterOcbDefinition;
            bool isChecked = Convert.ToBoolean(row.Cells["Use"].Value ?? false);

            if (isChecked && definition != null && definition.Mode == ObjectParameterOcbMode.FixedValue && !definition.IsCombinable)
            {
                _isUpdating = true;
                foreach (DataGridViewRow otherRow in gridOcbCodes.Rows)
                {
                    if (otherRow == row)
                        continue;

                    var otherDefinition = otherRow.Tag as ObjectParameterOcbDefinition;
                    if (otherDefinition != null && otherDefinition.Mode == ObjectParameterOcbMode.FixedValue && !otherDefinition.IsCombinable)
                        otherRow.Cells["Use"].Value = false;
                }
                _isUpdating = false;
            }

            UpdateSelectedOcbFromGrid();
        }

        private void UpdateSelectedOcbFromGrid()
        {
            short selectedOcb = CalculateSelectedOcbFromRows();
            SelectedOcbValue = selectedOcb;
            SelectedMappingStatus = ObjectParameterMappingStatus.Unknown;

            foreach (DataGridViewRow row in gridOcbCodes.Rows)
            {
                if (!Convert.ToBoolean(row.Cells["Use"].Value ?? false))
                    continue;

                if (row.Tag is ObjectParameterOcbDefinition definition && definition.MappingStatus == ObjectParameterMappingStatus.Mapped)
                {
                    SelectedMappingStatus = ObjectParameterMappingStatus.Mapped;
                    break;
                }
            }

            labelObject.Text = GetObjectCaption() + "   |   Selected OCB " + selectedOcb;
        }

        private short CalculateSelectedOcbFromRows()
        {
            int flags = 0;
            int? fixedValue = null;

            foreach (DataGridViewRow row in gridOcbCodes.Rows)
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

        private void butUseSelected_Click(object sender, EventArgs e)
        {
            UpdateSelectedOcbFromGrid();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void butClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
