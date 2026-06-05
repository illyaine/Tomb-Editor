using System.Collections.Generic;

namespace TombLib.LevelData.ObjectParameters
{
    public sealed class ObjectParameterExportContext
    {
        public ObjectParameterContext ObjectContext { get; set; } = new ObjectParameterContext();
        public ObjectParameterValueSet Values { get; set; } = new ObjectParameterValueSet();
        public ObjectParameterExportTarget Target { get; set; } = ObjectParameterExportTarget.RuntimeMetadata;
    }

    public sealed class ObjectParameterExportEntry
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public ObjectParameterExportTarget Target { get; set; } = ObjectParameterExportTarget.RuntimeMetadata;
    }

    public interface IObjectParameterExportProvider
    {
        IEnumerable<ObjectParameterExportEntry> Export(ObjectParameterExportContext context);
    }
}
