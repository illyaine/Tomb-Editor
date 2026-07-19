using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TombLib.LevelData;

namespace TombEditor.ToolWindows
{
    public partial class MainView
    {
        private ToolStripButton _butAddEffectBox;
        private bool _effectBoxToolbarInitialized;

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            if (_effectBoxToolbarInitialized)
                return;

            _effectBoxToolbarInitialized = true;
            _butAddEffectBox = new ToolStripButton
            {
                Name = "butAddEffectBox",
                BackColor = Color.FromArgb(60, 63, 65),
                ForeColor = Color.FromArgb(220, 220, 220),
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                Image = EffectBoxIcon.Image16,
                ImageTransparentColor = Color.Magenta,
                Size = new Size(23, 29),
                ToolTipText = "Add effect box"
            };
            _butAddEffectBox.Click += ButAddEffectBox_Click;

            if (!_toolstripButtons.Contains(_butAddEffectBox))
                _toolstripButtons.Add(_butAddEffectBox);

            EnsureEffectBoxToolbarButton();
            UpdateEffectBoxToolbarState();
            panel3D.InitializeEffectBoxOverlay();

            _editor.EditorEventRaised += EffectBoxToolbar_EditorEventRaised;
            Disposed += MainViewEffectBox_Disposed;
        }

        private void ButAddEffectBox_Click(object sender, EventArgs e)
        {
            if (!_editor.Level.IsTombEngine)
            {
                _editor.SendMessage("Effect boxes are available only for Tomb Engine levels.", PopupType.Warning);
                return;
            }

            _editor.Action = new EditorActionPlace(false,
                (location, room) => EffectBoxUtils.Create(_editor.Level.Settings));
        }

        private void EffectBoxToolbar_EditorEventRaised(IEditorEvent editorEvent)
        {
            if (editorEvent is Editor.ConfigurationChangedEvent)
                EnsureEffectBoxToolbarButton();

            if (editorEvent is Editor.InitEvent ||
                editorEvent is Editor.LevelChangedEvent ||
                editorEvent is Editor.GameVersionChangedEvent)
            {
                EnsureEffectBoxToolbarButton();
                UpdateEffectBoxToolbarState();
            }
        }

        private void EnsureEffectBoxToolbarButton()
        {
            if (_butAddEffectBox == null || toolStrip.Items.Contains(_butAddEffectBox))
                return;

            int insertIndex = toolStrip.Items.IndexOf(butAddBoxVolume);
            if (insertIndex >= 0)
                insertIndex++;
            else
            {
                insertIndex = toolStrip.Items.IndexOf(butAddSphereVolume);
                if (insertIndex < 0)
                    insertIndex = toolStrip.Items.Count;
            }

            toolStrip.Items.Insert(insertIndex, _butAddEffectBox);
            toolStrip.Invalidate();
        }

        private void UpdateEffectBoxToolbarState()
        {
            if (_butAddEffectBox != null)
                _butAddEffectBox.Enabled = _editor.Level != null && _editor.Level.IsTombEngine;
        }

        private void MainViewEffectBox_Disposed(object sender, EventArgs e)
        {
            _editor.EditorEventRaised -= EffectBoxToolbar_EditorEventRaised;
            Disposed -= MainViewEffectBox_Disposed;

            if (_butAddEffectBox != null)
            {
                _butAddEffectBox.Click -= ButAddEffectBox_Click;
                _butAddEffectBox.Dispose();
                _butAddEffectBox = null;
            }
        }
    }
}
