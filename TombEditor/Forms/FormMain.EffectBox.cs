using System;
using System.Windows.Forms;
using TombLib.Forms;
using TombLib.LevelData;

namespace TombEditor.Forms
{
    public partial class FormMain
    {
        private ToolStripMenuItem _addEffectBoxToolStripMenuItem;
        private bool _effectBoxMenuInitialized;

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            if (_effectBoxMenuInitialized)
                return;

            _effectBoxMenuInitialized = true;
            _addEffectBoxToolStripMenuItem = new ToolStripMenuItem
            {
                Name = "addEffectBoxToolStripMenuItem",
                Text = "Add effect box",
                Image = EffectBoxIcon.Image16
            };
            _addEffectBoxToolStripMenuItem.Click += AddEffectBoxToolStripMenuItem_Click;

            int insertIndex = itemsToolStripMenuItem.DropDownItems.IndexOf(addBoxVolumeToolStripMenuItem);
            if (insertIndex >= 0)
                insertIndex++;
            else
                insertIndex = itemsToolStripMenuItem.DropDownItems.Count;

            itemsToolStripMenuItem.DropDownItems.Insert(insertIndex, _addEffectBoxToolStripMenuItem);
            UpdateEffectBoxMenuState();

            _editor.EditorEventRaised += EffectBoxMenu_EditorEventRaised;
            Disposed += FormMainEffectBox_Disposed;
        }

        private void AddEffectBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_editor.Level == null || !_editor.Level.IsTombEngine)
            {
                _editor.SendMessage("Effect boxes are available only for Tomb Engine levels.", PopupType.Warning);
                return;
            }

            _editor.Action = new EditorActionPlace(false,
                (level, room) => EffectBoxUtils.Create(level.Settings));
        }

        private void EffectBoxMenu_EditorEventRaised(IEditorEvent editorEvent)
        {
            if (editorEvent is Editor.InitEvent ||
                editorEvent is Editor.LevelChangedEvent ||
                editorEvent is Editor.GameVersionChangedEvent)
                UpdateEffectBoxMenuState();
        }

        private void UpdateEffectBoxMenuState()
        {
            if (_addEffectBoxToolStripMenuItem != null)
                _addEffectBoxToolStripMenuItem.Visible = _editor.Level != null && _editor.Level.IsTombEngine;
        }

        private void FormMainEffectBox_Disposed(object sender, EventArgs e)
        {
            _editor.EditorEventRaised -= EffectBoxMenu_EditorEventRaised;
            Disposed -= FormMainEffectBox_Disposed;

            if (_addEffectBoxToolStripMenuItem != null)
            {
                _addEffectBoxToolStripMenuItem.Click -= AddEffectBoxToolStripMenuItem_Click;
                _addEffectBoxToolStripMenuItem.Dispose();
                _addEffectBoxToolStripMenuItem = null;
            }
        }
    }
}
