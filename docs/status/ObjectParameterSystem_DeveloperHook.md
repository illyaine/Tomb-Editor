# Object Parameter System - Developer Hook

## Scope

The Object Parameter System is a neutral editor-side extension point for TEN object settings.
It is not an atmosphere system and it must not contain atmosphere-specific classes, IDs or hardcoded behavior.

Atmosphere, sound, particles, AI helpers, camera helpers, traps or future plugin logic can hook into the same system by registering their own provider.

## TEN-only editor visibility

The Tomb Editor UI should expose object parameter editing only when the current level uses TombEngine/TEN.
Other engines should not show the Object Parameters action because the runtime/export side cannot be guaranteed there.

Current guard:

- `ObjectParameterSupport.IsSupported(Level level)`
- `MaterialObjectContextMenu` only adds `Object parameters...` when the level is TEN.

The core data/contracts stay neutral so that provider code remains independent and reusable.

## Provider registration

A developer system should register one provider at startup or module initialization:

```csharp
ObjectParameterProviderRegistry.Register(new MyObjectParameterProvider());
```

Providers must implement:

```csharp
IObjectParameterProvider
```

Required members:

- `Id`
- `Name`
- `GetDefinitionSets(ObjectParameterContext context)`
- `Validate(ObjectParameterContext context, ObjectParameterValueSet values)`

Optional export hook:

```csharp
IObjectParameterExportProvider
```

This allows a provider to convert saved object values into runtime metadata, Lua/Flow data or OCB fallback entries without hardcoding that logic into the core object parameter system.

## Context

`ObjectParameterContext` gives the provider neutral object information:

- game version
- engine ID
- object type ID
- slot ID when available
- object key with script ID / Lua name / room index / object index / slot ID / current OCB

Providers should use this context to decide whether they support the selected object.

## Definition sets

A provider can return one or more `ObjectParameterDefinitionSet` instances.

A definition set contains:

- provider ID
- definition set ID
- display name and description
- source marker: OCB, TEN, Script or Custom
- mapping status: unknown, partial or mapped
- object type / slot scope
- grouped parameter definitions
- object-specific OCB definitions
- presets
- export targets

Groups should keep normal level builder workflow simple:

- `Basic` for the most important fields
- `Advanced` for rarely used details
- `Compatibility` for OCB fallback / migration helpers
- `Runtime`, `Script`, `Diagnostics` only when needed

## Source metadata

Every row can carry source metadata:

- `ObjectParameterSource.Ocb` -> old OCB compatibility / existing OCB codes
- `ObjectParameterSource.Ten` -> official TEN parameter
- `ObjectParameterSource.Script` -> script-facing option
- `ObjectParameterSource.Custom` -> external provider / plugin parameter
- `ObjectParameterSource.Unknown` -> not classified yet

Every row also has `ObjectParameterMappingStatus`:

- `Unknown`: not documented or not safely mapped
- `Partial`: partially known but incomplete or risky
- `Mapped`: documented enough to expose as normal option

Unknown values must never be deleted or silently overwritten. They should remain visible as raw values.

## OCB definitions

OCB definitions are object-specific and belong to a provider definition set through:

```csharp
ObjectParameterDefinitionSet.OcbDefinitions
```

Each `ObjectParameterOcbDefinition` can define:

- OCB value
- name
- description
- group
- example
- combinable flag
- OCB mode: fixed value, additive flags or mixed
- mapping status
- warning

Do not guess OCB meanings. Only add confirmed meanings. If the meaning is uncertain, use `MappingStatus = Unknown` or `Partial` and provide a warning.

The object parameter dialog has a compact "Show existing OCB codes" button. It displays the definition-set OCB entries for the current object. If the current raw OCB value has no matching definition, it is shown as unknown / undocumented and preserved.

## TEN parameters

Official TEN parameters should be added as normal parameter definitions with:

```csharp
Source = ObjectParameterSource.Ten
```

They should not be mixed into OCB definitions unless they intentionally export to `LegacyOcb`.

## Presets

Presets are data-only helpers. They should not execute logic.

Example use cases:

- common emitter profile
- common helper behavior
- compatibility preset with OCB value
- quick setup for default runtime metadata

## Validation

Validation should be provider-owned.

The core only collects and displays validation messages. It should not know atmosphere, sound, particle or AI-specific rules.

Validation can return:

- `Info`
- `Warning`
- `Error`

The current dialog blocks saving on errors.

## Export

Export should stay provider-owned.

The neutral export entry model is intentionally simple:

- key
- value
- target

Later exporter layers can map these entries to:

- runtime metadata
- Flow
- Lua
- OCB fallback

The saved values already carry source and mapping metadata so exporters can keep TE display data separate from TEN runtime data.

## Atmosphere integration rule

Atmosphere must be implemented as a separate provider, not inside the core system.

Expected later structure:

- atmosphere provider registers itself through `ObjectParameterProviderRegistry.Register(...)`
- atmosphere provider returns its own definition sets and presets
- atmosphere provider validates atmosphere-specific values
- atmosphere provider exports runtime metadata / script entries as needed

The Object Parameter System itself must remain neutral.

## PRJ2 persistence

The object parameter values must be saved per object instance in `.prj2`.

This part is intentionally still separate because it touches large PRJ2 writer/loader files and should be patched with full file control.

Required later files:

- `Prj2Chunks.cs`
- `Prj2Writer.cs`
- `Prj2Loader.cs`

Persistence target:

- values are stored per object ID, not globally
- loaded values are attached back to the matching object instance
- no engine-specific parameter interpretation during loading
- raw OCB values and unknown mapping status remain preserved
