using DarkUI.Config;
using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.LevelData.ObjectParameters;

namespace TombEditor.Forms
{
    public class FormObjectParameters : DarkForm
    {
        private readonly Level _level;
        private readonly ObjectInstance _instance;
        private readonly ObjectParameterContext _context;
        private bool _isLoading;
        private bool _showHelp;

        private DarkLabel _labelObject;
        private DarkComboBox _comboDefinitions;
        private DarkComboBox _comboPresets;
        private DarkTextBox _textProviderId;
        private DarkTextBox _textDefinitionSetId;
        private DarkTextBox _textPresetId;
        private DataGridView _gridValues;
        private DarkButton _butHelp;
        private DarkLabel _labelHelp;

        public FormObjectParameters(Level level, ObjectInstance instance)
        {
            _level = level;
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
            _context = ObjectParameterContextFactory.FromObject(_level, _instance);

            InitializeComponent();
            LoadDefinitions();
            LoadObjectParameters();
            UpdateHelpPanel();
        }

        private void InitializeComponent()
        {
            Text = "Object Parameters";
            MinimizeBox = false;
            MaximizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            ClientSize = new Size(760, 500);
            BackColor = Colors.DarkBackground;

            _labelObject = new DarkLabel
            {
                Text = GetObjectCaption(),
                Location = new Point(10, 10),
                Size = new Size(740, 20)
            };
            Controls.Add(_labelObject);

            Controls.Add(CreateLabel("Definition", 10, 42));
            _comboDefinitions = new DarkComboBox
            {
                Location = new Point(105, 38),
                Size = new Size(405, 22)
            };
            _comboDefinitions.SelectedIndexChanged += comboDefinitions_SelectedIndexChanged;
            Controls.Add(_comboDefinitions);

            Controls.Add(CreateLabel("Preset", 520, 42));
            _comboPresets = new DarkComboBox
            {
                Location = new Point(575, 38),
                Size = new Size(175, 22)
            };
            _comboPresets.SelectedIndexChanged += comboPresets_SelectedIndexChanged;
            Controls.Add(_comboPresets);

            Controls.Add(CreateLabel("Provider", 10, 70));
            _textProviderId = CreateTextBox(105, 66, 185);
            _textProviderId.ReadOnly = true;
            Controls.Add(_textProviderId);

            Controls.Add(CreateLabel("Definition ID", 300, 70));
            _textDefinitionSetId = CreateTextBox(390, 66, 180);
            Controls.Add(_textDefinitionSetId);

            Controls.Add(CreateLabel("Preset ID", 580, 70));
            _textPresetId = CreateTextBox(640, 66, 110);
            Controls.Add(_textPresetId);

            _gridValues = new DataGridView
            {
                Location = new Point(10, 98),
                Size = new Size(740, 310),
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Colors.DarkBackground,
                BorderStyle = BorderStyle.FixedSingle,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            _gridValues.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", Name = "ParameterId", Visible = false });
            _gridValues.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Parameter", Name = "ParameterName", FillWeight = 150 });
            _gridValues.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Bedeutung", Name = "Description", FillWeight = 260, ReadOnly = true });
            _gridValues.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Wert", Name = "Value", FillWeight = 120 });
            _gridValues.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Einheit", Name = "Unit", FillWeight = 70, ReadOnly = true });
            _gridValues.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Beispiel", Name = "Example", Visible = false });
            _gridValues.SelectionChanged += gridValues_SelectionChanged;
            Controls.Add(_gridValues);

            _butHelp = new DarkButton
            {
                Text = "Hilfe einblenden",
                Location = new Point(10, 418),
                Size = new Size(125, 25)
            };
            _butHelp.Click += butHelp_Click;
            Controls.Add(_butHelp);

            _labelHelp = new DarkLabel
            {
                AutoSize = false,
                Location = new Point(145, 418),
                Size = new Size(425, 50),
                AutoUpdateHeight = true,
                Visible = false
            };
            Controls.Add(_labelHelp);

            var butOk = new DarkButton
            {
                Text = "OK",
                Location = new Point(580, 455),
                Size = new Size(80, 25)
            };
            butOk.Click += butOk_Click;
            Controls.Add(butOk);

            var butCancel = new DarkButton
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(670, 455),
                Size = new Size(80, 25)
            };
            Controls.Add(butCancel);

            AcceptButton = butOk;
            CancelButton = butCancel;
        }

        private string GetObjectCaption()
        {
            ObjectParameterObjectKey key = _context.ObjectKey ?? new ObjectParameterObjectKey();
            string slot = key.SlotId.HasValue ? "Slot " + key.SlotId.Value : "No slot";
            string scriptId = key.ScriptId.HasValue ? "Script ID " + key.ScriptId.Value : "No script ID";
            string room = key.RoomIndex.HasValue ? "Room " + key.RoomIndex.Value : "No room";
            return "Object: " + _context.ObjectTypeId + "   |   " + slot + "   |   " + scriptId + "   |   " + room;
        }

        private static DarkLabel CreateLabel(string text, int x, int y)
        {
            return new DarkLabel
            {
                Text = text,
                Location = new Point(x, y + 4),
                Size = new Size(90, 20)
            };
        }

        private static DarkTextBox CreateTextBox(int x, int y, int width)
        {
            return new DarkTextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 22)
            };
        }

        private void LoadDefinitions()
        {
            _isLoading = true;
            _comboDefinitions.Items.Clear();
            _comboDefinitions.Items.Add(new DefinitionItem(null, "Custom parameters"));

            foreach (ObjectParameterDefinitionSet definitionSet in ObjectParameterProviderRegistry.GetDefinitionSets(_context))
                _comboDefinitions.Items.Add(new DefinitionItem(definitionSet, definitionSet.Name));

            _comboDefinitions.SelectedIndex = 0;
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

                _textProviderId.Text = valueSet.ProviderId;
                _textDefinitionSetId.Text = valueSet.DefinitionSetId;
                _textPresetId.Text = valueSet.PresetId;

                foreach (ObjectParameterValue value in valueSet.Values)
                    AddOrUpdateValueRow(value.ParameterId, value.Value, FindParameterDefinition(value.ParameterId));

                SelectPreset(valueSet.PresetId);
            }

            _isLoading = false;
        }

        private void LoadPresets(ObjectParameterDefinitionSet definitionSet)
        {
            _comboPresets.Items.Clear();
            _comboPresets.Items.Add(new PresetItem(null, "No preset"));

            if (definitionSet != null)
                foreach (ObjectParameterPreset preset in definitionSet.Presets)
                    _comboPresets.Items.Add(new PresetItem(preset, preset.Name));

            _comboPresets.SelectedIndex = 0;
        }

        private void SelectDefinition(string providerId, string definitionSetId)
        {
            for (int i = 0; i < _comboDefinitions.Items.Count; i++)
            {
                var item = _comboDefinitions.Items[i] as DefinitionItem;
                if (item?.DefinitionSet == null)
                    continue;

                if (string.Equals(item.DefinitionSet.ProviderId, providerId, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(item.DefinitionSet.Id, definitionSetId, StringComparison.OrdinalIgnoreCase))
                {
                    _comboDefinitions.SelectedIndex = i;
                    LoadPresets(item.DefinitionSet);
                    return;
                }
            }

            _comboDefinitions.SelectedIndex = 0;
            LoadPresets(null);
        }

        private void SelectPreset(string presetId)
        {
            if (string.IsNullOrEmpty(presetId))
            {
                _comboPresets.SelectedIndex = 0;
                return;
            }

            for (int i = 0; i < _comboPresets.Items.Count; i++)
            {
                var item = _comboPresets.Items[i] as PresetItem;
                if (item?.Preset != null && string.Equals(item.Preset.Id, presetId, StringComparison.OrdinalIgnoreCase))
                {
                    _comboPresets.SelectedIndex = i;
                    return;
                }
            }
        }

        private ObjectParameterDefinitionSet SelectedDefinitionSet
        {
            get
            {
                var item = _comboDefinitions.SelectedItem as DefinitionItem;
                return item?.DefinitionSet;
            }
        }

        private void comboDefinitions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            ObjectParameterDefinitionSet definitionSet = SelectedDefinitionSet;

            _textProviderId.Text = definitionSet?.ProviderId ?? string.Empty;
            _textDefinitionSetId.Text = definitionSet?.Id ?? string.Empty;
            LoadPresets(definitionSet);

            if (definitionSet != null && GetValueRowCount() == 0)
                LoadDefaultValues(definitionSet);

            UpdateHelpPanel();
        }

        private void comboPresets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            var item = _comboPresets.SelectedItem as PresetItem;
            ObjectParameterPreset preset = item?.Preset;
            _textPresetId.Text = preset?.Id ?? string.Empty;

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
            _butHelp.Text = _showHelp ? "Hilfe ausblenden" : "Hilfe einblenden";
            UpdateHelpPanel();
        }

        private int GetValueRowCount()
        {
            int count = 0;
            foreach (DataGridViewRow row in _gridValues.Rows)
                if (!row.IsNewRow)
                    count++;
            return count;
        }

        private void LoadDefaultValues(ObjectParameterDefinitionSet definitionSet)
        {
            _gridValues.Rows.Clear();

            foreach (ObjectParameterGroup group in definitionSet.Groups)
                foreach (ObjectParameterDefinition parameter in group.Parameters)
                    AddOrUpdateValueRow(parameter.Id, parameter.DefaultValue, parameter);
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

        private void AddOrUpdateValueRow(string parameterId, string value, ObjectParameterDefinition parameter)
        {
            string name = parameter?.Name ?? parameterId;
            string description = parameter?.Description ?? string.Empty;
            string unit = parameter?.Unit ?? string.Empty;
            string example = parameter?.Example ?? string.Empty;

            foreach (DataGridViewRow row in _gridValues.Rows)
            {
                if (row.IsNewRow)
                    continue;

                string currentId = Convert.ToString(row.Cells["ParameterId"].Value)?.Trim() ?? string.Empty;
                if (string.Equals(currentId, parameterId, StringComparison.OrdinalIgnoreCase))
                {
                    row.Cells["ParameterName"].Value = name;
                    row.Cells["Description"].Value = description;
                    row.Cells["Value"].Value = value;
                    row.Cells["Unit"].Value = unit;
                    row.Cells["Example"].Value = example;
                    return;
                }
            }

            _gridValues.Rows.Add(parameterId, name, description, value, unit, example);
        }

        private void ApplyPreset(ObjectParameterPreset preset)
        {
            foreach (ObjectParameterValue value in preset.Values)
                AddOrUpdateValueRow(value.ParameterId, value.Value, FindParameterDefinition(value.ParameterId));
        }

        private void UpdateHelpPanel()
        {
            _labelHelp.Visible = _showHelp;
            if (!_showHelp)
                return;

            DataGridViewRow row = _gridValues.CurrentRow;
            if (row == null || row.IsNewRow)
            {
                _labelHelp.Text = "Select a parameter to show help.";
                return;
            }

            string description = Convert.ToString(row.Cells["Description"].Value) ?? string.Empty;
            string example = Convert.ToString(row.Cells["Example"].Value) ?? string.Empty;

            if (string.IsNullOrWhiteSpace(description) && string.IsNullOrWhiteSpace(example))
                _labelHelp.Text = "No additional help for this parameter.";
            else if (string.IsNullOrWhiteSpace(example))
                _labelHelp.Text = description;
            else if (string.IsNullOrWhiteSpace(description))
                _labelHelp.Text = "Example: " + example;
            else
                _labelHelp.Text = description + "  Example: " + example;
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            ObjectParameterValueSet valueSet = ObjectParameterStorage.GetOrCreate(_instance);
            valueSet.ProviderId = _textProviderId.Text.Trim();
            valueSet.DefinitionSetId = _textDefinitionSetId.Text.Trim();
            valueSet.PresetId = _textPresetId.Text.Trim();
            valueSet.Values.Clear();

            foreach (DataGridViewRow row in _gridValues.Rows)
            {
                if (row.IsNewRow)
                    continue;

                string parameterId = Convert.ToString(row.Cells["ParameterId"].Value)?.Trim() ?? string.Empty;
                string value = Convert.ToString(row.Cells["Value"].Value)?.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(parameterId) && string.IsNullOrEmpty(value))
                    continue;

                valueSet.Values.Add(new ObjectParameterValue
                {
                    ParameterId = parameterId,
                    Value = value
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
