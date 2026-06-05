using System.Collections.Generic;

namespace TombLib.LevelData.ObjectParameters
{
    public sealed class ObjectParameterOverviewEntry
    {
        public ObjectInstance Object { get; set; }
        public ObjectParameterObjectKey ObjectKey { get; set; } = new ObjectParameterObjectKey();
        public string ObjectName { get; set; } = string.Empty;
        public string ProviderId { get; set; } = string.Empty;
        public string DefinitionSetId { get; set; } = string.Empty;
        public string PresetId { get; set; } = string.Empty;
        public int ValueCount { get; set; }
        public List<ObjectParameterValidationMessage> ValidationMessages { get; set; } = new List<ObjectParameterValidationMessage>();
    }
}
