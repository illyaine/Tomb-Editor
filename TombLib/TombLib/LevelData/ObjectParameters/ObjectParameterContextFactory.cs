using TombLib.Wad.Catalog;

namespace TombLib.LevelData.ObjectParameters
{
    public static class ObjectParameterContextFactory
    {
        public static ObjectParameterContext FromObject(Level level, ObjectInstance instance)
        {
            var context = new ObjectParameterContext
            {
                GameVersion = level != null ? level.Settings.GameVersion : TRVersion.Game.TRNG,
                EngineId = level != null && level.IsTombEngine ? "TombEngine" : string.Empty,
                ObjectTypeId = instance != null ? instance.GetType().Name : string.Empty
            };

            if (instance is MoveableInstance moveable)
                context.SlotId = (int)moveable.WadObjectId.TypeId;
            else if (instance is StaticInstance stat)
                context.SlotId = (int)stat.WadObjectId.TypeId;

            if (instance != null)
                context.ObjectKey = ObjectParameterStorage.GetObjectKey(instance);

            return context;
        }
    }
}
