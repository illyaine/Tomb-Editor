# TEN Object OCB Curated Catalog - Pass 1

This catalog is derived from `docs/status/TEN_Object_OCB_SourceScan_Generated.md` and is intentionally conservative.
It is not a complete object-parameter provider yet. Only source-backed meanings are listed here.

## Scope and rules

- `ItemInfo::TriggerFlags` is the OCB storage used by TEN object logic.
- `ItemInfo::TestOcb()` treats OCB values as bit flags: `(TriggerFlags & ocbFlags) == ocbFlags`.
- `ItemFlags[]` are runtime/configuration fields and must not be presented as OCB values unless source explicitly copies or derives them from `TriggerFlags`.
- Entries marked `verified` are directly supported by source comments or simple source control flow.
- Entries marked `partial` are source-backed but still require either enum-value extraction, slot-name confirmation, or a second manual review before UI exposure.
- Entries marked `needs-review` must not be shipped as user-facing OCB descriptions yet.

## Core OCB semantics

| Scope | Value / bits | Meaning | Source evidence | Status | Notes |
| --- | --- | --- | --- | --- | --- |
| All items | `TriggerFlags` | Stores the raw OCB value. | `TombEngine\Game\items.h:152` | verified | Used by object code as the common OCB field. |
| All items | `TestOcb(flags)` | Bit-test helper: all requested bits must be present. | `TombEngine\Game\items.cpp:64-66` | verified | Required for OCB bitfield objects such as some creatures. |
| All items | `RemoveOcb(flags)` | Clears OCB bits from `TriggerFlags`. | `TombEngine\Game\items.cpp:69-71` | verified | Used after setup in some OCB bitfield initializers. |
| All items | `ClearAllOcb()` | Resets `TriggerFlags` to `0`. | `TombEngine\Game\items.cpp:74-76` | verified | Runtime helper, not a builder-facing OCB meaning. |
| All items | `ItemFlags[]` | Separate runtime/config fields. | `TombEngine\Game\items.h:150`, `items.cpp:84-116` | verified | Keep visually separate from OCB in the TE UI. |

## Generic object and switch OCBs

| Object / group | Value / bits | Meaning | Source evidence | Status | UI note |
| --- | --- | --- | --- | --- | --- |
| Generic doors | `1` | Door is raisable by cog-switch door logic. | `TombEngine\Objects\Generic\Doors\generic_doors.cpp:381-382` | verified | Show as legacy-compatible door mode. |
| Generic doors | `2` | Door can be opened with crowbar interaction. | `TombEngine\Objects\Generic\Doors\generic_doors.cpp:292` | verified | Show as crowbar door mode. |
| Sequence door | raw value | Door opens when the entered switch sequence resolves to this value. | `TombEngine\Objects\Generic\Doors\sequence_door.cpp:35` | partial | Needs the `SequenceResults` table before listing exact preset values. |
| Cog switch | `0` / empty | Enables built-in cog-switch-to-door coupling. | `TombEngine\Objects\Generic\Switches\cog_switch.cpp:67`, `:105`, `:148` | partial | UI text should say default cog-door behaviour; non-zero meaning needs review. |
| Full-block switch | raw value | Appends its `TriggerFlags` to the current sequence array. | `TombEngine\Objects\Generic\Switches\fullblock_switch.cpp:112-113` | verified | Meaning depends on connected sequence door. |
| Generic switch | `3` | Only this switch type currently passes the temporary underwater interaction guard. | `TombEngine\Objects\Generic\Switches\generic_switch.cpp:103-104` | partial | Do not label as final switch type until enum values are extracted. |
| Generic switch | `3` / `4` | Switch is treated as disabled while in `SWITCH_OFF` state. | `TombEngine\Objects\Generic\Switches\generic_switch.cpp:85` | partial | Needs enum-name extraction for user-facing labels. |
| Generic switch | `SWT_CUSTOM` | Uses `ItemFlags[4]` as on-animation, `ItemFlags[5]` as off-animation, `ItemFlags[6]` as placement offset. | `TombEngine\Objects\Generic\Switches\generic_switch.cpp:164`, `:218-219` | partial | This belongs in object parameters, but not as raw OCB-only UI. |
| Generic switch | fallback values | Unknown/non-enum OCB value is used as Lara's on animation; `OCB + 1` is used as off animation. | `TombEngine\Objects\Generic\Switches\generic_switch.cpp:223-224` | verified | UI should warn that this is animation-ID mode. |
| Pulley switch | `0` | Auto-converted to `1` during initialization. | `TombEngine\Objects\Generic\Switches\pulley_switch.cpp:74-76` | verified | Show `0` as default/one pull. |
| Pulley switch | positive value | Number of pulls required; decremented until zero. | `TombEngine\Objects\Generic\Switches\pulley_switch.cpp:78-79`, `:173-187` | verified | User-facing label: required pull count. |
| Shoot switch 2 | `444` | Special shoot-switch behaviour: trigger test and mesh handling. | `TombEngine\Objects\Generic\Switches\switch.cpp:28`, `:64` | partial | Needs slot-specific UI text; source confirms special value. |
| Pushable objects | bit `0` | Enables falling behaviour. | `TombEngine\Objects\Generic\Object\Pushable\PushableObject.cpp:70-72` | verified | Present as checkbox. |
| Pushable objects | bit `1` | Disables automatic center-align when set; center-align is used when bit is clear and object is not solid. | `TombEngine\Objects\Generic\Object\Pushable\PushableObject.cpp:72-73` | verified | Present as inverse checkbox or clear wording. |

## Generic traps, puzzle objects and effects

| Object / group | Value / bits | Meaning | Source evidence | Status | UI note |
| --- | --- | --- | --- | --- | --- |
| Dart / dart emitter | `0` | Uses default dart harm damage. | `TombEngine\Objects\Generic\Traps\dart_emitter.cpp:47-50` | verified | Damage fallback is source-backed; exact default constant should be resolved separately. |
| Dart / dart emitter | positive value | Absolute OCB value is used as damage amount. | `TombEngine\Objects\Generic\Traps\dart_emitter.cpp:50` | verified | Numeric damage field. |
| Dart / dart emitter | negative value | Adds poison and uses absolute value as damage. | `TombEngine\Objects\Generic\Traps\dart_emitter.cpp:47-50` | verified | Numeric damage field plus poison mode. |
| Falling block | `0` | Can activate by player/contact path when not already delayed. | `TombEngine\Objects\Generic\Traps\falling_block.cpp:80` | partial | Existing code also mutates `TriggerFlags` as countdown; expose carefully. |
| Falling block | positive value | Treated as a countdown before the falling sequence proceeds. | `TombEngine\Objects\Generic\Traps\falling_block.cpp:102-104` | verified | Numeric frame countdown. |
| Crumbling platform | `0` | Uses default crumble delay. | `TombEngine\Objects\Generic\Traps\crumblingPlatform.cpp:92-93` | verified | Default delay mode. |
| Crumbling platform | positive value | Absolute OCB value is used as delay in frames. | `TombEngine\Objects\Generic\Traps\crumblingPlatform.cpp:92-93` | verified | Numeric frame delay. |
| Crumbling platform | negative value | Requires trigger activation, then converts to positive delay. | `TombEngine\Objects\Generic\Traps\crumblingPlatform.cpp:121-127` | verified | Trigger-required delay mode. |
| Puzzle hole / puzzle done | `0` | Normal puzzle/key use animation flow. | `TombEngine\Objects\Generic\puzzles_keys.cpp:105-111`, `:414-418` | partial | Needs separation between puzzle-hole and puzzle-done UI. |
| Puzzle hole / puzzle done | `1..1024` except `998`, `999` | Uses animation-after/special flag flow. | `TombEngine\Objects\Generic\puzzles_keys.cpp:105-113`, `:414-419` | partial | Existing source indicates animation-related mode, exact user label needs review. |
| Puzzle hole / puzzle done | negative value | Lara animation number is `-TriggerFlags`. | `TombEngine\Objects\Generic\puzzles_keys.cpp:189`, `:530-537` | verified | Numeric custom animation mode. |
| Puzzle done | `999` | Bypasses normal object collision. | `TombEngine\Objects\Generic\puzzles_keys.cpp:249-252` | partial | Special collision mode, exact builder wording needs review. |
| Flame emitter | negative value | Jet flame mode; bitmask branch uses `(-TriggerFlags) & 7`. | `TombEngine\Objects\Effects\flame_emitters.cpp:149-152`, `:353-358` | partial | Needs bit table extraction before UI. |
| Flame emitter 2 | `0` | Normal flame with dynamic light. | `TombEngine\Objects\Effects\flame_emitters.cpp:271-301` | verified | Source groups `0` with dynamic light path. |
| Flame emitter 2 | `2` | Moving flame variant; also gets dynamic light. | `TombEngine\Objects\Effects\flame_emitters.cpp:274`, `:301-313`, `:400-424` | verified | Moving flame preset. |
| Flame emitter 2 | `123` | Special placement branch skip. | `TombEngine\Objects\Effects\flame_emitters.cpp:392-395` | partial | Needs exact user-facing legacy label. |
| Flame emitter 3 | `2` / `4` | Arc target is projected in front of emitter. | `TombEngine\Objects\Effects\flame_emitters.cpp:473-486` | partial | Electricity mode; exact labels need source pass. |
| Flame emitter 3 | `>=3` | Uses target animating references stored in `ItemFlags[2/3]`. | `TombEngine\Objects\Effects\flame_emitters.cpp:438-449`, `:486-501` | partial | Object-linking UI likely needed. |
| Ember emitter | negative value | Uses area/randomized ember spawning; clamped to at most `-11`. | `TombEngine\Objects\Effects\EmberEmitter.cpp:28-36` | verified | Negative area mode. |
| Ember emitter | positive value | Uses OCB divided by `10.0` as size. | `TombEngine\Objects\Effects\EmberEmitter.cpp:94-99` | verified | Numeric size field. |
| Fireflies | signed value | Stored in `ItemFlags[TriggerFlags]`; non-negative and negative branches alter streamer effect. | `TombEngine\Objects\Effects\Fireflies.cpp:54`, `:206-213` | partial | Needs practical UI grouping: area range, light, subtractive/normal effect. |
| Lens flare | positive value | Radius in blocks after which flare starts fading. | `TombEngine\Objects\Effects\LensFlare.cpp:175-176` | verified | Numeric radius in blocks. |
| Waterfall emitter | ItemFlags enum/defaults | Uses `WaterfallItemFlags` fields for velocity, sprite scale, sparseness, mist scale, etc. | `TombEngine\Objects\TR5\Emitter\Waterfall.cpp:29-57` | partial | Important object-parameter candidate, but not confirmed as raw `TriggerFlags` OCB in this scan. |

## Creature and boss OCBs - first verified candidates

| Object / group | Value / bits | Meaning | Source evidence | Status | UI note |
| --- | --- | --- | --- | --- | --- |
| TR1 Winged Mutant | `WMUTANT_OCB_START_AERIAL` | Starts in aerial pathfinding and fly animation. | `TombEngine\Objects\TR1\Entity\WingedMutant.cpp:178-184` | partial | Need enum constant value before UI. |
| TR1 Winged Mutant | `WMUTANT_OCB_START_INACTIVE` | Starts inactive on ground pathfinding. | `TombEngine\Objects\TR1\Entity\WingedMutant.cpp:186-190` | partial | Need enum constant value before UI. |
| TR1 Winged Mutant | `WMUTANT_OCB_START_POSE` | Starts in pose/inactive-style setup. | `TombEngine\Objects\TR1\Entity\WingedMutant.cpp:192-196` | partial | Need enum constant value before UI. |
| TR1 Winged Mutant | `WMUTANT_OCB_NO_WINGS` | Disables flying capability. | `TombEngine\Objects\TR1\Entity\WingedMutant.cpp:218-225` | partial | Need enum constant value before UI. |
| TR1 Winged Mutant | `WMUTANT_OCB_DISABLE_BOMB_WEAPON` | Disables bomb weapon path. | `TombEngine\Objects\TR1\Entity\WingedMutant.cpp:228+` | needs-review | Exact value and surrounding source must be confirmed from latest generated report. |
| TR2 Dragon | `DRAGON_OCB_DAGGER` | Dragon requires player to retrieve dagger for kill flow. | `TombEngine\Objects\TR2\Entity\Dragon.cpp:374-375`, `:701` | partial | Need constant value. Source comment confirms OCB `1` according to scan context. |
| TR3 Raptor | `OCB_ENABLE_JUMP` | Enables jump behaviour. | `TombEngine\Objects\TR3\Entity\Raptor.cpp:105-108` | partial | Need constant value. |
| TR3 Sophia Leigh | `SophiaOCB::Tower` | Tower-specific behaviour path. | `TombEngine\Objects\TR3\Entity\SophiaLeigh.cpp:300` | partial | Need enum values and other Sophia modes from current report. |
| TR4 Skeleton | `0..3` | Initialization uses OCB cases 0 to 3. | `TombEngine\Objects\TR4\Entity\tr4_skeleton.cpp:123-127` | partial | Need full switch body before labels are safe. |

## Do not expose yet

These entries are known to exist in the source scan but need a more specific extractor or manual source review before being user-facing:

- `SequenceResults` value table for sequence doors.
- `SWT_*` switch enum numeric mapping.
- Negative flame-emitter bit table.
- `WMUTANT_*` enum constant values.
- `DRAGON_OCB_DAGGER` numeric definition, even though source comments imply OCB `1`.
- TR3/TR4/TR5 creature-specific OCB constants caught by the latest OCB-symbol scan.
- Waterfall emitter object parameters: scan confirms ItemFlags-driven fields, but not raw OCB mapping yet.

## Next implementation plan

1. Extend the scanner or add a second extractor that records constant definitions and enum assignments near OCB symbols.
2. Generate a machine-readable intermediate catalog, for example `docs/status/TEN_Object_OCB_SourceCatalog.json`, with file, line, object, symbol, value, and confidence.
3. Only after this catalog is reviewed, create the TE-side object-parameter provider.
4. In the TE UI, show source-backed labels and keep unknown/raw OCB values editable.
5. Display Legacy/TEN scope where confirmed; otherwise show `Source-backed, scope pending`.
