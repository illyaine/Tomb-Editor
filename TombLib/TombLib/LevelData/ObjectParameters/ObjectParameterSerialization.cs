using System;
using System.Collections.Generic;
using System.IO;

namespace TombLib.LevelData.ObjectParameters
{
    public static class ObjectParameterSerialization
    {
        private const int FormatVersion = 1;

        public static void Write(BinaryWriter writer, ObjectParameterValueSet valueSet)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            valueSet = ObjectParameterStorage.Clone(valueSet) ?? new ObjectParameterValueSet();

            writer.Write(FormatVersion);
            writer.Write(valueSet.ProviderId ?? string.Empty);
            writer.Write(valueSet.DefinitionSetId ?? string.Empty);
            writer.Write(valueSet.PresetId ?? string.Empty);

            writer.Write(valueSet.Values?.Count ?? 0);
            if (valueSet.Values != null)
                foreach (ObjectParameterValue value in valueSet.Values)
                {
                    writer.Write(value.ParameterId ?? string.Empty);
                    writer.Write(value.Value ?? string.Empty);
                }
        }

        public static ObjectParameterValueSet Read(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            int version = reader.ReadInt32();
            if (version != FormatVersion)
                throw new InvalidDataException("Unsupported object parameter data version.");

            var valueSet = new ObjectParameterValueSet
            {
                ProviderId = reader.ReadString(),
                DefinitionSetId = reader.ReadString(),
                PresetId = reader.ReadString(),
                Values = new List<ObjectParameterValue>()
            };

            int valueCount = reader.ReadInt32();
            for (int i = 0; i < valueCount; i++)
            {
                valueSet.Values.Add(new ObjectParameterValue
                {
                    ParameterId = reader.ReadString(),
                    Value = reader.ReadString()
                });
            }

            return valueSet;
        }
    }
}
