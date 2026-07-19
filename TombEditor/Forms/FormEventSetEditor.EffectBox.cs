using System;
using System.Windows.Forms;
using TombLib.LevelData;

namespace TombEditor.Forms
{
    public partial class FormEventSetEditor
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_instance == null || !_instance.IsEffectBox())
                return;

            BeginInvoke(new Action(OpenEffectBoxEditor));
        }

        private void OpenEffectBoxEditor()
        {
            if (IsDisposed || _instance == null || !_instance.IsEffectBox())
                return;

            var owner = Owner as IWin32Window;
            Hide();

            using (var form = new FormEffectBoxEditor(_instance))
                DialogResult = form.ShowDialog(owner);

            Close();
        }
    }
}
