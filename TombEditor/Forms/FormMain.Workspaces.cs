using System;
using System.Drawing;
using System.Windows.Forms;
using TombEditor.Controls;

namespace TombEditor.Forms
{
    public partial class FormMain
    {
        private const int WorkspaceStripHeight = 38;

        private WorkspaceTabStrip _workspaceTabs;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            InitializeWorkspaceTabs();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (_workspaceTabs != null)
            {
                _editor.EditorEventRaised -= WorkspaceEditorEventRaised;
                _workspaceTabs.SelectedWorkspaceChanged -= WorkspaceTabs_SelectedWorkspaceChanged;
            }

            base.OnFormClosed(e);
        }

        private void InitializeWorkspaceTabs()
        {
            if (_workspaceTabs != null)
                return;

            _workspaceTabs = new WorkspaceTabStrip
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Location = Point.Empty,
                Size = new Size(panelDockArea.ClientSize.Width, WorkspaceStripHeight)
            };

            panelDockArea.Padding = new Padding(0, WorkspaceStripHeight, 0, 0);
            panelDockArea.Controls.Add(_workspaceTabs);
            _workspaceTabs.BringToFront();
            _workspaceTabs.SelectedWorkspaceChanged += WorkspaceTabs_SelectedWorkspaceChanged;
            _workspaceTabs.SyncEditorMode(_editor.Mode);
            _editor.EditorEventRaised += WorkspaceEditorEventRaised;
        }

        private void WorkspaceTabs_SelectedWorkspaceChanged(object sender, EditorWorkspaceChangedEventArgs e)
        {
            switch (e.Workspace)
            {
                case EditorWorkspace.Textures:
                    _editor.Mode = EditorMode.FaceEdit;
                    break;

                case EditorWorkspace.Lighting:
                    _editor.Mode = EditorMode.Lighting;
                    break;

                default:
                    _editor.Mode = EditorMode.Geometry;
                    break;
            }
        }

        private void WorkspaceEditorEventRaised(IEditorEvent editorEvent)
        {
            if (editorEvent is Editor.ModeChangedEvent modeChanged)
                _workspaceTabs?.SyncEditorMode(modeChanged.Current);
        }
    }
}
