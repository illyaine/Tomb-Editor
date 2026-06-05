# TEN Object OCB Exhaustive Object Checklist

## Purpose

The OCB source scan must cover every TEN object implementation, not only the first obvious door/switch/trap examples.

This checklist exists to prevent missing object-specific OCB codes such as waterfall emitters, effect emitters, legacy TR objects, special traps, puzzle objects and vehicle-specific behaviours.

## Required scan rule

Every source file under `TombEngine/Objects/**` must be searched for OCB-related logic before the provider is considered complete.

Do not treat the current manual scan report as complete. It is only an initial audit trail.

## Mandatory search terms

- `TriggerFlags`
- `TestOcb(`
- `RemoveOcb(`
- `ClearAllOcb(`
- `ItemFlags`
- `GetFlagField(`
- `TestFlagField(`
- `SetFlagField(`
- `ClearFlags(`
- numeric `case` labels near OCB/TriggerFlags logic
- negative OCB patterns
- bitwise OCB patterns using `&`

## Mandatory object groups

### Effects / emitters

Must include, at minimum by source discovery and not by guessing:

- Flame emitters
- Waterfall emitters
- Steam / smoke / mist emitters
- Electricity / arc emitters
- Ember / firefly / particle emitters
- Lens flare or light-related objects if they read `TriggerFlags`
- Any other file under `Objects/Effects/**`

### Generic objects

- Doors
- Switches
- Full-block / sequence switch logic
- Puzzle holes / key holes / puzzle done objects
- Trapdoors
- Bridge objects
- Pushables
- Ropes / poleropes / ziplines
- Generic animatings
- Generic traps
- Dart emitters
- Falling blocks / crumbling platforms
- Breakable walls
- Any other file under `Objects/Generic/**`

### Legacy engine groups

Each group must be scanned separately and kept separate in the resulting definitions:

- `Objects/TR1/**`
- `Objects/TR2/**`
- `Objects/TR3/**`
- `Objects/TR4/**`
- `Objects/TR5/**`

This includes enemies, vehicles, traps, special objects, boss logic and level-specific compatibility behaviours.

### Vehicles

Do not assume vehicles have no OCB just because one sampled file did not use `TriggerFlags`.

Scan all vehicle files under all legacy groups, including:

- TR2 vehicles
- TR3 vehicles
- any later vehicle/special movement object

### Cutscene / animating special cases

TR5-style hardcoded animating OCBs must be handled as their own group.

If TEN source marks something as TODO or placeholder, document it as `Unknown` or `Partial`; do not invent missing meanings.

## Required output fields per finding

For each finding, record:

- engine/source group: Generic, Effects, TR1, TR2, TR3, TR4, TR5, Core, Other
- file path
- function name if clear
- exact line or nearby code context
- numeric OCB evidence if present
- whether it is fixed value, range, signed value, bit flag or formula
- whether related `ItemFlags` are involved
- derived meaning only if directly supported by source
- `MappingStatus`: `Mapped`, `Partial` or `Unknown`

## Helper script

Use:

```powershell
pwsh docs/tools/ScanTenOcbSource.ps1 -TombEngineRoot "E:\Path\To\TombEngine" -OutputPath "docs/status/TEN_Object_OCB_SourceScan_Generated.md"
```

The generated report is evidence only. It still needs manual review before definitions are added to provider data.

## Provider rule

Do not add broad provider entries until object/slot assignment is reliable.

Preferred provider grouping:

- `ten.effects.*`
- `ten.generic.*`
- `ten.legacy.tr1.*`
- `ten.legacy.tr2.*`
- `ten.legacy.tr3.*`
- `ten.legacy.tr4.*`
- `ten.legacy.tr5.*`

This keeps legacy OCB behaviour visible and avoids mixing old TR4/TR5 behaviour with new TEN-specific parameters.
