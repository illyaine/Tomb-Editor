namespace TombEditor.Forms
{
    partial class FormObjectParameters
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            labelObject = new DarkUI.Controls.DarkLabel();
            darkLabel1 = new DarkUI.Controls.DarkLabel();
            comboDefinitions = new DarkUI.Controls.DarkComboBox();
            darkLabel2 = new DarkUI.Controls.DarkLabel();
            comboPresets = new DarkUI.Controls.DarkComboBox();
            darkLabel3 = new DarkUI.Controls.DarkLabel();
            textProviderId = new DarkUI.Controls.DarkTextBox();
            darkLabel4 = new DarkUI.Controls.DarkLabel();
            textDefinitionSetId = new DarkUI.Controls.DarkTextBox();
            darkLabel5 = new DarkUI.Controls.DarkLabel();
            textPresetId = new DarkUI.Controls.DarkTextBox();
            darkGroupBox1 = new DarkUI.Controls.DarkGroupBox();
            gridValues = new System.Windows.Forms.DataGridView();
            butOcbCodes = new DarkUI.Controls.DarkButton();
            butHelp = new DarkUI.Controls.DarkButton();
            labelHelp = new DarkUI.Controls.DarkLabel();
            butCancel = new DarkUI.Controls.DarkButton();
            butOk = new DarkUI.Controls.DarkButton();
            darkGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridValues).BeginInit();
            SuspendLayout();
            // 
            // labelObject
            // 
            labelObject.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            labelObject.AutoSize = false;
            labelObject.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            labelObject.Location = new System.Drawing.Point(7, 8);
            labelObject.Name = "labelObject";
            labelObject.Size = new System.Drawing.Size(746, 20);
            labelObject.TabIndex = 0;
            // 
            // darkLabel1
            // 
            darkLabel1.AutoSize = true;
            darkLabel1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel1.Location = new System.Drawing.Point(7, 39);
            darkLabel1.Name = "darkLabel1";
            darkLabel1.Size = new System.Drawing.Size(56, 13);
            darkLabel1.TabIndex = 1;
            darkLabel1.Text = "Definition:";
            // 
            // comboDefinitions
            // 
            comboDefinitions.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            comboDefinitions.FormattingEnabled = true;
            comboDefinitions.Location = new System.Drawing.Point(83, 35);
            comboDefinitions.Name = "comboDefinitions";
            comboDefinitions.Size = new System.Drawing.Size(432, 23);
            comboDefinitions.TabIndex = 2;
            comboDefinitions.SelectedIndexChanged += comboDefinitions_SelectedIndexChanged;
            // 
            // darkLabel2
            // 
            darkLabel2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            darkLabel2.AutoSize = true;
            darkLabel2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel2.Location = new System.Drawing.Point(522, 39);
            darkLabel2.Name = "darkLabel2";
            darkLabel2.Size = new System.Drawing.Size(42, 13);
            darkLabel2.TabIndex = 3;
            darkLabel2.Text = "Preset:";
            // 
            // comboPresets
            // 
            comboPresets.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            comboPresets.FormattingEnabled = true;
            comboPresets.Location = new System.Drawing.Point(570, 35);
            comboPresets.Name = "comboPresets";
            comboPresets.Size = new System.Drawing.Size(183, 23);
            comboPresets.TabIndex = 4;
            comboPresets.SelectedIndexChanged += comboPresets_SelectedIndexChanged;
            // 
            // darkLabel3
            // 
            darkLabel3.AutoSize = true;
            darkLabel3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel3.Location = new System.Drawing.Point(7, 67);
            darkLabel3.Name = "darkLabel3";
            darkLabel3.Size = new System.Drawing.Size(52, 13);
            darkLabel3.TabIndex = 5;
            darkLabel3.Text = "Provider:";
            // 
            // textProviderId
            // 
            textProviderId.Location = new System.Drawing.Point(83, 63);
            textProviderId.Name = "textProviderId";
            textProviderId.ReadOnly = true;
            textProviderId.Size = new System.Drawing.Size(180, 20);
            textProviderId.TabIndex = 6;
            // 
            // darkLabel4
            // 
            darkLabel4.AutoSize = true;
            darkLabel4.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel4.Location = new System.Drawing.Point(274, 67);
            darkLabel4.Name = "darkLabel4";
            darkLabel4.Size = new System.Drawing.Size(71, 13);
            darkLabel4.TabIndex = 7;
            darkLabel4.Text = "Definition ID:";
            // 
            // textDefinitionSetId
            // 
            textDefinitionSetId.Location = new System.Drawing.Point(351, 63);
            textDefinitionSetId.Name = "textDefinitionSetId";
            textDefinitionSetId.Size = new System.Drawing.Size(164, 20);
            textDefinitionSetId.TabIndex = 8;
            // 
            // darkLabel5
            // 
            darkLabel5.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            darkLabel5.AutoSize = true;
            darkLabel5.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel5.Location = new System.Drawing.Point(522, 67);
            darkLabel5.Name = "darkLabel5";
            darkLabel5.Size = new System.Drawing.Size(53, 13);
            darkLabel5.TabIndex = 9;
            darkLabel5.Text = "Preset ID:";
            // 
            // textPresetId
            // 
            textPresetId.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            textPresetId.Location = new System.Drawing.Point(581, 63);
            textPresetId.Name = "textPresetId";
            textPresetId.Size = new System.Drawing.Size(172, 20);
            textPresetId.TabIndex = 10;
            // 
            // darkGroupBox1
            // 
            darkGroupBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            darkGroupBox1.Controls.Add(gridValues);
            darkGroupBox1.Location = new System.Drawing.Point(7, 94);
            darkGroupBox1.Name = "darkGroupBox1";
            darkGroupBox1.Size = new System.Drawing.Size(746, 323);
            darkGroupBox1.TabIndex = 11;
            darkGroupBox1.TabStop = false;
            darkGroupBox1.Text = "Object codes && parameters";
            // 
            // gridValues
            // 
            gridValues.AllowUserToAddRows = true;
            gridValues.AllowUserToDeleteRows = true;
            gridValues.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gridValues.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            gridValues.BackgroundColor = System.Drawing.Color.FromArgb(43, 43, 43);
            gridValues.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            gridValues.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridValues.Location = new System.Drawing.Point(7, 19);
            gridValues.MultiSelect = false;
            gridValues.Name = "gridValues";
            gridValues.RowHeadersVisible = false;
            gridValues.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            gridValues.Size = new System.Drawing.Size(733, 298);
            gridValues.TabIndex = 0;
            gridValues.SelectionChanged += gridValues_SelectionChanged;
            // 
            // butOcbCodes
            // 
            butOcbCodes.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            butOcbCodes.Checked = false;
            butOcbCodes.Location = new System.Drawing.Point(7, 426);
            butOcbCodes.Name = "butOcbCodes";
            butOcbCodes.Size = new System.Drawing.Size(152, 23);
            butOcbCodes.TabIndex = 12;
            butOcbCodes.Text = "Show existing OCB codes";
            butOcbCodes.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            butOcbCodes.Click += butShowOcbCodes_Click;
            // 
            // butHelp
            // 
            butHelp.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            butHelp.Checked = false;
            butHelp.Location = new System.Drawing.Point(165, 426);
            butHelp.Name = "butHelp";
            butHelp.Size = new System.Drawing.Size(96, 23);
            butHelp.TabIndex = 13;
            butHelp.Text = "Show help";
            butHelp.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            butHelp.Click += butHelp_Click;
            // 
            // labelHelp
            // 
            labelHelp.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            labelHelp.AutoSize = false;
            labelHelp.AutoUpdateHeight = true;
            labelHelp.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            labelHelp.Location = new System.Drawing.Point(267, 429);
            labelHelp.Name = "labelHelp";
            labelHelp.Size = new System.Drawing.Size(282, 36);
            labelHelp.TabIndex = 14;
            labelHelp.Visible = false;
            // 
            // butCancel
            // 
            butCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butCancel.Checked = false;
            butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            butCancel.Location = new System.Drawing.Point(673, 456);
            butCancel.Name = "butCancel";
            butCancel.Size = new System.Drawing.Size(80, 23);
            butCancel.TabIndex = 16;
            butCancel.Text = "Cancel";
            butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            butCancel.Click += butCancel_Click;
            // 
            // butOk
            // 
            butOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butOk.Checked = false;
            butOk.Location = new System.Drawing.Point(587, 456);
            butOk.Name = "butOk";
            butOk.Size = new System.Drawing.Size(80, 23);
            butOk.TabIndex = 15;
            butOk.Text = "OK";
            butOk.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            butOk.Click += butOk_Click;
            // 
            // FormObjectParameters
            // 
            AcceptButton = butOk;
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = butCancel;
            ClientSize = new System.Drawing.Size(760, 487);
            Controls.Add(butCancel);
            Controls.Add(butOk);
            Controls.Add(labelHelp);
            Controls.Add(butHelp);
            Controls.Add(butOcbCodes);
            Controls.Add(darkGroupBox1);
            Controls.Add(textPresetId);
            Controls.Add(darkLabel5);
            Controls.Add(textDefinitionSetId);
            Controls.Add(darkLabel4);
            Controls.Add(textProviderId);
            Controls.Add(darkLabel3);
            Controls.Add(comboPresets);
            Controls.Add(darkLabel2);
            Controls.Add(comboDefinitions);
            Controls.Add(darkLabel1);
            Controls.Add(labelObject);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormObjectParameters";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Object Parameters";
            darkGroupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridValues).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private DarkUI.Controls.DarkLabel labelObject;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkComboBox comboDefinitions;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkComboBox comboPresets;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkTextBox textProviderId;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkTextBox textDefinitionSetId;
        private DarkUI.Controls.DarkLabel darkLabel5;
        private DarkUI.Controls.DarkTextBox textPresetId;
        private DarkUI.Controls.DarkGroupBox darkGroupBox1;
        private System.Windows.Forms.DataGridView gridValues;
        private DarkUI.Controls.DarkButton butOcbCodes;
        private DarkUI.Controls.DarkButton butHelp;
        private DarkUI.Controls.DarkLabel labelHelp;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butOk;
    }
}
