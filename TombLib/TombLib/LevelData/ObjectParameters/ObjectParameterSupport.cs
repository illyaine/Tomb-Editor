using TombLib.LevelData;

namespace TombLib.LevelData.ObjectParameters
{
    public static class ObjectParameterSupport
    {
        public static bool IsSupported(Level level)
        {
            return level != null;
        }

        public static bool IsSupported(Level level, ObjectInstance instance)
        {
            if (level == null || instance == null)
                return false;

            if (level.IsTombEngine)
                return true;

            return instance is ItemInstance;
        }

        public static bool SupportsRuntimeParameters(Level level)
        {
            return level != null && level.IsTombEngine;
        }
    }
}
