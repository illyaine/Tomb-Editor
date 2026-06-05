namespace TombLib.LevelData.ObjectParameters
{
    public static class ObjectParameterSupport
    {
        public static bool IsSupported(Level level)
        {
            return level != null && level.IsTombEngine;
        }
    }
}
