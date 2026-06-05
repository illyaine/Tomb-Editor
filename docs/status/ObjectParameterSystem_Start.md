# Object Parameter System - Start

## Goal

The Object Parameter System is intended as a generic, future-proof replacement path for object-specific legacy OCB usage. It must not become an "OCB Editor". OCB remains only a compatibility fallback or quick-preset output for legacy objects and old engine behavior.

The system should allow Tomb Editor, TombEngine and future plugins/providers to expose object-specific settings in a consistent editor workflow.

## Scope

The system should support object-specific configuration for cases such as:

- Atmosphere emitters
- Sound emitters
- Light helpers
- Particle sources
- Trap and puzzle object settings
- AI helpers
- Camera helpers
- Runtime metadata driven helper objects
- Future plugin/provider object logic

Settings are per object instance, identified by a stable object id where possible. The system is not meant to be a global level setting store.

## Placement decision

Initial neutral contracts and value models belong in `TombLib`, because they should be shared by Tomb Editor UI, scripting/export code and later external providers without depending on editor UI classes.

Planned layering:

- `TombLib`: neutral definitions, value sets, presets, validation messages and provider contracts.
- `TombLib.Forms`: later reusable Tomb Editor style parameter controls/dialog helpers.
- `TombEditor`: later context menu entry, object selection integration, level overview page and command wiring.
- `TombLib.Scripting.*` / TombIDE integration: later export adapters for Flow, Lua or script metadata if needed.
- Runtime/export layer: later metadata export for TombEngine or other engines.

## Editor workflow target

Main workflow:

1. Builder right-clicks an object instance in the editor.
2. Context menu shows `Object Parameters` when the selected object has definitions or stored values.
3. Tomb Editor opens a TE-style parameter window/panel.
4. The window shows simple grouped settings first.
5. Advanced groups are collapsed by default.
6. Presets can fill several values at once.
7. Validation messages explain missing or conflicting values.
8. Export can produce Flow/Lua/runtime metadata or legacy OCB fallback depending on the provider/object.

A separate overview page should list all level objects that have parameter values. It should support filters by effect/type, provider, object slot and validation status. This is useful for large levels where helper objects are spread across many rooms.

## Extensibility target

Developers should be able to plug in object parameter definitions for their own objects. The preferred model is a small provider contract that returns definition sets by object kind, slot, game version or engine context.

Provider responsibilities:

- Provide stable provider id.
- Provide object/slot-specific parameter definitions.
- Provide presets.
- Validate parameter values.
- Optionally declare export targets such as Flow, Lua, runtime metadata or legacy OCB fallback.

The core data model should stay simple and serializable. Avoid heavy service infrastructure until Tomb Editor integration requires it.

## NodeEditor integration target

The Object Parameter System should be usable by the NodeEditor later. Possible integration points:

- Nodes can read parameter values from selected/linked object instances.
- Object parameter presets can create NodeEditor templates.
- Validation can warn when a node expects an object parameter that does not exist.
- Export adapters can share metadata between Object Parameters and NodeEditor generated logic.

This should remain a docking point for now. No NodeEditor dependency should be added to the initial TombLib models.

## Legacy OCB handling

OCB stays supported as compatibility output, not as the central authoring model.

Recommended handling:

- Definitions may declare that a parameter or preset can export to legacy OCB.
- Presets may carry a legacy OCB fallback value.
- UI should label this as compatibility/fallback behavior, not as the primary system.
- New systems should prefer runtime metadata or scripting export over hard-coded OCB values.

## First implementation step in this branch

Added neutral TombLib contracts/models under:

`TombLib/TombLib/LevelData/ObjectParameters/`

Initial files:

- `ObjectParameterDefinitions.cs`
- `ObjectParameterValues.cs`
- `ObjectParameterProvider.cs`

These files are intentionally UI-free and do not integrate with the editor yet. They are a safe foundation for the next step.

## Next recommended steps

1. Locate the existing object context-menu and property editing code in TombEditor.
2. Add a disabled/hidden integration point for `Object Parameters` without changing existing OCB behavior.
3. Add a minimal built-in provider with one harmless test definition for a helper object only after the UI path is known.
4. Add serialization strategy after inspecting TE level file save/load code, so stored values are not lost.
5. Add the level overview page after instance storage and provider lookup are stable.

## Additional ideas

- Add a search field inside the Object Parameters window for advanced objects.
- Show a small compatibility badge when a value exports to OCB.
- Add provider/version compatibility warnings so old projects can still open safely.
- Add a read-only raw export preview for advanced users.
- Allow definitions to declare editor display hints, but keep actual UI styling inside TombEditor/TombLib.Forms.
- Allow parameter groups to be marked as `Advanced`, `Compatibility`, `Runtime`, `Script` or `Diagnostics` later if needed.
