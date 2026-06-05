using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TombLib.LevelData.ObjectParameters
{
    public static class ObjectParameterStorage
    {
        private static readonly ConditionalWeakTable<ObjectInstance, ObjectParameterValueSet> _values = new ConditionalWeakTable<ObjectInstance, ObjectParameterValueSet>();

        public static bool HasValues(ObjectInstance instance)
        {
            if (instance == null)
                return false;

            if (!_values.TryGetValue(instance, out ObjectParameterValueSet valueSet))
                return false;

            return !string.IsNullOrEmpty(valueSet.DefinitionSetId) ||
                   !string.IsNullOrEmpty(valueSet.ProviderId) ||
                   !string.IsNullOrEmpty(valueSet.PresetId) ||
                   valueSet.Values.Count != 0;
        }

        public static ObjectParameterValueSet GetOrCreate(ObjectInstance instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            ObjectParameterValueSet valueSet = _values.GetValue(instance, obj => new ObjectParameterValueSet());
            UpdateObjectKey(valueSet, instance);
            return valueSet;
        }

        public static ObjectParameterValueSet TryGet(ObjectInstance instance)
        {
            if (instance == null)
                return null;

            if (!_values.TryGetValue(instance, out ObjectParameterValueSet valueSet))
                return null;

            UpdateObjectKey(valueSet, instance);
            return valueSet;
        }

        public static void Set(ObjectInstance instance, ObjectParameterValueSet valueSet)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            _values.Remove(instance);

            if (valueSet == null)
                return;

            ObjectParameterValueSet copy = Clone(valueSet);
            UpdateObjectKey(copy, instance);
            _values.Add(instance, copy);
        }

        public static void Clear(ObjectInstance instance)
        {
            if (instance == null)
                return;

            _values.Remove(instance);
        }

        public static ObjectParameterObjectKey GetObjectKey(ObjectInstance instance)
        {
            if (instance == null)
                return new ObjectParameterObjectKey();

            var valueSet = new ObjectParameterValueSet();
            UpdateObjectKey(valueSet, instance);
            return Clone(valueSet.ObjectKey);
        }

        public static ObjectParameterValueSet Clone(ObjectParameterValueSet valueSet)
        {
            if (valueSet == null)
                return null;

            return new ObjectParameterValueSet
            {
                ObjectKey = Clone(valueSet.ObjectKey),
                DefinitionSetId = valueSet.DefinitionSetId ?? string.Empty,
                ProviderId = valueSet.ProviderId ?? string.Empty,
                PresetId = valueSet.PresetId ?? string.Empty,
                Values = valueSet.Values?.Select(Clone).Where(value => value != null).ToList() ?? new List<ObjectParameterValue>()
            };
        }

        public static ObjectParameterValue Clone(ObjectParameterValue value)
        {
            if (value == null)
                return null;

            return new ObjectParameterValue
            {
                ParameterId = value.ParameterId ?? string.Empty,
                Value = value.Value ?? string.Empty
            };
        }

        private static ObjectParameterObjectKey Clone(ObjectParameterObjectKey key)
        {
            if (key == null)
                return new ObjectParameterObjectKey();

            return new ObjectParameterObjectKey
            {
                ScriptId = key.ScriptId,
                RoomIndex = key.RoomIndex,
                ObjectIndex = key.ObjectIndex,
                SlotId = key.SlotId,
                ObjectTypeId = key.ObjectTypeId ?? string.Empty
            };
        }

        private static void UpdateObjectKey(ObjectParameterValueSet valueSet, ObjectInstance instance)
        {
            if (valueSet.ObjectKey == null)
                valueSet.ObjectKey = new ObjectParameterObjectKey();

            valueSet.ObjectKey.ObjectTypeId = instance.GetType().Name;

            if (instance is IHasScriptID scriptObject)
                valueSet.ObjectKey.ScriptId = scriptObject.ScriptId;

            if (instance is MoveableInstance moveable)
                valueSet.ObjectKey.SlotId = (int)moveable.WadObjectId.TypeId;
            else if (instance is StaticInstance stat)
                valueSet.ObjectKey.SlotId = (int)stat.WadObjectId.TypeId;

            if (instance is PositionBasedObjectInstance positionBased && positionBased.Room != null)
            {
                Room room = positionBased.Room;
                Level level = room.Level;

                if (level != null)
                {
                    for (int i = 0; i < level.Rooms.Length; i++)
                        if (level.Rooms[i] == room)
                        {
                            valueSet.ObjectKey.RoomIndex = i;
                            break;
                        }
                }

                IReadOnlyList<PositionBasedObjectInstance> objects = room.Objects;
                for (int i = 0; i < objects.Count; i++)
                    if (objects[i] == instance)
                    {
                        valueSet.ObjectKey.ObjectIndex = i;
                        break;
                    }
            }
        }
    }
}
