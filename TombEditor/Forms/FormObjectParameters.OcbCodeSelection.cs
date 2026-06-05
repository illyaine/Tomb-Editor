using System;
using System.Windows.Forms;
using TombLib.LevelData.ObjectParameters;

namespace TombEditor.Forms
{
    public partial class FormObjectParameters
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            butOcbCodes.Click -= butShowOcbCodes_Click;
            butOcbCodes.Click += butShowOcbCodesApply_Click;
        }

        private void butShowOcbCodesApply_Click(object sender, EventArgs e)
        {
            using (var form = new FormObjectOcbCodes(_instance, SelectedDefinitionSet))
            {
                if (form.ShowDialog(this) != DialogResult.OK || !form.SelectedOcbValue.HasValue)
                    return;

                ObjectParameterDefinition parameter = FindParameterDefinition("ocb.raw");
                ObjectParameterSource source = SelectedDefinitionSet?.Source ?? ObjectParameterSource.Ocb;
                ObjectParameterMappingStatus status = form.SelectedMappingStatus;

                AddOrUpdateValueRow("ocb.raw", form.SelectedOcbValue.Value.ToString(), parameter, source, status);

                ObjectParameterDefinitionSet definitionSet = SelectedDefinitionSet;
                if (definitionSet != null)
                {
                    textProviderId.Text = definitionSet.ProviderId;
                    textDefinitionSetId.Text = definitionSet.Id;
                    textPresetId.Text = string.Empty;
                    comboPresets.SelectedIndex = comboPresets.Items.Count > 0 ? 0 : -1;
                }

                labelObject.Text = GetObjectCaption();
                UpdateHelpPanel();
            }
        }
    }
}
