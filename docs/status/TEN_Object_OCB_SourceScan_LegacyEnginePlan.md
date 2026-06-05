# TEN Object OCB Source Scan - Legacy Engine Expansion Plan

## Correction after first scan

The first scan only captured a conservative set of directly inspected TEN object behaviours. It is not a complete TEN OCB catalogue.

TEN contains many legacy object implementations under engine-specific object groups such as TR1, TR2, TR3, TR4 and TR5. These must be scanned as separate source categories instead of being merged into one generic TEN bucket.

## Required category model

Future ObjectParameter definitions should preserve both dimensions:

- Runtime source: TEN source code
- Behaviour family: Generic / TEN-specific / TR1 / TR2 / TR3 / TR4 / TR5 / Legacy compatibility

This prevents old TR4/TR5 compatibility OCBs from looking like newly invented TEN parameters and prevents TEN-specific object parameters from being mixed into legacy object behaviour.

## Required scan groups

### Generic / shared TEN objects

Examples:

- `Objects/Generic/**`
- `Objects/Effects/**`
- shared doors, switches, emitters, traps, puzzle/key objects, bridge objects, animatings

### TR1 source group

Examples:

- `Objects/TR1/**`
- TR1 enemies, traps and special objects

### TR2 source group

Examples:

- `Objects/TR2/**`
- TR2 enemies, traps, vehicles and special objects

### TR3 source group

Examples:

- `Objects/TR3/**`
- TR3 enemies, traps, vehicles and special objects

### TR4 source group

Examples:

- `Objects/TR4/**`
- TR4 classic objects, puzzle logic, traps, emitters and compatibility behaviours

### TR5 source group

Examples:

- `Objects/TR5/**`
- TR5 animating special cases, cutscene-era objects, trap and effect behaviours

## Scan patterns

Each source group must be searched for:

- `TriggerFlags`
- `TestOcb(`
- `RemoveOcb(`
- `ClearAllOcb(`
- comparisons against literal OCB values
- switch statements using `TriggerFlags`
- bitwise checks using `TriggerFlags & value`
- signed OCB patterns such as negative values for alternate behaviour

`ItemFlags` / `GetFlagField(...)` / `TestFlagField(...)` must still be recorded separately and must not be mixed into OCB entries.

## Provider impact

The provider should not expose one flat `TEN` list.

Preferred structure:

- `TenBuiltInObjectParameterProvider`
  - Generic / shared TEN definitions
  - TR1 legacy definitions
  - TR2 legacy definitions
  - TR3 legacy definitions
  - TR4 legacy definitions
  - TR5 legacy definitions
  - TEN-specific definitions

Each `ObjectParameterDefinitionSet` should carry a clear `ObjectTypeId` and should use naming that makes the source family visible, for example:

- `ten.generic.doors.ocb`
- `ten.legacy.tr4.flameEmitter2.ocb`
- `ten.legacy.tr5.animating.ocb`
- `ten.legacy.tr2.vehicle.speedboat.ocb`

## Status

This is a correction and expansion plan for the next scan pass. The current source-scan report remains valid as a partial audit trail, but it must not be treated as complete.
