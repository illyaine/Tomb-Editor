using System.Collections.Generic;

namespace TombLib.LevelData.ObjectParameters
{
    public sealed class ObjectParameterObjectKey
    {
        public int? ScriptId { get; set; }
        public int? RoomIndex { get; set; }
        public int? ObjectIndex { get; set; }
        public int? SlotId { get; set; }
        public string ObjectTypeId { get; set; } = string.Empty;

        public bool IsEmpty => ScriptId == null && RoomIndex == null && ObjectIndex == null && SlotId == null && string.IsNullOrEmpty(ObjectTypeId);
    }

    public sealed class ObjectParameterValueSet
    {
        public ObjectParameterObjectKey ObjectKey { get; set; } = new ObjectParameterObjectKey();
        public string DefinitionSetId { get; set; } = string.Empty;
        public string ProviderId { get; set; } = string.Empty;
        public string PresetId { get; set; } = string.Empty;
        public List<ObjectParameterValue> Values { get; set; } = new List<ObjectParameterValue>();
    }

    public sealed class ObjectParameterValue
    {
        public string ParameterId { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public sealed class ObjectParameterPreset
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? LegacyOcbValue { get; set; }
        public List<ObjectParameterValue> Values { get; set; } = new List<ObjectParameterValue>();
    }
}
