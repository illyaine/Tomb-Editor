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
            gridOcbCodes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "OCB", Name = "OcbValue", FillWeight = 60, ReadOnly = true });
            gridOcbCodes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", Name = "Name", FillWeight = 150, ReadOnly = true });
            gridOcbCodes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Description", Name = "Description", FillWeight = 260, ReadOnly = true });
            gridOcbCodes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Combinable", Name = "Combinable", FillWeight = 80, ReadOnly = true });
            gridOcbCodes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Group", Name = "Group", FillWeight = 90, ReadOnly = true });
            gridOcbCodes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", Name = "Status", FillWeight = 80, ReadOnly = true });
            gridOcbCodes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Example", Name = "Example", FillWeight = 140, ReadOnly = true });
            gridOcbCodes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Warning", Name = "Warning", FillWeight = 150, ReadOnly = true });
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
            gridOcbCodes.Rows.Clear();

            bool hasCurrentOcbMapping = false;
            short? currentOcb = (_instance as ItemInstance)?.Ocb;

            if (_definitionSet != null)
            {
                foreach (ObjectParameterOcbDefinition definition in _definitionSet.OcbDefinitions)
                {
                    if (currentOcb.HasValue && definition.Value == currentOcb.Value)
                        hasCurrentOcbMapping = true;

                    AddDefinitionRow(definition);
                }
            }

            if (currentOcb.HasValue && !hasCurrentOcbMapping)
            {
                gridOcbCodes.Rows.Add(
                    currentOcb.Value,
                    "Unknown / undocumented",
                    "The current raw OCB value is preserved. No confirmed object-specific meaning is available yet.",
                    "No",
                    "Raw",
                    ObjectParameterMappingStatus.Unknown.ToString(),
                    string.Empty,
                    "Do not overwrite this value unless you know what it does.");
            }
        }

        private void AddDefinitionRow(ObjectParameterOcbDefinition definition)
        {
            gridOcbCodes.Rows.Add(
                definition.Value,
                definition.Name,
                definition.Description,
                definition.IsCombinable ? "Yes" : "No",
                definition.Group,
                definition.MappingStatus.ToString(),
                definition.Example,
                definition.Warning);
        }

        private void butClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
