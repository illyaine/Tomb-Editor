using System.Collections.Generic;
using TombLib.Wad.Catalog;

namespace TombLib.LevelData.ObjectParameters
{
    public enum ObjectParameterMessageSeverity
    {
        Info,
        Warning,
        Error
    }

    public sealed class ObjectParameterContext
    {
        public TRVersion.Game GameVersion { get; set; }
        public string EngineId { get; set; } = string.Empty;
        public int? SlotId { get; set; }
        public string ObjectTypeId { get; set; } = string.Empty;
        public ObjectParameterObjectKey ObjectKey { get; set; } = new ObjectParameterObjectKey();
    }

    public sealed class ObjectParameterValidationMessage
    {
        public ObjectParameterMessageSeverity Severity { get; set; } = ObjectParameterMessageSeverity.Info;
        public string ParameterId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public interface IObjectParameterProvider
    {
        string Id { get; }
        string Name { get; }

        IEnumerable<ObjectParameterDefinitionSet> GetDefinitionSets(ObjectParameterContext context);
        IEnumerable<ObjectParameterValidationMessage> Validate(ObjectParameterContext context, ObjectParameterValueSet values);
    }
}
