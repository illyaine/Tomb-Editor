using DarkUI.Config;
using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.LevelData.ObjectParameters;

namespace TombEditor.Forms
{
    public class FormObjectParameters : DarkForm
    {
        private readonly ObjectInstance _instance;

        private DarkTextBox _textProviderId;
        private DarkTextBox _textDefinitionSetId;
        private DarkTextBox _textPresetId;
        private DataGridView _gridValues;

        public FormObjectParameters(ObjectInstance instance)
        {
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));

            InitializeComponent();
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
            ClientSize = new Size(680, 460);

            var labelInfo = new DarkLabel
            {
                AutoSize = false,
                Text = "Object-specific parameters are stored on this object instance and can later be exported as runtime metadata, Flow/Lua data or legacy OCB fallback.",
                Location = new Point(10, 10),
                Size = new Size(660, 40),
                AutoUpdateHeight = true
            };
            Controls.Add(labelInfo);

            Controls.Add(CreateLabel("Provider", 10, 64));
            _textProviderId = CreateTextBox(120, 60, 540);
            Controls.Add(_textProviderId);

            Controls.Add(CreateLabel("Definition", 10, 94));
            _textDefinitionSetId = CreateTextBox(120, 90, 540);
            Controls.Add(_textDefinitionSetId);

            Controls.Add(CreateLabel("Preset", 10, 124));
            _textPresetId = CreateTextBox(120, 120, 540);
            Controls.Add(_textPresetId);

            _gridValues = new DataGridView
            {
                Location = new Point(10, 160),
                Size = new Size(660, 240),
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
                DialogResult = DialogResult.OK,
                Location = new Point(500, 420),
                Size = new Size(80, 25)
            };
            butOk.Click += butOk_Click;
            Controls.Add(butOk);

            var butCancel = new DarkButton
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(590, 420),
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

        private void LoadObjectParameters()
        {
            ObjectParameterValueSet valueSet = ObjectParameterStorage.TryGet(_instance);
            if (valueSet == null)
                return;

            _textProviderId.Text = valueSet.ProviderId;
            _textDefinitionSetId.Text = valueSet.DefinitionSetId;
            _textPresetId.Text = valueSet.PresetId;

            foreach (ObjectParameterValue value in valueSet.Values)
                _gridValues.Rows.Add(value.ParameterId, value.Value);
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

            ObjectParameterStorage.Set(_instance, valueSet);
        }
    }
}
