using System;
using System.Collections.Generic;
using System.IO;

namespace TombLib.LevelData.ObjectParameters
{
    public static class ObjectParameterSerialization
    {
        private const int FormatVersion = 2;

        public static void Write(BinaryWriter writer, ObjectParameterValueSet valueSet)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            valueSet = ObjectParameterStorage.Clone(valueSet) ?? new ObjectParameterValueSet();

            writer.Write(FormatVersion);
            WriteObjectKey(writer, valueSet.ObjectKey);
            writer.Write(valueSet.ProviderId ?? string.Empty);
            writer.Write(valueSet.DefinitionSetId ?? string.Empty);
            writer.Write(valueSet.PresetId ?? string.Empty);

            writer.Write(valueSet.Values?.Count ?? 0);
            if (valueSet.Values != null)
                foreach (ObjectParameterValue value in valueSet.Values)
                {
                    writer.Write(value.ParameterId ?? string.Empty);
                    writer.Write(value.Value ?? string.Empty);
                    writer.Write((int)value.Source);
                    writer.Write((int)value.MappingStatus);
                }
        }

        public static ObjectParameterValueSet Read(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            int version = reader.ReadInt32();
            if (version < 1 || version > FormatVersion)
                throw new InvalidDataException("Unsupported object parameter data version.");

            var valueSet = new ObjectParameterValueSet
            {
                ObjectKey = version >= 2 ? ReadObjectKey(reader) : new ObjectParameterObjectKey(),
                ProviderId = reader.ReadString(),
                DefinitionSetId = reader.ReadString(),
                PresetId = reader.ReadString(),
                Values = new List<ObjectParameterValue>()
            };

            int valueCount = reader.ReadInt32();
            for (int i = 0; i < valueCount; i++)
            {
                var value = new ObjectParameterValue
                {
                    ParameterId = reader.ReadString(),
                    Value = reader.ReadString()
                };

                if (version >= 2)
                {
                    value.Source = (ObjectParameterSource)reader.ReadInt32();
                    value.MappingStatus = (ObjectParameterMappingStatus)reader.ReadInt32();
                }

                valueSet.Values.Add(value);
            }

            return valueSet;
        }

        private static void WriteObjectKey(BinaryWriter writer, ObjectParameterObjectKey key)
        {
            key ??= new ObjectParameterObjectKey();

            WriteNullableUInt(writer, key.ScriptId);
            WriteNullableInt(writer, key.RoomIndex);
            WriteNullableInt(writer, key.ObjectIndex);
            WriteNullableInt(writer, key.SlotId);
            WriteNullableShort(writer, key.Ocb);
            writer.Write(key.LuaName ?? string.Empty);
            writer.Write(key.ObjectTypeId ?? string.Empty);
        }

        private static ObjectParameterObjectKey ReadObjectKey(BinaryReader reader)
        {
            return new ObjectParameterObjectKey
            {
                ScriptId = ReadNullableUInt(reader),
                RoomIndex = ReadNullableInt(reader),
                ObjectIndex = ReadNullableInt(reader),
                SlotId = ReadNullableInt(reader),
                Ocb = ReadNullableShort(reader),
                LuaName = reader.ReadString(),
                ObjectTypeId = reader.ReadString()
            };
        }

        private static void WriteNullableInt(BinaryWriter writer, int? value)
        {
            writer.Write(value.HasValue);
            if (value.HasValue)
                writer.Write(value.Value);
        }

        private static int? ReadNullableInt(BinaryReader reader)
        {
            return reader.ReadBoolean() ? reader.ReadInt32() : null;
        }

        private static void WriteNullableUInt(BinaryWriter writer, uint? value)
        {
            writer.Write(value.HasValue);
            if (value.HasValue)
                writer.Write(value.Value);
        }

        private static uint? ReadNullableUInt(BinaryReader reader)
        {
            return reader.ReadBoolean() ? reader.ReadUInt32() : null;
        }

        private static void WriteNullableShort(BinaryWriter writer, short? value)
        {
            writer.Write(value.HasValue);
            if (value.HasValue)
                writer.Write(value.Value);
        }

        private static short? ReadNullableShort(BinaryReader reader)
        {
            return reader.ReadBoolean() ? reader.ReadInt16() : null;
        }
    }
}
