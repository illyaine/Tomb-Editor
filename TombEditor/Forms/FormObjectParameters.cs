using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.LevelData.ObjectParameters;

namespace TombEditor.Forms
{
    public partial class FormObjectParameters : DarkForm
    {
        private readonly Level _level;
        private readonly ObjectInstance _instance;
        private readonly ObjectParameterContext _context;
        private bool _isLoading;
        private bool _showHelp;

        public FormObjectParameters(Level level, ObjectInstance instance)
        {
            _level = level;
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
            _context = ObjectParameterContextFactory.FromObject(_level, _instance);

            InitializeComponent();
            InitializeGrid();
            labelObject.Text = GetObjectCaption();
            LoadDefinitions();
            LoadObjectParameters();
            UpdateHelpPanel();
        }

        private void InitializeGrid()
        {
            gridValues.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", Name = "ParameterId", Visible = false });
            gridValues.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Type", Name = "Source", FillWeight = 95, ReadOnly = true });
            gridValues.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Parameter", Name = "ParameterName", FillWeight = 150 });
            gridValues.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Description", Name = "Description", FillWeight = 220, ReadOnly = true });
            gridValues.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Value", Name = "Value", FillWeight = 110 });
            gridValues.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Unit", Name = "Unit", FillWeight = 60, ReadOnly = true });
            gridValues.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", Name = "MappingStatus", FillWeight = 75, ReadOnly = true });
            gridValues.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Example", Name = "Example", Visible = false });
        }

        private string GetObjectCaption()
        {
            ObjectParameterObjectKey key = _context.ObjectKey ?? new ObjectParameterObjectKey();
            string engine = string.IsNullOrWhiteSpace(_context.EngineId) ? "Unknown engine" : _context.EngineId;
            string slot = key.SlotId.HasValue ? "Slot " + key.SlotId.Value : "No slot";
            string scriptId = key.ScriptId.HasValue ? "Script ID " + key.ScriptId.Value : "No script ID";
            string itemIndex = key.ObjectIndex.HasValue ? "ItemIndex " + key.ObjectIndex.Value : "No item index";
            string luaName = string.IsNullOrWhiteSpace(key.LuaName) ? "No Lua name" : "Lua name " + key.LuaName;
            string ocb = key.Ocb.HasValue ? "OCB " + key.Ocb.Value : "No OCB";
            return "Engine " + engine + "   |   " + _context.ObjectTypeId + "   |   " + slot + "   |   " + itemIndex + "   |   " + scriptId + "   |   " + luaName + "   |   " + ocb;
        }

        private void LoadDefinitions()
        {
            _isLoading = true;
            comboDefinitions.Items.Clear();
            comboDefinitions.Items.Add(new DefinitionItem(null, "Custom parameters"));

            foreach (ObjectParameterDefinitionSet definitionSet in ObjectParameterProviderRegistry.GetDefinitionSets(_context))
                comboDefinitions.Items.Add(new DefinitionItem(definitionSet, definitionSet.Name));

            comboDefinitions.SelectedIndex = 0;
            LoadPresets(null);
            _isLoading = false;
        }

        private void LoadObjectParameters()
        {
            _isLoading = true;

            ObjectParameterValueSet valueSet = ObjectParameterStorage.TryGet(_instance);
            if (valueSet != null)
            {
                SelectDefinition(valueSet.ProviderId, valueSet.DefinitionSetId);

                textProviderId.Text = valueSet.ProviderId;
                textDefinitionSetId.Text = valueSet.DefinitionSetId;
                textPresetId.Text = valueSet.PresetId;

                foreach (ObjectParameterValue value in valueSet.Values)
                    AddOrUpdateValueRow(value.ParameterId, value.Value, FindParameterDefinition(value.ParameterId), value.Source, value.MappingStatus);

                SelectPreset(valueSet.PresetId);
            }
            else if (_instance is ItemInstance item)
            {
                AddOrUpdateValueRow("ocb.raw", item.Ocb.ToString(), null, ObjectParameterSource.Ocb, ObjectParameterMappingStatus.Unknown);
            }

            _isLoading = false;
        }

        private void LoadPresets(ObjectParameterDefinitionSet definitionSet)
        {
            comboPresets.Items.Clear();
            comboPresets.Items.Add(new PresetItem(null, "No preset"));

            if (definitionSet != null)
                foreach (ObjectParameterPreset preset in definitionSet.Presets)
                    comboPresets.Items.Add(new PresetItem(preset, preset.Name));

            comboPresets.SelectedIndex = 0;
        }

        private void SelectDefinition(string providerId, string definitionSetId)
        {
            for (int i = 0; i < comboDefinitions.Items.Count; i++)
            {
                var item = comboDefinitions.Items[i] as DefinitionItem;
                if (item?.DefinitionSet == null)
                    continue;

                if (string.Equals(item.DefinitionSet.ProviderId, providerId, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(item.DefinitionSet.Id, definitionSetId, StringComparison.OrdinalIgnoreCase))
                {
                    comboDefinitions.SelectedIndex = i;
                    LoadPresets(item.DefinitionSet);
                    return;
                }
            }

            comboDefinitions.SelectedIndex = 0;
            LoadPresets(null);
        }

        private void SelectPreset(string presetId)
        {
            if (string.IsNullOrEmpty(presetId))
            {
                comboPresets.SelectedIndex = 0;
                return;
            }

            for (int i = 0; i < comboPresets.Items.Count; i++)
            {
                var item = comboPresets.Items[i] as PresetItem;
                if (item?.Preset != null && string.Equals(item.Preset.Id, presetId, StringComparison.OrdinalIgnoreCase))
                {
                    comboPresets.SelectedIndex = i;
                    return;
                }
            }
        }

        private ObjectParameterDefinitionSet SelectedDefinitionSet
        {
            get
            {
                var item = comboDefinitions.SelectedItem as DefinitionItem;
                return item?.DefinitionSet;
            }
        }

        private void comboDefinitions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            ObjectParameterDefinitionSet definitionSet = SelectedDefinitionSet;

            textProviderId.Text = definitionSet?.ProviderId ?? string.Empty;
            textDefinitionSetId.Text = definitionSet?.Id ?? string.Empty;
            LoadPresets(definitionSet);

            if (definitionSet != null && GetValueRowCount() == 0)
                LoadDefaultValues(definitionSet);

            UpdateHelpPanel();
        }

        private void comboPresets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            var item = comboPresets.SelectedItem as PresetItem;
            ObjectParameterPreset preset = item?.Preset;
            textPresetId.Text = preset?.Id ?? string.Empty;

            if (preset != null)
                ApplyPreset(preset);
        }

        private void gridValues_SelectionChanged(object sender, EventArgs e)
        {
            UpdateHelpPanel();
        }

        private void butHelp_Click(object sender, EventArgs e)
        {
            _showHelp = !_showHelp;
            butHelp.Text = _showHelp ? "Hide help" : "Show help";
            UpdateHelpPanel();
        }

        private int GetValueRowCount()
        {
            int count = 0;
            foreach (DataGridViewRow row in gridValues.Rows)
                if (!row.IsNewRow)
                    count++;
            return count;
        }

        private void LoadDefaultValues(ObjectParameterDefinitionSet definitionSet)
        {
            gridValues.Rows.Clear();

            foreach (ObjectParameterGroup group in definitionSet.Groups)
                foreach (ObjectParameterDefinition parameter in group.Parameters)
                    AddOrUpdateValueRow(parameter.Id, parameter.DefaultValue, parameter, parameter.Source, parameter.MappingStatus);

            if (_instance is ItemInstance item && !HasParameterRow("ocb.raw"))
                AddOrUpdateValueRow("ocb.raw", item.Ocb.ToString(), null, ObjectParameterSource.Ocb, ObjectParameterMappingStatus.Unknown);
        }

        private bool HasParameterRow(string parameterId)
        {
            foreach (DataGridViewRow row in gridValues.Rows)
            {
                if (row.IsNewRow)
                    continue;

                string currentId = Convert.ToString(row.Cells["ParameterId"].Value)?.Trim() ?? string.Empty;
                if (string.Equals(currentId, parameterId, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private ObjectParameterDefinition FindParameterDefinition(string parameterId)
        {
            ObjectParameterDefinitionSet definitionSet = SelectedDefinitionSet;
            if (definitionSet == null || string.IsNullOrEmpty(parameterId))
                return null;

            foreach (ObjectParameterGroup group in definitionSet.Groups)
                foreach (ObjectParameterDefinition parameter in group.Parameters)
                    if (string.Equals(parameter.Id, parameterId, StringComparison.OrdinalIgnoreCase))
                        return parameter;

            return null;
        }

        private void AddOrUpdateValueRow(string parameterId, string value, ObjectParameterDefinition parameter, ObjectParameterSource source, ObjectParameterMappingStatus mappingStatus)
        {
            string name = parameter?.Name ?? parameterId;
            string description = parameter?.Description ?? (parameterId == "ocb.raw" ? "Raw OCB value. The meaning is not mapped for this object." : string.Empty);
            string unit = parameter?.Unit ?? string.Empty;
            string example = parameter?.Example ?? string.Empty;

            foreach (DataGridViewRow row in gridValues.Rows)
            {
                if (row.IsNewRow)
                    continue;

                string currentId = Convert.ToString(row.Cells["ParameterId"].Value)?.Trim() ?? string.Empty;
                if (string.Equals(currentId, parameterId, StringComparison.OrdinalIgnoreCase))
                {
                    row.Cells["Source"].Value = GetSourceText(source);
                    row.Cells["ParameterName"].Value = name;
                    row.Cells["Description"].Value = description;
                    row.Cells["Value"].Value = value;
                    row.Cells["Unit"].Value = unit;
                    row.Cells["MappingStatus"].Value = mappingStatus.ToString();
                    row.Cells["Example"].Value = example;
                    row.Cells["Source"].ToolTipText = GetSourceDescription(source);
                    return;
                }
            }

            int index = gridValues.Rows.Add(parameterId, GetSourceText(source), name, description, value, unit, mappingStatus.ToString(), example);
            gridValues.Rows[index].Cells["Source"].ToolTipText = GetSourceDescription(source);
        }

        private static string GetSourceText(ObjectParameterSource source)
        {
            switch (source)
            {
                case ObjectParameterSource.Ocb:
                    return "Legacy OCB";
                case ObjectParameterSource.Ten:
                    return "TEN";
                case ObjectParameterSource.Script:
                    return "Script";
                case ObjectParameterSource.Custom:
                    return "Custom";
                default:
                    return "Unknown";
            }
        }

        private static string GetSourceDescription(ObjectParameterSource source)
        {
            switch (source)
            {
                case ObjectParameterSource.Ocb:
                    return "Legacy OCB compatibility value. Meaning depends on the selected object/slot.";
                case ObjectParameterSource.Ten:
                    return "Native TombEngine object parameter.";
                case ObjectParameterSource.Script:
                    return "Script-facing object option.";
                case ObjectParameterSource.Custom:
                    return "Parameter supplied by an external provider.";
                default:
                    return "Unknown parameter source.";
            }
        }

        private static ObjectParameterSource ParseSource(string value)
        {
            switch (value)
            {
                case "[OCB]":
                case "Legacy OCB":
                    return ObjectParameterSource.Ocb;
                case "[TEN]":
                case "TEN":
                    return ObjectParameterSource.Ten;
                case "[Script]":
                case "Script":
                    return ObjectParameterSource.Script;
                case "[Custom]":
                case "Custom":
                    return ObjectParameterSource.Custom;
                default:
                    return ObjectParameterSource.Unknown;
            }
        }

        private static ObjectParameterMappingStatus ParseMappingStatus(string value)
        {
            if (Enum.TryParse(value, out ObjectParameterMappingStatus result))
                return result;

            return ObjectParameterMappingStatus.Unknown;
        }

        private void ApplyPreset(ObjectParameterPreset preset)
        {
            foreach (ObjectParameterValue value in preset.Values)
                AddOrUpdateValueRow(value.ParameterId, value.Value, FindParameterDefinition(value.ParameterId), value.Source, value.MappingStatus);
        }

        private void UpdateHelpPanel()
        {
            labelHelp.Visible = _showHelp;
            if (!_showHelp)
                return;

            DataGridViewRow row = gridValues.CurrentRow;
            if (row == null || row.IsNewRow)
            {
                labelHelp.Text = "Select a parameter to show help.";
                return;
            }

            string description = Convert.ToString(row.Cells["Description"].Value) ?? string.Empty;
            string example = Convert.ToString(row.Cells["Example"].Value) ?? string.Empty;
            string status = Convert.ToString(row.Cells["MappingStatus"].Value) ?? string.Empty;
            string source = Convert.ToString(row.Cells["Source"].Value) ?? string.Empty;
            string sourceDescription = GetSourceDescription(ParseSource(source));
            string nameWarning = ObjectParameterSupport.SupportsRuntimeParameters(_level) && string.IsNullOrWhiteSpace(_context.ObjectKey?.LuaName) ? "Hint: assign a Lua name for clear TEN runtime references. " : string.Empty;
            string prefix = source + ": " + sourceDescription + " ";

            if (string.IsNullOrWhiteSpace(description) && string.IsNullOrWhiteSpace(example))
                labelHelp.Text = nameWarning + prefix + "No additional help for this parameter. Status: " + status;
            else if (string.IsNullOrWhiteSpace(example))
                labelHelp.Text = nameWarning + prefix + description + " Status: " + status;
            else if (string.IsNullOrWhiteSpace(description))
                labelHelp.Text = nameWarning + prefix + "Example: " + example + " Status: " + status;
            else
                labelHelp.Text = nameWarning + prefix + description + "  Example: " + example + " Status: " + status;
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            ObjectParameterValueSet valueSet = ObjectParameterStorage.GetOrCreate(_instance);
            valueSet.ProviderId = textProviderId.Text.Trim();
            valueSet.DefinitionSetId = textDefinitionSetId.Text.Trim();
            valueSet.PresetId = textPresetId.Text.Trim();
            valueSet.Values.Clear();

            foreach (DataGridViewRow row in gridValues.Rows)
            {
                if (row.IsNewRow)
                    continue;

                string parameterId = Convert.ToString(row.Cells["ParameterId"].Value)?.Trim() ?? string.Empty;
                string value = Convert.ToString(row.Cells["Value"].Value)?.Trim() ?? string.Empty;
                string source = Convert.ToString(row.Cells["Source"].Value)?.Trim() ?? string.Empty;
                string mappingStatus = Convert.ToString(row.Cells["MappingStatus"].Value)?.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(parameterId) && string.IsNullOrEmpty(value))
                    continue;

                valueSet.Values.Add(new ObjectParameterValue
                {
                    ParameterId = parameterId,
                    Value = value,
                    Source = ParseSource(source),
                    MappingStatus = ParseMappingStatus(mappingStatus)
                });
            }

            List<ObjectParameterValidationMessage> messages = ObjectParameterProviderRegistry.Validate(_context, valueSet).ToList();
            List<ObjectParameterValidationMessage> errors = messages.Where(message => message.Severity == ObjectParameterMessageSeverity.Error).ToList();
            if (errors.Count != 0)
            {
                MessageBox.Show(string.Join(Environment.NewLine, errors.Select(message => message.Message)), "Object Parameters", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ObjectParameterStorage.Set(_instance, valueSet);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private sealed class DefinitionItem
        {
            public DefinitionItem(ObjectParameterDefinitionSet definitionSet, string text)
            {
                DefinitionSet = definitionSet;
                Text = text;
            }

            public ObjectParameterDefinitionSet DefinitionSet { get; }
            public string Text { get; }

            public override string ToString() => Text;
        }

        private sealed class PresetItem
        {
            public PresetItem(ObjectParameterPreset preset, string text)
            {
                Preset = preset;
                Text = text;
            }

            public ObjectParameterPreset Preset { get; }
            public string Text { get; }

            public override string ToString() => Text;
        }
    }
}
