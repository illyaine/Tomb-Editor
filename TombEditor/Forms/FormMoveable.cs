using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;
using TombLib.LevelData.ObjectParameters;
using TombLib.Utils;
using System.Drawing;

namespace TombEditor.Forms
{
    public partial class FormMoveable : DarkForm
    {
        private readonly MoveableInstance _movable;
        private readonly List<ObjectParameterOcbDefinition> _ocbDefinitions = new List<ObjectParameterOcbDefinition>();
        private bool _updatingOcbList;
        private Vector3 oldColor;
        private Editor _editor;
        public FormMoveable(MoveableInstance moveable)
        {
            _movable = moveable;
            InitializeComponent();
            _editor = Editor.Instance;

            if (ObjectParameterProviderRegistry.FindProvider(TenReviewedOcbObjectParameterProvider.ProviderId) == null)
                ObjectParameterProviderRegistry.Register(new TenReviewedOcbObjectParameterProvider());

            // Set window property handlers
            Configuration.ConfigureWindow(this, _editor.Configuration);

            // Clear body flag in TEN is repurposed for reflection canceling.
            if (_editor.Level.IsTombEngine)
                cbClearBody.Text = "No reflection";
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            _movable.Color = oldColor;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void FormObject_Load(object sender, EventArgs e)
        {
            // Disable version-specific controls
            tbOCB.Enabled = _editor.Level.Settings.GameVersion.Native() > TRVersion.Game.TR3;

            oldColor = _movable.Color;
            cbBit1.Checked = (_movable.CodeBits & (1 << 0)) != 0;
            cbBit2.Checked = (_movable.CodeBits & (1 << 1)) != 0;
            cbBit3.Checked = (_movable.CodeBits & (1 << 2)) != 0;
            cbBit4.Checked = (_movable.CodeBits & (1 << 3)) != 0;
            cbBit5.Checked = (_movable.CodeBits & (1 << 4)) != 0;
            panelColor.BackColor = (_movable.Color * 0.5f).ToWinFormsColor();
            cbInvisible.Checked = _movable.Invisible;
            cbClearBody.Checked = _movable.ClearBody;
            tbOCB.Text = _movable.Ocb.ToString();

            LoadReviewedOcbSelector();

            // Disable mesh-specific controls
            var canBeColored = _movable.CanBeColored();
            Size = new System.Drawing.Size(Size.Width, canBeColored ? 344 : 316);
            lblColor.Visible = canBeColored;
            panelColor.Visible = canBeColored;
            butResetTint.Visible = canBeColored;
        }

        private void LoadReviewedOcbSelector()
        {
            _updatingOcbList = true;
            _ocbDefinitions.Clear();
            listOcbDefinitions.Items.Clear();

            bool useReviewedSelector = _editor.Level.IsTombEngine;
            label1.Text = useReviewedSelector ? "OCB:" : "OCB:";
            tbOCB.Visible = !useReviewedSelector;
            listOcbDefinitions.Visible = useReviewedSelector;

            if (!useReviewedSelector)
            {
                _updatingOcbList = false;
                return;
            }

            ObjectParameterContext context = ObjectParameterContextFactory.FromObject(_editor.Level, _movable);
            ObjectParameterDefinitionSet definitionSet = ObjectParameterProviderRegistry.GetDefinitionSets(context).FirstOrDefault(set => set.OcbDefinitions.Count != 0);

            if (definitionSet == null)
            {
                listOcbDefinitions.Items.Add(new OcbListItem(new ObjectParameterOcbDefinition
                {
                    Value = _movable.Ocb,
                    Name = "Current raw OCB",
                    Description = "Current raw OCB value.",
                    Group = "Raw",
                    Mode = ObjectParameterOcbMode.FixedValue,
                    MappingStatus = ObjectParameterMappingStatus.Unknown
                }), true);
                _updatingOcbList = false;
                return;
            }

            foreach (ObjectParameterOcbDefinition definition in definitionSet.OcbDefinitions)
            {
                _ocbDefinitions.Add(definition);
                listOcbDefinitions.Items.Add(new OcbListItem(definition), IsDefinitionActive(definition, _movable.Ocb));
            }

            if (CalculateOcbFromList() != _movable.Ocb)
            {
                for (int i = 0; i < listOcbDefinitions.Items.Count; i++)
                    listOcbDefinitions.SetItemChecked(i, false);

                var rawDefinition = new ObjectParameterOcbDefinition
                {
                    Value = _movable.Ocb,
                    Name = "Unknown / undocumented",
                    Description = "Current raw OCB value. No confirmed object-specific meaning is available yet.",
                    Group = "Raw",
                    Mode = ObjectParameterOcbMode.FixedValue,
                    MappingStatus = ObjectParameterMappingStatus.Unknown,
                    Warning = "Do not overwrite this value unless you know what it does."
                };
                _ocbDefinitions.Add(rawDefinition);
                listOcbDefinitions.Items.Add(new OcbListItem(rawDefinition), true);
            }

            tbOCB.Text = CalculateOcbFromList().ToString();
            _updatingOcbList = false;
        }

        private static bool IsDefinitionActive(ObjectParameterOcbDefinition definition, short currentOcb)
        {
            if (definition.Mode == ObjectParameterOcbMode.AdditiveFlags || definition.IsCombinable)
                return definition.Value > 0 && (currentOcb & definition.Value) == definition.Value;

            return definition.Value == currentOcb;
        }

        private short CalculateOcbFromList()
        {
            int flags = 0;
            int? fixedValue = null;

            for (int i = 0; i < listOcbDefinitions.Items.Count; i++)
            {
                if (!listOcbDefinitions.GetItemChecked(i))
                    continue;

                if (!(listOcbDefinitions.Items[i] is OcbListItem item))
                    continue;

                ObjectParameterOcbDefinition definition = item.Definition;
                if (definition.Mode == ObjectParameterOcbMode.AdditiveFlags || definition.IsCombinable)
                    flags |= definition.Value;
                else
                    fixedValue = definition.Value;
            }

            int value = fixedValue ?? 0;
            value |= flags;

            if (value < short.MinValue)
                return short.MinValue;
            if (value > short.MaxValue)
                return short.MaxValue;

            return (short)value;
        }

        private short GetSelectedOcb()
        {
            if (!listOcbDefinitions.Visible)
            {
                short ocb;
                if (!short.TryParse(tbOCB.Text, out ocb))
                    throw new FormatException();

                return ocb;
            }

            return CalculateOcbFromList();
        }

        private void listOcbDefinitions_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_updatingOcbList)
                return;

            BeginInvoke(new Action(() => NormalizeOcbSelection(e.Index, e.NewValue == CheckState.Checked)));
        }

        private void NormalizeOcbSelection(int changedIndex, bool checkedState)
        {
            if (changedIndex < 0 || changedIndex >= listOcbDefinitions.Items.Count)
                return;

            if (!(listOcbDefinitions.Items[changedIndex] is OcbListItem changedItem))
                return;

            _updatingOcbList = true;

            if (checkedState && changedItem.Definition.Mode == ObjectParameterOcbMode.FixedValue && !changedItem.Definition.IsCombinable)
            {
                for (int i = 0; i < listOcbDefinitions.Items.Count; i++)
                {
                    if (i == changedIndex)
                        continue;

                    if (listOcbDefinitions.Items[i] is OcbListItem otherItem &&
                        otherItem.Definition.Mode == ObjectParameterOcbMode.FixedValue &&
                        !otherItem.Definition.IsCombinable)
                    {
                        listOcbDefinitions.SetItemChecked(i, false);
                    }
                }
            }

            tbOCB.Text = CalculateOcbFromList().ToString();
            _updatingOcbList = false;
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            short ocb;
            try
            {
                ocb = GetSelectedOcb();
            }
            catch (FormatException)
            {
                DarkMessageBox.Show(this, "The value of OCB field is not valid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _editor.UndoManager.PushObjectPropertyChanged(_movable);

            byte CodeBits = 0;
            CodeBits |= (byte)(cbBit1.Checked ? 1 << 0 : 0);
            CodeBits |= (byte)(cbBit2.Checked ? 1 << 1 : 0);
            CodeBits |= (byte)(cbBit3.Checked ? 1 << 2 : 0);
            CodeBits |= (byte)(cbBit4.Checked ? 1 << 3 : 0);
            CodeBits |= (byte)(cbBit5.Checked ? 1 << 4 : 0);
            _movable.CodeBits = CodeBits;

            _movable.Invisible = cbInvisible.Checked;
            _movable.ClearBody = cbClearBody.Checked;

            _movable.Ocb = ocb;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void tbOCB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '-' || tbOCB.SelectionStart != 0))
                e.Handled = true;
        }

        private void panelColor_Click(object sender, EventArgs e)
        {
            EditorActions.EditColor(this, _movable, (Vector3 newColor) =>
            {
                panelColor.BackColor = newColor.ToWinFormsColor();
            });
        }

        private void butResetTint_Click(object sender, EventArgs e)
        {
            panelColor.BackColor = Color.Gray;
            _movable.Color = panelColor.BackColor.ToFloat3Color() * 2.0f;
        }

        private sealed class OcbListItem
        {
            public OcbListItem(ObjectParameterOcbDefinition definition)
            {
                Definition = definition;
            }

            public ObjectParameterOcbDefinition Definition { get; }

            public override string ToString()
            {
                return Definition.Value + " - " + Definition.Name;
            }
        }
    }
}