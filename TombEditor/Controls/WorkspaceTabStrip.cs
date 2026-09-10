using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TombEditor.Controls
{
    public enum EditorWorkspace
    {
        Geometry,
        Terrain,
        Textures,
        Objects,
        Lighting,
        Gameplay
    }

    public sealed class EditorWorkspaceChangedEventArgs : EventArgs
    {
        public EditorWorkspace Workspace { get; }

        public EditorWorkspaceChangedEventArgs(EditorWorkspace workspace)
        {
            Workspace = workspace;
        }
    }

    public partial class WorkspaceTabStrip : UserControl
    {
        private static readonly Dictionary<EditorWorkspace, Color> AccentColors = new Dictionary<EditorWorkspace, Color>
        {
            { EditorWorkspace.Geometry, Color.FromArgb(62, 132, 220) },
            { EditorWorkspace.Terrain, Color.FromArgb(67, 160, 71) },
            { EditorWorkspace.Textures, Color.FromArgb(216, 138, 46) },
            { EditorWorkspace.Objects, Color.FromArgb(141, 99, 198) },
            { EditorWorkspace.Lighting, Color.FromArgb(219, 184, 57) },
            { EditorWorkspace.Gameplay, Color.FromArgb(196, 75, 83) }
        };

        private readonly Dictionary<EditorWorkspace, DarkButton> _buttons;
        private EditorWorkspace _selectedWorkspace = EditorWorkspace.Geometry;

        public event EventHandler<EditorWorkspaceChangedEventArgs> SelectedWorkspaceChanged;

        public EditorWorkspace SelectedWorkspace
        {
            get { return _selectedWorkspace; }
            set { SelectWorkspace(value, true); }
        }

        public WorkspaceTabStrip()
        {
            InitializeComponent();

            _buttons = new Dictionary<EditorWorkspace, DarkButton>
            {
                { EditorWorkspace.Geometry, butGeometry },
                { EditorWorkspace.Terrain, butTerrain },
                { EditorWorkspace.Textures, butTextures },
                { EditorWorkspace.Objects, butObjects },
                { EditorWorkspace.Lighting, butLighting },
                { EditorWorkspace.Gameplay, butGameplay }
            };

            foreach (var button in _buttons.Values)
                button.Click += WorkspaceButton_Click;

            UpdateVisualState();
        }

        public void SyncEditorMode(EditorMode mode)
        {
            if (mode == EditorMode.FaceEdit)
                SelectWorkspace(EditorWorkspace.Textures, false);
            else if (mode == EditorMode.Lighting)
                SelectWorkspace(EditorWorkspace.Lighting, false);
            else if (mode == EditorMode.Geometry &&
                     (_selectedWorkspace == EditorWorkspace.Textures || _selectedWorkspace == EditorWorkspace.Lighting))
                SelectWorkspace(EditorWorkspace.Geometry, false);
        }

        private void WorkspaceButton_Click(object sender, EventArgs e)
        {
            foreach (var pair in _buttons)
            {
                if (!ReferenceEquals(pair.Value, sender))
                    continue;

                SelectWorkspace(pair.Key, true);
                return;
            }
        }

        private void SelectWorkspace(EditorWorkspace workspace, bool raiseEvent)
        {
            bool changed = workspace != _selectedWorkspace;
            _selectedWorkspace = workspace;

            if (changed)
                UpdateVisualState();

            if (raiseEvent)
                SelectedWorkspaceChanged?.Invoke(this, new EditorWorkspaceChangedEventArgs(workspace));
        }

        private void UpdateVisualState()
        {
            foreach (var pair in _buttons)
            {
                float brightness = pair.Key == _selectedWorkspace ? 0.95f : 0.48f;
                pair.Value.BackColor = ScaleColor(AccentColors[pair.Key], brightness);
            }
        }

        private static Color ScaleColor(Color color, float factor)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(255, (int)Math.Round(color.R * factor)),
                Math.Min(255, (int)Math.Round(color.G * factor)),
                Math.Min(255, (int)Math.Round(color.B * factor)));
        }
    }
}
