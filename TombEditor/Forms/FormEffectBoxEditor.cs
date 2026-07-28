using DarkUI.Config;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Controls.VisualScripting;
using TombLib.LevelData;
using TombLib.LevelData.VisualScripting;
using TombLib.Utils;

namespace TombEditor.Forms
{
    /// <summary>
    /// Editor-only graph authoring surface for TEN effect boxes.
    /// Runtime execution is intentionally not part of this first integration block.
    /// </summary>
    public sealed class FormEffectBoxEditor : DarkForm
    {
        private sealed class EffectSequenceItem
        {
            public TriggerNode Root { get; }

            public EffectSequenceItem(TriggerNode root)
            {
                Root = root;
            }

            public override string ToString()
            {
                int nodeCount = 0;
                for (var node = Root; node != null; node = node.Next)
                    nodeCount++;

                var name = string.IsNullOrWhiteSpace(Root?.Name) ? "Effect sequence" : Root.Name;
                return nodeCount > 1 ? name + "  (" + nodeCount + ")" : name;
            }
        }

        private readonly Editor _editor;
        private readonly VolumeInstance _instance;
        private readonly VolumeEventSet _eventSet;
        private readonly Event _graphEvent;
        private readonly VolumeEventSet _backupEventSet;
        private readonly int _eventSetIndex;
        private readonly bool _backupEnabled;
        private readonly string _backupLuaName;
        private readonly List<NodeFunction> _effectFunctions;

        private readonly NodeEditor _nodeEditor = new NodeEditor();
        private readonly ListBox _sequenceList = new ListBox();
        private readonly TreeView _effectLibrary = new TreeView();
        private readonly TextBox _searchBox = new TextBox();
        private readonly TextBox _sequenceName = new TextBox();
        private readonly TextBox _luaName = new TextBox();
        private readonly CheckBox _enabled = new CheckBox();
        private readonly Label _description = new Label();
        private readonly Label _status = new Label();
        private readonly Button _addSequence = new Button();
        private readonly Button _deleteSequence = new Button();
        private readonly Button _addEffect = new Button();
        private readonly Button _deleteNode = new Button();
        private readonly Button _linkNodes = new Button();
        private readonly Button _clearGraph = new Button();
        private readonly Button _ok = new Button();
        private readonly Button _cancel = new Button();

        private bool _restored;
        private bool _lockSelection;

        public FormEffectBoxEditor(VolumeInstance instance)
        {
            _editor = Editor.Instance;
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));

            if (!_instance.IsEffectBox())
                throw new ArgumentException("The supplied volume is not an effect box.", nameof(instance));

            _eventSet = (VolumeEventSet)_instance.EventSet;
            _graphEvent = _instance.GetGraphEvent();
            _eventSetIndex = _editor.Level.Settings.VolumeEventSets.IndexOf(_eventSet);
            _backupEventSet = (VolumeEventSet)_eventSet.Clone();
            _backupEnabled = _instance.Enabled;
            _backupLuaName = _instance.LuaName;
            _effectFunctions = EffectBoxUtils.GetAvailableEffectFunctions().ToList();

            InitializeForm();
            InitializeNodeEditor();
            PopulateEffectLibrary();
            RefreshSequenceList();
            UpdateControls();
        }

        private void InitializeForm()
        {
            Text = "Effect box editor";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(1100, 650);
            Size = new Size(1450, 850);
            ShowIcon = false;
            KeyPreview = true;

            BackColor = Colors.GreyBackground;
            ForeColor = Colors.LightText;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(8)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            Controls.Add(root);

            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                Margin = Padding.Empty
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            header.Controls.Add(CreateLabel("Lua name:"), 0, 0);
            ConfigureTextBox(_luaName);
            _luaName.Text = _instance.LuaName ?? string.Empty;
            header.Controls.Add(_luaName, 1, 0);

            header.Controls.Add(CreateLabel("Box state:"), 2, 0);
            _enabled.Text = "Enabled";
            _enabled.Checked = _instance.Enabled;
            _enabled.AutoSize = true;
            _enabled.Anchor = AnchorStyles.Left;
            _enabled.ForeColor = Colors.LightText;
            header.Controls.Add(_enabled, 3, 0);

            _status.AutoSize = true;
            _status.Anchor = AnchorStyles.Right;
            _status.ForeColor = Color.Goldenrod;
            _status.Text = "Persistent editor graph — runtime effect execution is the next integration layer";
            header.Controls.Add(_status, 5, 0);
            root.Controls.Add(header, 0, 0);

            var workspace = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = Padding.Empty
            };
            workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 245));
            workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 330));
            root.Controls.Add(workspace, 0, 1);

            workspace.Controls.Add(BuildSequencePanel(), 0, 0);
            workspace.Controls.Add(BuildGraphPanel(), 1, 0);
            workspace.Controls.Add(BuildLibraryPanel(), 2, 0);

            var footer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 8, 0, 0)
            };

            ConfigureButton(_ok, "OK", 90);
            ConfigureButton(_cancel, "Cancel", 90);
            _ok.DialogResult = DialogResult.OK;
            _cancel.DialogResult = DialogResult.Cancel;
            _ok.Click += Ok_Click;
            _cancel.Click += (sender, args) => RestoreState();
            footer.Controls.Add(_ok);
            footer.Controls.Add(_cancel);
            root.Controls.Add(footer, 0, 2);

            AcceptButton = _ok;
            CancelButton = _cancel;
            FormClosing += FormEffectBoxEditor_FormClosing;
            KeyDown += FormEffectBoxEditor_KeyDown;
        }

        private Control BuildSequencePanel()
        {
            var panel = CreatePanel();
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(8)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            panel.Controls.Add(layout);

            layout.Controls.Add(CreateHeading("Effect entries"), 0, 0);

            ConfigureListBox(_sequenceList);
            _sequenceList.SelectedIndexChanged += SequenceList_SelectedIndexChanged;
            layout.Controls.Add(_sequenceList, 0, 1);

            ConfigureTextBox(_sequenceName);
            _sequenceName.PlaceholderText = "Entry name";
            _sequenceName.Validated += SequenceName_Validated;
            layout.Controls.Add(_sequenceName, 0, 2);

            var addDelete = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            addDelete.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            addDelete.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            ConfigureButton(_addSequence, "Create", 0);
            ConfigureButton(_deleteSequence, "Delete", 0);
            _addSequence.Click += (sender, args) => AddEffectNode(true);
            _deleteSequence.Click += DeleteSequence_Click;
            addDelete.Controls.Add(_addSequence, 0, 0);
            addDelete.Controls.Add(_deleteSequence, 1, 0);
            layout.Controls.Add(addDelete, 0, 3);

            var nodeButtons = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            nodeButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            nodeButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            ConfigureButton(_deleteNode, "Delete node", 0);
            ConfigureButton(_linkNodes, "Link nodes", 0);
            _deleteNode.Click += DeleteNode_Click;
            _linkNodes.Click += LinkNodes_Click;
            nodeButtons.Controls.Add(_deleteNode, 0, 0);
            nodeButtons.Controls.Add(_linkNodes, 1, 0);
            layout.Controls.Add(nodeButtons, 0, 4);

            ConfigureButton(_clearGraph, "Clear graph", 0);
            _clearGraph.Click += ClearGraph_Click;
            layout.Controls.Add(_clearGraph, 0, 5);

            return panel;
        }

        private Control BuildGraphPanel()
        {
            var panel = CreatePanel();
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(8)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            panel.Controls.Add(layout);

            layout.Controls.Add(CreateHeading("Effect composition"), 0, 0);
            _nodeEditor.Dock = DockStyle.Fill;
            _nodeEditor.Margin = Padding.Empty;
            layout.Controls.Add(_nodeEditor, 0, 1);
            return panel;
        }

        private Control BuildLibraryPanel()
        {
            var panel = CreatePanel();
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(8)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 70));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            panel.Controls.Add(layout);

            layout.Controls.Add(CreateHeading("Available TEN effects"), 0, 0);

            ConfigureTextBox(_searchBox);
            _searchBox.PlaceholderText = "Search effects...";
            _searchBox.TextChanged += (sender, args) => PopulateEffectLibrary();
            layout.Controls.Add(_searchBox, 0, 1);

            _effectLibrary.Dock = DockStyle.Fill;
            _effectLibrary.BackColor = Colors.DarkBackground;
            _effectLibrary.ForeColor = Colors.LightText;
            _effectLibrary.BorderStyle = BorderStyle.FixedSingle;
            _effectLibrary.HideSelection = false;
            _effectLibrary.AfterSelect += EffectLibrary_AfterSelect;
            _effectLibrary.NodeMouseDoubleClick += (sender, args) => AddEffectNode(false);
            layout.Controls.Add(_effectLibrary, 0, 2);

            ConfigureButton(_addEffect, "Add selected effect", 0);
            _addEffect.Click += (sender, args) => AddEffectNode(false);
            layout.Controls.Add(_addEffect, 0, 3);

            _description.Dock = DockStyle.Fill;
            _description.ForeColor = Colors.DisabledText;
            _description.Padding = new Padding(4);
            _description.AutoEllipsis = true;
            layout.Controls.Add(_description, 0, 4);

            var hint = CreateLabel("Double-click also adds an effect node.");
            hint.ForeColor = Colors.DisabledText;
            layout.Controls.Add(hint, 0, 5);

            return panel;
        }

        private void InitializeNodeEditor()
        {
            _nodeEditor.GridSize = _editor.Configuration.NodeEditor_Size;
            _nodeEditor.GridStep = _editor.Configuration.NodeEditor_GridStep;
            _nodeEditor.DefaultNodeWidth = _editor.Configuration.NodeEditor_DefaultNodeWidth;
            _nodeEditor.LinksAsRopes = _editor.Configuration.NodeEditor_LinksAsRopes;
            _nodeEditor.ShowGrips = _editor.Configuration.NodeEditor_ShowGrips;
            _nodeEditor.SelectionColor = Color.Goldenrod;
            _nodeEditor.NodeFunctions.Clear();
            _nodeEditor.NodeFunctions.AddRange(_effectFunctions);
            _nodeEditor.Initialize(_editor.Level);
            _nodeEditor.Nodes = _graphEvent.Nodes;

            if (_graphEvent.NodePosition.X == float.MaxValue)
                _nodeEditor.ViewPosition = new Vector2(_nodeEditor.GridSize / 2.0f);
            else
                _nodeEditor.ViewPosition = _graphEvent.NodePosition;

            _nodeEditor.SelectionChanged += NodeEditor_SelectionChanged;
            _nodeEditor.ViewPositionChanged += NodeEditor_ViewPositionChanged;
        }

        private void PopulateEffectLibrary()
        {
            var selectedSignature = (_effectLibrary.SelectedNode?.Tag as NodeFunction)?.Signature;
            var search = (_searchBox.Text ?? string.Empty).Trim();

            _effectLibrary.BeginUpdate();
            _effectLibrary.Nodes.Clear();

            var functions = _effectFunctions.Where(function =>
                string.IsNullOrEmpty(search) ||
                (function.Name ?? string.Empty).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                (function.Section ?? string.Empty).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                (function.Description ?? string.Empty).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var group in functions.GroupBy(function => string.IsNullOrWhiteSpace(function.Section) ? "Effects" : function.Section))
            {
                var sectionNode = new TreeNode(group.Key);
                foreach (var function in group)
                {
                    var functionNode = new TreeNode(function.Name) { Tag = function };
                    sectionNode.Nodes.Add(functionNode);
                    if (function.Signature == selectedSignature)
                        _effectLibrary.SelectedNode = functionNode;
                }
                _effectLibrary.Nodes.Add(sectionNode);
            }

            _effectLibrary.ExpandAll();
            _effectLibrary.EndUpdate();
            UpdateControls();
        }

        private void RefreshSequenceList(TriggerNode selectRoot = null)
        {
            _lockSelection = true;
            _sequenceList.BeginUpdate();
            _sequenceList.Items.Clear();

            foreach (var root in _graphEvent.Nodes)
            {
                var item = new EffectSequenceItem(root);
                _sequenceList.Items.Add(item);
                if (root == selectRoot)
                    _sequenceList.SelectedItem = item;
            }

            _sequenceList.EndUpdate();
            _lockSelection = false;

            if (_sequenceList.SelectedIndex < 0 && _sequenceList.Items.Count > 0)
                _sequenceList.SelectedIndex = 0;

            UpdateControls();
        }

        private void AddEffectNode(bool forceNewSequence)
        {
            var function = GetSelectedFunction() ?? _effectFunctions.FirstOrDefault();
            if (function == null)
            {
                MessageBox.Show(this,
                    "No compatible TEN effect nodes were found in the installed node catalogs.",
                    "Effect box editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var nodeCount = TriggerNode.LinearizeNodes(_graphEvent.Nodes).Count;
            var node = EffectBoxUtils.CreateNode(function, nodeCount + 1);
            var selectedNode = _nodeEditor.SelectedNode;
            var linked = false;

            if (!forceNewSequence && selectedNode != null && selectedNode.Next == null)
            {
                selectedNode.Next = node;
                node.Previous = selectedNode;
                node.ScreenPosition = ClampNodePosition(selectedNode.ScreenPosition + new Vector2(0, 28));
                linked = true;
            }

            if (!linked)
            {
                int rootIndex = _graphEvent.Nodes.Count;
                node.ScreenPosition = new Vector2(32 + (rootIndex % 3) * 72, 38 + (rootIndex / 3) * 52);
                node.ScreenPosition = ClampNodePosition(node.ScreenPosition);
                _graphEvent.Nodes.Add(node);
            }

            _nodeEditor.Nodes = _graphEvent.Nodes;
            _nodeEditor.SelectNode(node, false, true);
            _nodeEditor.ShowNode(node);
            RefreshSequenceList(FindRoot(node));
            _editor.ObjectChange(_instance, ObjectChangeType.Change);
        }

        private static Vector2 ClampNodePosition(Vector2 value)
        {
            return Vector2.Clamp(value, new Vector2(1), new Vector2(255));
        }

        private NodeFunction GetSelectedFunction()
        {
            return _effectLibrary.SelectedNode?.Tag as NodeFunction;
        }

        private TriggerNode FindRoot(TriggerNode node)
        {
            if (node == null)
                return null;

            while (node.Previous != null)
                node = node.Previous;
            return node;
        }

        private void SequenceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_lockSelection)
                return;

            var root = (_sequenceList.SelectedItem as EffectSequenceItem)?.Root;
            _sequenceName.Text = root?.Name ?? string.Empty;

            if (root != null)
            {
                _nodeEditor.SelectNode(root, false, true);
                _nodeEditor.ShowNode(root);
            }

            UpdateControls();
        }

        private void SequenceName_Validated(object sender, EventArgs e)
        {
            var root = (_sequenceList.SelectedItem as EffectSequenceItem)?.Root;
            if (root == null)
                return;

            var name = (_sequenceName.Text ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(name))
                root.Name = name;

            RefreshSequenceList(root);
            _nodeEditor.UpdateVisibleNodes(true);
            _editor.ObjectChange(_instance, ObjectChangeType.Change);
        }

        private void DeleteSequence_Click(object sender, EventArgs e)
        {
            var root = (_sequenceList.SelectedItem as EffectSequenceItem)?.Root;
            if (root == null)
                return;

            if (MessageBox.Show(this,
                    "Delete the selected effect entry and all nodes connected to it?",
                    "Delete effect entry", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            _graphEvent.Nodes.Remove(root);
            _nodeEditor.Nodes = _graphEvent.Nodes;
            RefreshSequenceList();
            _editor.ObjectChange(_instance, ObjectChangeType.Change);
        }

        private void DeleteNode_Click(object sender, EventArgs e)
        {
            if (_nodeEditor.SelectedNodes.Count == 0)
                return;

            _nodeEditor.DeleteNodes();
            RefreshSequenceList();
            _editor.ObjectChange(_instance, ObjectChangeType.Change);
        }

        private void LinkNodes_Click(object sender, EventArgs e)
        {
            _nodeEditor.LinkSelectedNodes();
            RefreshSequenceList(FindRoot(_nodeEditor.SelectedNode));
            _editor.ObjectChange(_instance, ObjectChangeType.Change);
        }

        private void ClearGraph_Click(object sender, EventArgs e)
        {
            if (_graphEvent.Nodes.Count == 0)
                return;

            if (MessageBox.Show(this, "Delete the complete effect graph?", "Clear effect graph",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            _nodeEditor.ClearNodes();
            RefreshSequenceList();
            _editor.ObjectChange(_instance, ObjectChangeType.Change);
        }

        private void NodeEditor_SelectionChanged(object sender, EventArgs e)
        {
            var root = FindRoot(_nodeEditor.SelectedNode);
            if (root != null)
            {
                _lockSelection = true;
                for (int i = 0; i < _sequenceList.Items.Count; i++)
                {
                    if ((_sequenceList.Items[i] as EffectSequenceItem)?.Root == root)
                    {
                        _sequenceList.SelectedIndex = i;
                        _sequenceName.Text = root.Name;
                        break;
                    }
                }
                _lockSelection = false;
            }

            UpdateControls();
        }

        private void NodeEditor_ViewPositionChanged(object sender, EventArgs e)
        {
            _graphEvent.NodePosition = _nodeEditor.ViewPosition;
        }

        private void EffectLibrary_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var function = e.Node?.Tag as NodeFunction;
            _description.Text = function == null
                ? "Select an effect to view its description."
                : (function.Description ?? string.Empty) + Environment.NewLine + Environment.NewLine + function.Signature;
            UpdateControls();
        }

        private void UpdateControls()
        {
            var hasSequence = _sequenceList.SelectedItem is EffectSequenceItem;
            var hasNode = _nodeEditor.SelectedNodes.Count > 0;
            var hasFunction = GetSelectedFunction() != null || _effectFunctions.Count > 0;

            _sequenceName.Enabled = hasSequence;
            _deleteSequence.Enabled = hasSequence;
            _addSequence.Enabled = hasFunction;
            _addEffect.Enabled = hasFunction;
            _deleteNode.Enabled = hasNode;
            _linkNodes.Enabled = _nodeEditor.SelectedNodes.Count > 1;
            _clearGraph.Enabled = _graphEvent.Nodes.Count > 0;
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            var requestedName = (_luaName.Text ?? string.Empty).Trim();
            if (!_instance.CanSetLuaName(requestedName))
            {
                MessageBox.Show(this, "The Lua name is already assigned to another object.",
                    "Effect box editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                return;
            }

            _instance.LuaName = requestedName;
            _instance.Enabled = _enabled.Checked;
            _eventSet.Activators = VolumeActivators.None;
            _eventSet.LastUsedEvent = EventType.OnVolumeInside;
            _graphEvent.NodePosition = _nodeEditor.ViewPosition;
            _editor.ObjectChange(_instance, ObjectChangeType.Change);
            _editor.EventSetsChange();
        }

        private void RestoreState()
        {
            if (_restored)
                return;

            _restored = true;
            _instance.Enabled = _backupEnabled;
            _instance.LuaName = _backupLuaName;

            if (_eventSetIndex >= 0 && _eventSetIndex < _editor.Level.Settings.VolumeEventSets.Count)
                _editor.Level.Settings.VolumeEventSets[_eventSetIndex] = _backupEventSet;
            else if (!_editor.Level.Settings.VolumeEventSets.Contains(_backupEventSet))
                _editor.Level.Settings.VolumeEventSets.Add(_backupEventSet);

            _instance.EventSet = _backupEventSet;
            _editor.ObjectChange(_instance, ObjectChangeType.Change);
            _editor.EventSetsChange();
        }

        private void FormEffectBoxEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult != DialogResult.OK)
                RestoreState();
        }

        private void FormEffectBoxEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && _nodeEditor.Focused)
            {
                DeleteNode_Click(sender, EventArgs.Empty);
                e.Handled = true;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _nodeEditor.SelectionChanged -= NodeEditor_SelectionChanged;
                _nodeEditor.ViewPositionChanged -= NodeEditor_ViewPositionChanged;
            }

            base.Dispose(disposing);
        }

        private static Panel CreatePanel()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.DarkBackground,
                Margin = new Padding(4),
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private static Label CreateHeading(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Bold),
                ForeColor = Colors.LightText,
                Anchor = AnchorStyles.Left
            };
        }

        private static Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                ForeColor = Colors.LightText,
                Anchor = AnchorStyles.Left
            };
        }

        private static void ConfigureTextBox(TextBox textBox)
        {
            textBox.Dock = DockStyle.Fill;
            textBox.BackColor = Colors.DarkBackground;
            textBox.ForeColor = Colors.LightText;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.Margin = new Padding(4);
        }

        private static void ConfigureListBox(ListBox listBox)
        {
            listBox.Dock = DockStyle.Fill;
            listBox.BackColor = Colors.DarkBackground;
            listBox.ForeColor = Colors.LightText;
            listBox.BorderStyle = BorderStyle.FixedSingle;
            listBox.IntegralHeight = false;
        }

        private static void ConfigureButton(Button button, string text, int width)
        {
            button.Text = text;
            button.Dock = width == 0 ? DockStyle.Fill : DockStyle.None;
            button.Width = width;
            button.Height = 28;
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = Colors.GreyBackground;
            button.ForeColor = Colors.LightText;
            button.FlatAppearance.BorderColor = Colors.DarkBorder;
            button.Margin = new Padding(3);
        }
    }
}
