using System.Collections.Generic;

namespace TombLib.LevelData.ObjectParameters
{
    public enum ObjectParameterType
    {
        Boolean,
        Integer,
        Float,
        String,
        Enum,
        Color,
        Vector2,
        Vector3
    }

    public enum ObjectParameterExportTarget
    {
        None,
        RuntimeMetadata,
        Flow,
        Lua,
        LegacyOcb
    }

    public enum ObjectParameterGroupKind
    {
        Basic,
        Advanced,
        Compatibility,
        Runtime,
        Script,
        Diagnostics
    }

    public sealed class ObjectParameterDefinitionSet
    {
        public string Id { get; set; } = string.Empty;
        public string ProviderId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ObjectTypeId { get; set; } = string.Empty;
        public int? SlotId { get; set; }
        public List<ObjectParameterGroup> Groups { get; set; } = new List<ObjectParameterGroup>();
        public List<ObjectParameterPreset> Presets { get; set; } = new List<ObjectParameterPreset>();
        public List<ObjectParameterExportTarget> ExportTargets { get; set; } = new List<ObjectParameterExportTarget>();
    }

    public sealed class ObjectParameterGroup
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ObjectParameterGroupKind Kind { get; set; } = ObjectParameterGroupKind.Basic;
        public bool IsCollapsedByDefault { get; set; }
        public List<ObjectParameterDefinition> Parameters { get; set; } = new List<ObjectParameterDefinition>();
    }

    public sealed class ObjectParameterDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ObjectParameterType Type { get; set; } = ObjectParameterType.String;
        public string DefaultValue { get; set; } = string.Empty;
        public string MinimumValue { get; set; } = string.Empty;
        public string MaximumValue { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public bool IsReadOnly { get; set; }
        public List<ObjectParameterOption> Options { get; set; } = new List<ObjectParameterOption>();
        public List<ObjectParameterExportTarget> ExportTargets { get; set; } = new List<ObjectParameterExportTarget>();
    }

    public sealed class ObjectParameterOption
    {
        public string Value { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
