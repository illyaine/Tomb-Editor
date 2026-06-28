using DarkUI.Controls;
using DarkUI.Docking;
using System;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.ToolWindows
{
    public partial class Lighting : DarkToolWindow
    {
        private readonly Editor _editor;

        private DarkPanel _hdrPanel;
        private DarkComboBox _cmbHDRMode;
        private DarkNumericUpDown _numHDRPhysicalIntensity;
        private DarkNumericUpDown _numHDRPhysicalRange;
        private DarkNumericUpDown _numHDRSourceWidth;
        private DarkNumericUpDown _numHDRSourceHeight;
        private DarkNumericUpDown _numHDRCoreIntensity;
        private DarkNumericUpDown _numHDRHaloIntensity;
        private DarkNumericUpDown _numHDRGlareIntensity;
        private bool _updatingHDRControls;

        public Lighting()
        {
            InitializeComponent();
            InitializeHDRControls();
            CommandHandler.AssignCommandsToControls(Editor.Instance, this, toolTip);

            foreach (LightType l in Enum.GetValues(typeof(LightType)))
                cmbLightTypes.Items.Add(l.ToString().SplitCamelcase());

            cmbLightQuality.SelectedIndex = cmbLightTypes.SelectedIndex = 0;

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;
        }

        private void InitializeHDRControls()
        {
            _hdrPanel = new DarkPanel
            {
                Location = new Point(3, 144),
                Size = new Size(365, 218),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Visible = false
            };

            var title = new DarkLabel
            {
                Location = new Point(4, 2),
                Size = new Size(250, 20),
                Text = "HDR light effects",
                Font = new Font("Segoe UI", 8.25f, FontStyle.Bold),
                ForeColor = Color.Gainsboro
            };
            _hdrPanel.Controls.Add(title);

            _cmbHDRMode = new DarkComboBox
            {
                Location = new Point(174, 24),
                Size = new Size(184, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbHDRMode.Items.Add("Light and effects");
            _cmbHDRMode.Items.Add("Light only");
            _cmbHDRMode.Items.Add("Effects only");
            _cmbHDRMode.SelectedIndexChanged += cmbHDRMode_SelectedIndexChanged;
            AddHDRLabel("Mode", 27);
            _hdrPanel.Controls.Add(_cmbHDRMode);

            _numHDRPhysicalIntensity = AddHDRNumeric("Light intensity", 51, 0.0m, 10.0m, 0.05m, 2);
            _numHDRPhysicalRange = AddHDRNumeric("Light range (sectors)", 75, 0.01m, 100.0m, 0.1m, 2);
            _numHDRSourceWidth = AddHDRNumeric("Visible source width", 99, 0.01m, 16.0m, 0.01m, 3);
            _numHDRSourceHeight = AddHDRNumeric("Visible source height", 123, 0.01m, 16.0m, 0.01m, 3);
            _numHDRCoreIntensity = AddHDRNumeric("Core intensity", 147, 0.0m, 20.0m, 0.1m, 2);
            _numHDRHaloIntensity = AddHDRNumeric("Halo intensity", 171, 0.0m, 20.0m, 0.1m, 2);
            _numHDRGlareIntensity = AddHDRNumeric("Glare intensity", 195, 0.0m, 8.0m, 0.1m, 2);

            _numHDRPhysicalIntensity.ValueChanged += (sender, args) => UpdateHDRFloat(
                light => light.HDRPhysicalIntensity,
                (light, value) => light.HDRPhysicalIntensity = value,
                _numHDRPhysicalIntensity);
            _numHDRPhysicalRange.ValueChanged += (sender, args) => UpdateHDRFloat(
                light => light.HDRPhysicalRange,
                (light, value) => light.HDRPhysicalRange = value,
                _numHDRPhysicalRange);
            _numHDRSourceWidth.ValueChanged += (sender, args) => UpdateHDRFloat(
                light => light.HDRSourceWidth,
                (light, value) => light.HDRSourceWidth = value,
                _numHDRSourceWidth);
            _numHDRSourceHeight.ValueChanged += (sender, args) => UpdateHDRFloat(
                light => light.HDRSourceHeight,
                (light, value) => light.HDRSourceHeight = value,
                _numHDRSourceHeight);
            _numHDRCoreIntensity.ValueChanged += (sender, args) => UpdateHDRFloat(
                light => light.HDRCoreIntensity,
                (light, value) => light.HDRCoreIntensity = value,
                _numHDRCoreIntensity);
            _numHDRHaloIntensity.ValueChanged += (sender, args) => UpdateHDRFloat(
                light => light.HDRHaloIntensity,
                (light, value) => light.HDRHaloIntensity = value,
                _numHDRHaloIntensity);
            _numHDRGlareIntensity.ValueChanged += (sender, args) => UpdateHDRFloat(
                light => light.HDRGlareIntensity,
                (light, value) => light.HDRGlareIntensity = value,
                _numHDRGlareIntensity);

            Controls.Add(_hdrPanel);
            MinimumSize = new Size(371, 366);
            Size = new Size(Math.Max(Width, 371), Math.Max(Height, 366));
        }

        private void AddHDRLabel(string text, int y)
        {
            _hdrPanel.Controls.Add(new DarkLabel
            {
                Location = new Point(4, y),
                Size = new Size(166, 20),
                Text = text,
                ForeColor = Color.Gainsboro
            });
        }

        private DarkNumericUpDown AddHDRNumeric(string label, int y, decimal minimum, decimal maximum, decimal increment, int decimalPlaces)
        {
            AddHDRLabel(label, y + 3);
            var control = new DarkNumericUpDown
            {
                Location = new Point(174, y),
                Size = new Size(184, 23),
                Minimum = minimum,
                Maximum = maximum,
                Increment = increment,
                DecimalPlaces = decimalPlaces
            };
            _hdrPanel.Controls.Add(control);
            return control;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= EditorEventRaised;
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private static decimal ClampToControl(float value, NumericUpDown control)
        {
            return Math.Max(control.Minimum, Math.Min(control.Maximum, (decimal)value));
        }

        private void UpdateHDRControls(LightInstance light, bool isTEN)
        {
            var isHDR = isTEN && light?.IsHDRLight == true;
            _hdrPanel.Visible = isHDR;
            _hdrPanel.Enabled = isHDR;
            if (!isHDR)
                return;

            _updatingHDRControls = true;
            _cmbHDRMode.SelectedIndex = (int)light.HDRMode;
            _numHDRPhysicalIntensity.Value = ClampToControl(light.HDRPhysicalIntensity, _numHDRPhysicalIntensity);
            _numHDRPhysicalRange.Value = ClampToControl(light.HDRPhysicalRange, _numHDRPhysicalRange);
            _numHDRSourceWidth.Value = ClampToControl(light.HDRSourceWidth, _numHDRSourceWidth);
            _numHDRSourceHeight.Value = ClampToControl(light.HDRSourceHeight, _numHDRSourceHeight);
            _numHDRCoreIntensity.Value = ClampToControl(light.HDRCoreIntensity, _numHDRCoreIntensity);
            _numHDRHaloIntensity.Value = ClampToControl(light.HDRHaloIntensity, _numHDRHaloIntensity);
            _numHDRGlareIntensity.Value = ClampToControl(light.HDRGlareIntensity, _numHDRGlareIntensity);
            _updatingHDRControls = false;
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.InitEvent ||
                obj is Editor.GameVersionChangedEvent ||
                obj is Editor.LevelChangedEvent ||
                obj is Editor.ObjectChangedEvent ||
                obj is Editor.SelectedObjectChangedEvent)
            {
                var isTEN = _editor.Level.Settings.GameVersion == TRVersion.Game.TombEngine;
                var light = _editor.SelectedObject as LightInstance;
                var isHDR = isTEN && light?.IsHDRLight == true;

                bool hasInRange = false;
                bool hasOutRange = false;
                bool hasInOutAngle = false;
                bool hasDirection = false;
                bool canCastShadows = false;
                bool canCastDynamicShadows = false;
                bool canIlluminateGeometry = false;

                cmbLightQuality.Enabled = false;

                if (isHDR)
                {
                    canCastShadows = true;
                    canCastDynamicShadows = light.HDRMode != HDRLightMode.EffectsOnly;
                }
                else if (light != null)
                {
                    switch (light.Type)
                    {
                        case LightType.Point:
                            hasInRange = true;
                            hasOutRange = true;
                            canIlluminateGeometry = true;
                            canCastShadows = true;
                            canCastDynamicShadows = isTEN;
                            break;

                        case LightType.Shadow:
                            hasInRange = true;
                            hasOutRange = true;
                            canCastShadows = true;
                            canIlluminateGeometry = true;
                            break;

                        case LightType.Effect:
                            hasInRange = true;
                            hasOutRange = true;
                            break;

                        case LightType.FogBulb:
                            hasOutRange = true;
                            break;

                        case LightType.Spot:
                            hasInRange = true;
                            hasOutRange = true;
                            hasInOutAngle = true;
                            hasDirection = true;
                            canIlluminateGeometry = true;
                            canCastShadows = true;
                            canCastDynamicShadows = isTEN;
                            break;

                        case LightType.Sun:
                            hasDirection = true;
                            canCastShadows = true;
                            canIlluminateGeometry = true;
                            break;
                    }
                }

                panelLightColor.Enabled = light != null;
                cbLightEnabled.Enabled = light != null;
                cbLightIsObstructedByRoomGeometry.Enabled = canCastShadows;
                cbLightCastsShadow.Enabled = canCastDynamicShadows;
                cbLightIsDynamicallyUsed.Enabled = canIlluminateGeometry;
                cbLightIsStaticallyUsed.Enabled = canIlluminateGeometry;
                cbLightIsUsedForImportedGeometry.Enabled = canIlluminateGeometry;
                numIntensity.Enabled = light != null && !isHDR;
                numInnerRange.Enabled = hasInRange;
                numOuterRange.Enabled = hasOutRange;
                numInnerAngle.Enabled = hasInOutAngle;
                numOuterAngle.Enabled = hasInOutAngle;
                numDirectionY.Enabled = hasDirection;
                numDirectionX.Enabled = hasDirection;

                panelLightColor.BackColor = light != null ? new Vector4(light.Color * 0.5f, 1.0f).ToWinFormsColor() : BackColor;
                numIntensity.Value = (decimal)(light != null && !isHDR ? light.Intensity : 0);
                numInnerRange.Value = hasInRange ? (decimal)light.InnerRange : 0;
                numOuterRange.Value = hasOutRange ? (decimal)light.OuterRange : 0;
                numInnerAngle.Value = hasInOutAngle ? (decimal)light.InnerAngle : 0;
                numOuterAngle.Value = hasInOutAngle ? (decimal)light.OuterAngle : 0;
                numDirectionY.Value = hasDirection ? (decimal)light.RotationY : 0;
                numDirectionX.Value = hasDirection ? (decimal)light.RotationX : 0;

                cbLightEnabled.Checked = light?.Enabled ?? false;
                cbLightIsObstructedByRoomGeometry.Checked = light?.IsObstructedByRoomGeometry ?? false;
                cbLightIsDynamicallyUsed.Checked = light?.IsDynamicallyUsed ?? false;
                cbLightIsStaticallyUsed.Checked = light?.IsStaticallyUsed ?? false;
                cbLightIsUsedForImportedGeometry.Checked = light?.IsUsedForImportedGeometry ?? false;
                cbLightCastsShadow.Checked = light?.CastDynamicShadows ?? false;

                if (light != null && !isHDR)
                {
                    cmbLightQuality.Enabled = true;
                    cmbLightQuality.SelectedIndex = Math.Min((int)light.Quality, cmbLightQuality.Items.Count - 1);
                }
                else
                {
                    cmbLightQuality.Enabled = false;
                    cmbLightQuality.SelectedIndex = 0;
                }

                cmbLightTypes.SelectedIndex = (int)(light?.DisplayType ?? (LightType)cmbLightTypes.SelectedIndex);
                UpdateHDRControls(light, isTEN);
            }

            if (obj is Editor.ConfigurationChangedEvent &&
                ((Editor.ConfigurationChangedEvent)obj).UpdateKeyboardShortcuts)
            {
                CommandHandler.AssignCommandsToControls(_editor, this, toolTip, true);
            }
        }

        private void UpdateHDRFloat(Func<LightInstance, float> getter, Action<LightInstance, float> setter, DarkNumericUpDown control)
        {
            if (_updatingHDRControls || !control.Enabled)
                return;

            EditorActions.UpdateLight<float>(
                (light, value) => !light.IsHDRLight || Compare(getter(light), value, control),
                (light, value) => setter(light, value),
                light => (float)control.Value);
        }

        private void cmbHDRMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updatingHDRControls || _cmbHDRMode.SelectedIndex < 0)
                return;

            EditorActions.UpdateLight<HDRLightMode>(
                (light, value) => !light.IsHDRLight || light.HDRMode == value,
                (light, value) => light.HDRMode = value,
                light => (HDRLightMode)_cmbHDRMode.SelectedIndex);
        }

        private void cbLightEnabled_CheckedChanged(object sender, EventArgs e)
        {
            EditorActions.UpdateLight<bool>((light, value) => light.Enabled == value, (light, value) => light.Enabled = value,
                light => cbLightEnabled.Checked);
        }

        private void cbLightIsObstructedByRoomGeometry_CheckedChanged(object sender, EventArgs e)
        {
            EditorActions.UpdateLight<bool>((light, value) => light.IsObstructedByRoomGeometry == value, (light, value) => light.IsObstructedByRoomGeometry = value,
                light => cbLightIsObstructedByRoomGeometry.Checked);
        }

        private void cbLightIsStaticallyUsed_CheckedChanged(object sender, EventArgs e)
        {
            EditorActions.UpdateLight<bool>((light, value) => light.IsStaticallyUsed == value, (light, value) => light.IsStaticallyUsed = value,
                light => cbLightIsStaticallyUsed.Checked);
        }

        private void cbLightIsDynamicallyUsed_CheckedChanged(object sender, EventArgs e)
        {
            EditorActions.UpdateLight<bool>((light, value) => light.IsDynamicallyUsed == value, (light, value) => light.IsDynamicallyUsed = value,
                light => cbLightIsDynamicallyUsed.Checked);
        }

        private void cbLightIsUsedForImportedGeometry_CheckedChanged(object sender, EventArgs e)
        {
            EditorActions.UpdateLight<bool>((light, value) => light.IsUsedForImportedGeometry == value, (light, value) => light.IsUsedForImportedGeometry = value,
                light => cbLightIsUsedForImportedGeometry.Checked);
        }

        private void cbLightCastsShadow_CheckedChanged(object sender, EventArgs e)
        {
            EditorActions.UpdateLight<bool>((light, value) => light.CastDynamicShadows == value, (light, value) => light.CastDynamicShadows = value,
                light => cbLightCastsShadow.Checked);
        }

        private static bool Compare(float firstValue, float secondValue, NumericUpDown control)
        {
            if (!control.Enabled)
                return true;

            for (int i = 0; i < control.DecimalPlaces; ++i)
            {
                firstValue *= 10.0f;
                secondValue *= 10.0f;
            }

            return Math.Round(firstValue) == Math.Round(secondValue);
        }

        private void numIntensity_ValueChanged(object sender, EventArgs e)
        {
            EditorActions.UpdateLight<float>((light, value) => Compare(light.Intensity, value, numIntensity),
                (light, value) => light.Intensity = value, light => (float)numIntensity.Value);
        }

        private void numInnerRange_ValueChanged(object sender, EventArgs e)
        {
            EditorActions.UpdateLight<float>((light, value) => Compare(light.InnerRange, value, numInnerRange),
                (light, value) => light.InnerRange = value, light => (float)numInnerRange.Value);
        }

        private void numOuterRange_ValueChanged(object sender, EventArgs e)
        {
            EditorActions.UpdateLight<float>((light, value) => Compare(light.OuterRange, value, numOuterRange),
                (light, value) => light.OuterRange = value, light => (float)numOuterRange.Value);
        }

        private void numInnerAngle_ValueChanged(object sender, EventArgs e)
        {
            EditorActions.UpdateLight<float>((light, value) => Compare(light.InnerAngle, value, numInnerAngle),
                 (light, value) => light.InnerAngle = value, light => (float)numInnerAngle.Value);
        }

        private void numOuterAngle_ValueChanged(object sender, EventArgs e)
        {
            EditorActions.UpdateLight<float>((light, value) => Compare(light.OuterAngle, value, numOuterAngle),
                 (light, value) => light.OuterAngle = value, light => (float)numOuterAngle.Value);
        }

        private void numDirectionY_ValueChanged(object sender, EventArgs e)
        {
            EditorActions.UpdateLight<float>((light, value) => Compare(light.RotationY, value, numDirectionY),
                 (light, value) => light.RotationY = value, light => (float)numDirectionY.Value);
        }

        private void numDirectionX_ValueChanged(object sender, EventArgs e)
        {
            EditorActions.UpdateLight<float>((light, value) => Compare(light.RotationX, value, numDirectionX),
                 (light, value) => light.RotationX = value, light => (float)numDirectionX.Value);
        }

        private void panelLightColor_Click(object sender, EventArgs e)
        {
            EditorActions.EditLightColor(this);
        }

        private void cmbLightQualityChanged(object sender, EventArgs e)
        {
            if (cmbLightQuality.Enabled && cmbLightQuality.SelectedIndex >= 0)
                EditorActions.UpdateLightQuality((LightQuality)cmbLightQuality.SelectedIndex);
        }

        private void butAddLight_Click(object sender, EventArgs e)
        {
            EditorActions.PlaceLight((LightType)cmbLightTypes.SelectedIndex);
        }

        private void cmbLightTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbLightTypes.SelectedIndex < 0)
                return;

            var selectedType = (LightType)cmbLightTypes.SelectedIndex;
            EditorActions.UpdateLight<LightType>(
                (light, value) => light.DisplayType == value,
                (light, value) =>
                {
                    if (value == LightType.HDR)
                    {
                        light.Type = LightType.Spot;
                        light.Quality = LightQuality.HDR;
                        light.IsStaticallyUsed = false;
                        light.IsUsedForImportedGeometry = false;
                    }
                    else
                    {
                        if (light.IsHDRLight)
                            light.Quality = LightQuality.Default;
                        light.Type = value;
                    }
                },
                light => selectedType);
        }
    }
}