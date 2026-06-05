using System;
using System.Windows.Forms;
using TombLib.LevelData;

namespace TombEditor.Forms
{
    public partial class FormObjectParameters
    {
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK && !TryApplyRawOcbToObject(out string errorMessage))
            {
                MessageBox.Show(errorMessage, "Object Parameters", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                e.Cancel = true;
                return;
            }

            base.OnFormClosing(e);
        }

        private bool TryApplyRawOcbToObject(out string errorMessage)
        {
            errorMessage = string.Empty;

            string rawOcbValue = GetRawOcbValueFromGrid();
            if (string.IsNullOrWhiteSpace(rawOcbValue))
                return true;

            if (!short.TryParse(rawOcbValue.Trim(), out short ocb))
            {
                errorMessage = "OCB must be a signed 16-bit integer.";
                return false;
            }

            if (_instance is ItemInstance item)
                item.Ocb = ocb;
            else if (_instance is StaticInstance staticInstance)
                staticInstance.Ocb = ocb;

            return true;
        }

        private string GetRawOcbValueFromGrid()
        {
            foreach (DataGridViewRow row in gridValues.Rows)
            {
                if (row.IsNewRow)
                    continue;

                string parameterId = Convert.ToString(row.Cells["ParameterId"].Value)?.Trim() ?? string.Empty;
                if (string.Equals(parameterId, "ocb.raw", StringComparison.OrdinalIgnoreCase))
                    return Convert.ToString(row.Cells["Value"].Value)?.Trim() ?? string.Empty;
            }

            return string.Empty;
        }
    }
}
