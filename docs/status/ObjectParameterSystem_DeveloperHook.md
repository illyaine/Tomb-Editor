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

This allows a provider to convert saved object values into runtime metadata, Lua/Flow data or legacy OCB fallback entries without hardcoding that logic into the core object parameter system.

## Context

`ObjectParameterContext` gives the provider neutral object information:

- game version
- engine ID
- object type ID
- slot ID when available
- object key with script ID / room index / object index / slot ID

Providers should use this context to decide whether they support the selected object.

## Definition sets

A provider can return one or more `ObjectParameterDefinitionSet` instances.

A definition set contains:

- provider ID
- definition set ID
- display name and description
- object type / slot scope
- grouped parameter definitions
- presets
- export targets

Groups should keep normal level builder workflow simple:

- `Basic` for the most important fields
- `Advanced` for rarely used details
- `Compatibility` for OCB fallback / migration helpers
- `Runtime`, `Script`, `Diagnostics` only when needed

## Presets

Presets are data-only helpers. They should not execute logic.

Example use cases:

- common emitter profile
- common helper behavior
- compatibility preset with legacy OCB value
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
- legacy OCB fallback

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
