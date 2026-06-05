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

        private DarkComboBox _comboDefinitions;
        private DarkComboBox _comboPresets;
        private DarkTextBox _textProviderId;
        private DarkTextBox _textDefinitionSetId;
        private DarkTextBox _textPresetId;
        private DataGridView _gridValues;

        public FormObjectParameters(Level level, ObjectInstance instance)
        {
            _level = level;
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
            _context = ObjectParameterContextFactory.FromObject(_level, _instance);

            InitializeComponent();
            LoadDefinitions();
            LoadObjectParameters();
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
            ClientSize = new Size(760, 540);

            var labelInfo = new DarkLabel
            {
                AutoSize = false,
                Text = "Object-specific parameters are stored on this object instance. External systems can register providers to expose object-specific settings here.",
                Location = new Point(10, 10),
                Size = new Size(740, 40),
                AutoUpdateHeight = true
            };
            Controls.Add(labelInfo);

            Controls.Add(CreateLabel("Definition", 10, 64));
            _comboDefinitions = new DarkComboBox
            {
                Location = new Point(120, 60),
                Size = new Size(630, 22)
            };
            _comboDefinitions.SelectedIndexChanged += comboDefinitions_SelectedIndexChanged;
            Controls.Add(_comboDefinitions);

            Controls.Add(CreateLabel("Provider", 10, 94));
            _textProviderId = CreateTextBox(120, 90, 630);
            _textProviderId.ReadOnly = true;
            Controls.Add(_textProviderId);

            Controls.Add(CreateLabel("Definition ID", 10, 124));
            _textDefinitionSetId = CreateTextBox(120, 120, 630);
            Controls.Add(_textDefinitionSetId);

            Controls.Add(CreateLabel("Preset", 10, 154));
            _comboPresets = new DarkComboBox
            {
                Location = new Point(120, 150),
                Size = new Size(390, 22)
            };
            _comboPresets.SelectedIndexChanged += comboPresets_SelectedIndexChanged;
            Controls.Add(_comboPresets);

            _textPresetId = CreateTextBox(520, 150, 230);
            Controls.Add(_textPresetId);

            _gridValues = new DataGridView
            {
                Location = new Point(10, 190),
                Size = new Size(740, 290),
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Colors.DarkBackground,
                BorderStyle = BorderStyle.FixedSingle,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                RowHeadersVisible = false
            };
            _gridValues.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Parameter", Name = "ParameterId" });
            _gridValues.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Value", Name = "Value" });
            Controls.Add(_gridValues);

            var butOk = new DarkButton
            {
                Text = "OK",
                Location = new Point(580, 500),
                Size = new Size(80, 25)
            };
            butOk.Click += butOk_Click;
            Controls.Add(butOk);

            var butCancel = new DarkButton
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(670, 500),
                Size = new Size(80, 25)
            };
            Controls.Add(butCancel);

            AcceptButton = butOk;
            CancelButton = butCancel;
        }

        private static DarkLabel CreateLabel(string text, int x, int y)
        {
            return new DarkLabel
            {
                Text = text,
                Location = new Point(x, y + 4),
                Size = new Size(100, 20)
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
                    _gridValues.Rows.Add(value.ParameterId, value.Value);

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

        private void comboDefinitions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            var item = _comboDefinitions.SelectedItem as DefinitionItem;
            ObjectParameterDefinitionSet definitionSet = item?.DefinitionSet;

            _textProviderId.Text = definitionSet?.ProviderId ?? string.Empty;
            _textDefinitionSetId.Text = definitionSet?.Id ?? string.Empty;
            LoadPresets(definitionSet);

            if (definitionSet != null && GetValueRowCount() == 0)
                LoadDefaultValues(definitionSet);
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
                    _gridValues.Rows.Add(parameter.Id, parameter.DefaultValue);
        }

        private void ApplyPreset(ObjectParameterPreset preset)
        {
            foreach (ObjectParameterValue value in preset.Values)
                SetGridValue(value.ParameterId, value.Value);
        }

        private void SetGridValue(string parameterId, string value)
        {
            foreach (DataGridViewRow row in _gridValues.Rows)
            {
                if (row.IsNewRow)
                    continue;

                string currentId = Convert.ToString(row.Cells["ParameterId"].Value)?.Trim() ?? string.Empty;
                if (string.Equals(currentId, parameterId, StringComparison.OrdinalIgnoreCase))
                {
                    row.Cells["Value"].Value = value;
                    return;
                }
            }

            _gridValues.Rows.Add(parameterId, value);
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
