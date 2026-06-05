# TEN Object OCB Source Scan

## Scope

This document records the first focused source scan for TEN object OCB and object-parameter behaviour that can later feed Tomb Editor's neutral Object Parameter System.

TE branch: `object_parameter_ocb_scan`

TE base branch: `object_parameter_system`

TEN source branch: `illyaine/TombEngine` / `master`

The scan is intentionally conservative. It records only behaviour that is directly visible in TEN source code or in source comments. TRNG TXT data is not imported here.

## Rules used for this pass

- `TriggerFlags` is treated as the OCB source field.
- `ItemInfo::TestOcb(short ocbFlags)` is treated as a bitwise all-bits OCB test: `(TriggerFlags & ocbFlags) == ocbFlags`.
- `ItemInfo::RemoveOcb(short ocbFlags)` removes OCB bits from `TriggerFlags`.
- `ItemInfo::ClearAllOcb()` clears `TriggerFlags`.
- `ItemFlags`, `TestFlags`, `TestFlagField`, `GetFlagField`, `SetFlagField` and `ClearFlags` are recorded only as related implementation details. They must not be mixed into OCB definitions.
- If the source only shows that a value participates in a formula or lookup, the mapping is marked `Partial` unless the value meaning is fully clear.

## Source files inspected in this pass

- `TombEngine/Game/items.cpp`
- `TombEngine/Objects/Generic/Doors/generic_doors.cpp`
- `TombEngine/Objects/Generic/Doors/sequence_door.cpp`
- `TombEngine/Objects/Generic/Doors/underwater_door.cpp`
- `TombEngine/Objects/Generic/Switches/generic_switch.cpp`
- `TombEngine/Objects/Generic/Switches/cog_switch.cpp`
- `TombEngine/Objects/Generic/Switches/fullblock_switch.cpp`
- `TombEngine/Objects/Generic/Traps/dart_emitter.cpp`
- `TombEngine/Objects/Generic/Traps/falling_block.cpp`
- `TombEngine/Objects/Generic/Object/objects.cpp`
- `TombEngine/Objects/Effects/flame_emitters.cpp`
- `TombEngine/Objects/Generic/puzzles_keys.cpp`
- `TombEngine/Objects/TR2/Vehicles/skidoo.cpp`
- `TombEngine/Objects/TR2/Vehicles/speedboat.cpp`

## Findings

| Area | Source file | Function | OCB / TriggerFlags evidence | Derived editor meaning | MappingStatus | Notes |
| --- | --- | --- | --- | --- | --- | --- |
| Core | `Game/items.cpp` | `ItemInfo::TestOcb` | `(TriggerFlags & ocbFlags) == ocbFlags` | OCB bit test helper. | Mapped | This is the basis for future bitwise scans. |
| Core | `Game/items.cpp` | `ItemInfo::RemoveOcb` | `TriggerFlags &= ~ocbFlags` | Removes OCB bit flags from the item OCB field. | Mapped | Do not confuse with `ItemFlags`. |
| Core | `Game/items.cpp` | `ItemInfo::ClearAllOcb` | `TriggerFlags = 0` | Clears all OCB data. | Mapped | Do not expose as a normal object preset. |
| Core | `Game/items.cpp` | `TestFlags` / `TestFlagField` / `GetFlagField` | Operates on `ItemFlags[id]` | Runtime item flag fields, not OCB. | Mapped | Kept separate for later object-parameter work. |
| Generic doors | `Objects/Generic/Doors/generic_doors.cpp` | `DoorCollision` | `doorItem.TriggerFlags == 2` | Door can be opened with the crowbar. | Mapped | Uses inventory check for `ID_CROWBAR_ITEM`. |
| Generic doors | `Objects/Generic/Doors/generic_doors.cpp` | `DoorControl` | Source comment: `Doors with OCB = 1 are raisable with cog switchs`; code checks `doorItem.TriggerFlags == 1` | Door is raised/lowered by cog switch logic. | Mapped | The comment has typo `switchs` in source; keep editor text clean. |
| Sequence door | `Objects/Generic/Doors/sequence_door.cpp` | `SequenceDoorControl` | `SequenceResults[Sequences[0]][Sequences[1]][Sequences[2]] == doorItem->TriggerFlags` | Door OCB must match the computed 3-step sequence result. | Partial | Fixed values are not self-contained in this file; requires sequence-result table origin. |
| Underwater door | `Objects/Generic/Doors/underwater_door.cpp` | `UnderwaterDoorCollision` | No `TriggerFlags` usage found in inspected file. | No TEN OCB mapping found here. | Unknown | Leave without built-in OCB entries. |
| Generic switch | `Objects/Generic/Switches/generic_switch.cpp` | `SwitchOCBs` enum | `0..7` enum values | Switch OCB selects the switch interaction type. | Mapped | Values are listed below in provider candidates. |
| Generic switch | `Objects/Generic/Switches/generic_switch.cpp` | `SwitchCollision` | `TriggerFlags == 3 || TriggerFlags == 4` disabled when off | Big button / giant button have extra disabled/off behaviour. | Mapped | Behaviour-specific note for values 3 and 4. |
| Generic switch | `Objects/Generic/Switches/generic_switch.cpp` | `SwitchCollision` | `TriggerFlags != 3 && isUnderwater` returns | Only OCB 3 is currently accepted by this generic underwater path. | Partial | Source has a TODO saying temporary code. |
| Generic switch | `Objects/Generic/Switches/generic_switch.cpp` | `SwitchCollision` | `case SWT_CUSTOM` uses `ItemFlags[4]`, `[5]`, `[6]` | OCB 7 uses custom animation/position data stored in ItemFlags. | Partial | OCB value is known; related ItemFlags need a separate parameter model. |
| Generic switch | `Objects/Generic/Switches/generic_switch.cpp` | `SwitchCollision` | `default: onAnim = TriggerFlags; offAnim = TriggerFlags + 1` | Non-enum OCB can act as direct Lara animation pair base. | Partial | Needs UI warning; not a normal fixed OCB list. |
| Cog switch | `Objects/Generic/Switches/cog_switch.cpp` | `CogSwitchCollision` / `CogSwitchControl` | `!switchItem->TriggerFlags` gates built-in door coupling | OCB 0 uses built-in cog-door coupling when the triggered item is a door. | Mapped | Non-zero bypasses this built-in coupling; exact non-zero meanings not identified here. |
| Full-block switch | `Objects/Generic/Switches/fullblock_switch.cpp` | `FullBlockSwitchControl` | `Sequences[CurrentSequence++] = switchItem->TriggerFlags` | OCB contributes one value to a 3-step sequence. | Partial | Used with `sequence_door.cpp`; actual result matrix is separate. |
| Dart emitter / dart | `Objects/Generic/Traps/dart_emitter.cpp` | `ControlDart` | `TriggerFlags < 0` adds poison; damage is `TriggerFlags ? abs(TriggerFlags) : 25` | OCB 0 uses default damage 25; positive OCB is damage; negative OCB is damage plus poison. | Mapped | Dart emitter copies its OCB to the spawned dart. |
| Dart emitter | `Objects/Generic/Traps/dart_emitter.cpp` | `InitializeDartEmitter` | `ItemFlags[0]` is delay, `ItemFlags[1]` is timer | Delay/timer are ItemFlags, not OCB. | Mapped | Keep as separate advanced parameters later. |
| Falling block | `Objects/Generic/Traps/falling_block.cpp` | `FallingBlockCollision` / `FallingBlockControl` | Auto-trigger only when `!TriggerFlags`; otherwise `TriggerFlags--` each frame | OCB 0 allows floor-contact activation; positive OCB acts as a countdown before crumble/fall logic. | Mapped | Unit appears to be frames from direct decrement. |
| Generic animating | `Objects/Generic/Object/objects.cpp` | `ControlAnimatingSlots` | Source TODO: TR5 has hardcoded OCB codes; placeholder | TR5 hardcoded animating OCBs are not implemented here. | Unknown | Do not invent missing TR5 mapping. |
| Generic animating | `Objects/Generic/Object/objects.cpp` | `AnimatingControl` | `TriggerFlags == 666` source comment: helicopter animating in Train level | OCB 666 enables helicopter loop behaviour for that animating use case. | Mapped | Object scope needs careful slot mapping before broad UI exposure. |
| Generic animating | `Objects/Generic/Object/objects.cpp` | `AnimatingControl` | `TriggerFlags == 2` source comment: make animating disappear when anti-triggered | OCB 2 makes the animating disappear when anti-triggered. | Mapped | Applies to the generic animating control path. |
| Earthquake | `Objects/Generic/Object/objects.cpp` | `EarthquakeControl` | Source comments: `333` and `888` legacy; positive seconds; negative rumble seconds | Positive OCB is earthquake duration in seconds, negative OCB is rumble duration in seconds; 333 = legacy 16 seconds, 888 = legacy 5 seconds. | Mapped | `333` suppresses the camera bounce branch because of source condition `!legacyMode333`. |
| Flame emitter | `Objects/Effects/flame_emitters.cpp` | `FlameEmitterControl` / `InitializeFlameEmitter` | Negative OCB is converted to `ocb = -TriggerFlags`; `(ocb & 7) == 2 || 7` is constant jet; otherwise intermittent jet | Negative OCB enables jet flame modes using lower bits and timing formula. | Partial | Pattern is clear, but every numeric value should not be expanded yet. |
| Flame emitter | `Objects/Effects/flame_emitters.cpp` | `FlameEmitterControl` | Negative flipmap branch uses `DoFlipMap(-TriggerFlags)` when active and `ItemFlags[0] == 0` | Negative OCB can trigger flipmap handling in this path. | Partial | Needs object/slot-specific verification before exposing. |
| Flame emitter 2 | `Objects/Effects/flame_emitters.cpp` | `FlameEmitter2Control` | `TriggerFlags` 0, 1, 3, 4, 123 map to fire sizes; 2 is moving flame | OCB selects flame size or moving flame mode. | Mapped | Values are suitable for provider entries. |
| Flame emitter 2 | `Objects/Effects/flame_emitters.cpp` | `InitializeFlameEmitter2` | OCB 2 uses 80 position offset; others use 256; OCB 123 skips orientation offset | OCB 2 and 123 also alter placement behaviour. | Mapped | Add warnings in provider entries. |
| Flame emitter 3 | `Objects/Effects/flame_emitters.cpp` | `InitializeFlameEmitter3` / `FlameEmitter3Control` | `TriggerFlags >= 3` links to `ID_ANIMATING3`; `TriggerFlags == 2 || 4` creates directional electricity | Electric arc emitter modes; OCB values 2/4 directional, 3+ link to animating targets. | Partial | Requires slot naming before broad UI exposure. |
| Puzzle hole | `Objects/Generic/puzzles_keys.cpp` | `PuzzleHoleCollision` | `TriggerFlags >= 0`, `<= 1024`, `999`, `998`, `> 1024`, `< 0` classify puzzle type | OCB selects normal, animation-after, cutscene or specific puzzle animation behaviour. | Partial | Negative OCB is used as animation number by `-TriggerFlags`; 998/999 special meaning needs more context. |
| Puzzle done | `Objects/Generic/puzzles_keys.cpp` | `PuzzleDoneCollision` | `TriggerFlags != 999` affects collision handling | OCB 999 has special no-normal-collision behaviour for done receptacle. | Partial | Needs paired puzzle-hole context before exposing as final. |
| TR2 skidoo | `Objects/TR2/Vehicles/skidoo.cpp` | inspected sections | No `TriggerFlags` usage found in inspected sections. | No OCB mapping found in this pass. | Unknown | Re-scan if later exact object files change. |
| TR2 speedboat | `Objects/TR2/Vehicles/speedboat.cpp` | inspected sections | No `TriggerFlags` usage found in inspected section. | No OCB mapping found in this pass. | Unknown | Re-scan if later exact object files change. |

## Provider candidates for this branch

These are safe to prepare in `TenBuiltInObjectParameterProvider` because the meaning is directly visible in TEN source:

### Generic doors

- OCB `1`: raisable cog-switch door.
- OCB `2`: crowbar door.

### Generic switches

- OCB `0`: big lever.
- OCB `1`: small lever.
- OCB `2`: small button.
- OCB `3`: big button.
- OCB `4`: giant button.
- OCB `5`: valve.
- OCB `6`: wall hole.
- OCB `7`: custom switch animation setup, but ItemFlags details must remain separate.

### Dart emitter / dart

- Signed OCB value controls damage and poison:
  - `0`: default damage 25.
  - positive value: damage amount.
  - negative value: absolute damage amount plus poison.

### Falling block

- OCB `0`: contact activation allowed.
- positive value: frame countdown before crumble/fall logic.

### Earthquake

- positive value: earthquake duration in seconds.
- negative value: rumble duration in seconds.
- OCB `333`: legacy 16 second mode.
- OCB `888`: legacy 5 second mode.

### Flame emitter 2

- OCB `0`: normal large flame size.
- OCB `1`: normal medium flame size.
- OCB `2`: moving flame.
- OCB `3`: small flame size.
- OCB `4`: very small flame size.
- OCB `123`: very small flame size with special placement handling.

### Generic animating

- OCB `2`: disappear when anti-triggered.
- OCB `666`: helicopter animating behaviour used by Train level.

## Deferred / unsafe for final mapping in this pass

- Full sequence-door value matrix: only the lookup and comparison are visible in the inspected source. The final value table needs the `SequenceResults` initialization source.
- Fullblock switch values: the switch OCB is pushed into the sequence array, but the resulting meaning depends on the sequence-door table.
- Generic switch non-enum fallback animation IDs: code maps OCB to Lara animation IDs, but this needs a dedicated UI warning and probably a separate advanced parameter.
- Flame emitter negative jet formula: the lower-bit behaviour is visible, but a generated full numeric table would be noisy and error-prone. Keep as a structured signed OCB pattern first.
- Flame emitter 3 arc/link modes: needs slot-aware naming before final editor exposure.
- Puzzle-hole values: source shows broad classification, but special values `998` and `999` need additional source context before final wording.
- TRNG OCBs: intentionally not included. They must come from TRNG TXT files later.

## Integration notes

- The provider must remain editor-side data. It must not import or depend on TEN runtime classes.
- The provider should not be auto-registered until object type / slot identification is good enough to avoid noisy definitions on unrelated objects.
- `ItemFlags`-based details should be represented as separate advanced object parameters later, not as OCB meanings.
- Any later `.md` or `.txt` export should be generated from structured provider data where possible; this report is only the audit trail.
