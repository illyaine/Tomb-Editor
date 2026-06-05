using TombLib.LevelData.ObjectParameters;

namespace TombEditor.Forms
{
    public partial class FormObjectParameters
    {
        static FormObjectParameters()
        {
            if (ObjectParameterProviderRegistry.FindProvider(TenReviewedOcbObjectParameterProvider.ProviderId) == null)
                ObjectParameterProviderRegistry.Register(new TenReviewedOcbObjectParameterProvider());
        }
    }
}
