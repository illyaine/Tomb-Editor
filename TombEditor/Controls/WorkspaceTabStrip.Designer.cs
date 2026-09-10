using DarkUI.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace TombEditor.Controls
{
    partial class WorkspaceTabStrip
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel layoutTabs;
        private DarkButton butGeometry;
        private DarkButton butTerrain;
        private DarkButton butTextures;
        private DarkButton butObjects;
        private DarkButton butLighting;
        private DarkButton butGameplay;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            layoutTabs = new TableLayoutPanel();
            butGeometry = CreateWorkspaceButton("Geometry");
            butTerrain = CreateWorkspaceButton("Terrain");
            butTextures = CreateWorkspaceButton("Textures");
            butObjects = CreateWorkspaceButton("Objects");
            butLighting = CreateWorkspaceButton("Lighting");
            butGameplay = CreateWorkspaceButton("Gameplay");
            layoutTabs.SuspendLayout();
            SuspendLayout();
            // 
            // layoutTabs
            // 
            layoutTabs.ColumnCount = 6;
            layoutTabs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66667f));
            layoutTabs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66667f));
            layoutTabs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66667f));
            layoutTabs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66667f));
            layoutTabs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66667f));
            layoutTabs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66665f));
            layoutTabs.Controls.Add(butGeometry, 0, 0);
            layoutTabs.Controls.Add(butTerrain, 1, 0);
            layoutTabs.Controls.Add(butTextures, 2, 0);
            layoutTabs.Controls.Add(butObjects, 3, 0);
            layoutTabs.Controls.Add(butLighting, 4, 0);
            layoutTabs.Controls.Add(butGameplay, 5, 0);
            layoutTabs.Dock = DockStyle.Fill;
            layoutTabs.Location = new Point(4, 4);
            layoutTabs.Margin = new Padding(0);
            layoutTabs.Name = "layoutTabs";
            layoutTabs.RowCount = 1;
            layoutTabs.RowStyles.Add(new RowStyle(SizeType.Percent, 100.0f));
            layoutTabs.Size = new Size(892, 30);
            layoutTabs.TabIndex = 0;
            // 
            // WorkspaceTabStrip
            // 
            BackColor = Color.FromArgb(45, 45, 48);
            Controls.Add(layoutTabs);
            Name = "WorkspaceTabStrip";
            Padding = new Padding(4);
            Size = new Size(900, 38);
            layoutTabs.ResumeLayout(false);
            ResumeLayout(false);
        }

        private static DarkButton CreateWorkspaceButton(string text)
        {
            return new DarkButton
            {
                BackColorUseGeneric = false,
                Dock = DockStyle.Fill,
                ForeColor = Color.WhiteSmoke,
                Margin = new Padding(2, 0, 2, 0),
                Selectable = false,
                Text = text,
                UseForeColor = true
            };
        }
    }
}
