namespace TombEditor.Forms
{
    partial class FormObjectOcbCodes
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
            darkGroupBox1 = new DarkUI.Controls.DarkGroupBox();
            gridOcbCodes = new System.Windows.Forms.DataGridView();
            butUseSelected = new DarkUI.Controls.DarkButton();
            butClose = new DarkUI.Controls.DarkButton();
            darkGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridOcbCodes).BeginInit();
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
            // darkGroupBox1
            // 
            darkGroupBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            darkGroupBox1.Controls.Add(gridOcbCodes);
            darkGroupBox1.Location = new System.Drawing.Point(7, 35);
            darkGroupBox1.Name = "darkGroupBox1";
            darkGroupBox1.Size = new System.Drawing.Size(746, 347);
            darkGroupBox1.TabIndex = 1;
            darkGroupBox1.TabStop = false;
            darkGroupBox1.Text = "Existing OCB codes for this object";
            // 
            // gridOcbCodes
            // 
            gridOcbCodes.AllowUserToAddRows = false;
            gridOcbCodes.AllowUserToDeleteRows = false;
            gridOcbCodes.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gridOcbCodes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            gridOcbCodes.BackgroundColor = System.Drawing.Color.FromArgb(43, 43, 43);
            gridOcbCodes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            gridOcbCodes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridOcbCodes.Location = new System.Drawing.Point(7, 19);
            gridOcbCodes.MultiSelect = false;
            gridOcbCodes.Name = "gridOcbCodes";
            gridOcbCodes.RowHeadersVisible = false;
            gridOcbCodes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            gridOcbCodes.Size = new System.Drawing.Size(733, 322);
            gridOcbCodes.TabIndex = 0;
            // 
            // butUseSelected
            // 
            butUseSelected.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butUseSelected.Checked = false;
            butUseSelected.Location = new System.Drawing.Point(563, 391);
            butUseSelected.Name = "butUseSelected";
            butUseSelected.Size = new System.Drawing.Size(104, 23);
            butUseSelected.TabIndex = 2;
            butUseSelected.Text = "Use selected";
            butUseSelected.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            butUseSelected.Click += butUseSelected_Click;
            // 
            // butClose
            // 
            butClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butClose.Checked = false;
            butClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            butClose.Location = new System.Drawing.Point(673, 391);
            butClose.Name = "butClose";
            butClose.Size = new System.Drawing.Size(80, 23);
            butClose.TabIndex = 3;
            butClose.Text = "Cancel";
            butClose.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            butClose.Click += butClose_Click;
            // 
            // FormObjectOcbCodes
            // 
            AcceptButton = butUseSelected;
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = butClose;
            ClientSize = new System.Drawing.Size(760, 421);
            Controls.Add(butUseSelected);
            Controls.Add(butClose);
            Controls.Add(darkGroupBox1);
            Controls.Add(labelObject);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(680, 360);
            Name = "FormObjectOcbCodes";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Existing OCB Codes";
            darkGroupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridOcbCodes).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private DarkUI.Controls.DarkLabel labelObject;
        private DarkUI.Controls.DarkGroupBox darkGroupBox1;
        private System.Windows.Forms.DataGridView gridOcbCodes;
        private DarkUI.Controls.DarkButton butUseSelected;
        private DarkUI.Controls.DarkButton butClose;
    }
}
