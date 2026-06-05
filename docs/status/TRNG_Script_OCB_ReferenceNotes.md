# TRNG Script OCB Reference Notes

This note is based on the uploaded `TRNG_Script.zip` reference pack.
It must stay separate from the TEN source-backed OCB provider work.

## Classification

The ZIP is TRNG/NGLE script-reference material, not a complete object-by-object OCB meaning list.
It contains 57 text reference files, including:

- `NG_Constants.txt`
- `MEMORY_ITEM.txt`
- `MEMORY_CODE.txt`
- `MEMORY_SLOT.txt`
- `STATE_ID_LIST.txt`
- `ANIMATION_LIST_*.txt`
- value-domain helper lists such as timers, colors, sequence values, memory indices and percentages.

## OCB-related findings

Direct OCB hits found in the uploaded text references:

| File | Line | Text | Interpretation |
| --- | ---: | --- | --- |
| `MEMORY_ITEM.txt` | 25 | `17: OCB Code (The value you typed in OCB of this item) (Short)` | Confirms that TRNG exposes the typed object OCB as item-memory field 17. |
| `NG_Constants.txt` | 38 | `OldFlip. (?) Force Von Croy to reach <&>AI_FOLLOW OCB when ???` | TRNG trigger/action reference, not an object OCB meaning table. |
| `NG_Constants.txt` | 41 | `OldFlip. Start screen timer and force Von Croy to reach <&>AI_FOLLOW OCB number` | TRNG AI/flipeffect reference. |
| `NG_Constants.txt` | 78 | `Lara. (Move) Move Lara in LARA_START_POS with <&>OCB value in (E)way` | TRNG script/action parameter that uses an OCB value. |
| `NG_Constants.txt` | 3144 | `1:Current AI OCB value` | AI memory/parameter reference. |
| `NG_Constants.txt` | 3145 | `2:Next AI OCB value to reach` | AI memory/parameter reference. |
| `NG_Constants.txt` | 3327 | `Enemy. Move immediatly <#>enemy in lara_start_pos with (E)OCB setting` | TRNG action using an OCB setting. |
| `NG_Constants.txt` | 3367 | `Enemy. (OCB) Change the OCB value of <#>Moveable with (E)Big Number value` | TRNG action that changes a moveable OCB at runtime. |
| `NG_Constants.txt` | 3443 | `Makes bubbles in position of <&>moveable (LARA_START_POS)` | Lara-start-position reference; not a direct object OCB meaning. |
| `NG_Constants.txt` | 3450 | `Ghost trap in position #LARA_START_POS for Wraith3` | Lara-start-position reference; not a direct object OCB meaning. |
| `NG_Constants.txt` | 5100 | `33:OCB=%3d of LARA_START_POS(%d) in sector (%d,%d) of %s` | Diagnostic/script text involving LARA_START_POS OCB. |

## Decision for the TE OCB project

- Use `TRNG_Script.zip` as a TRNG script/trigger reference only.
- Do not merge its constants blindly into the TEN OCB provider seed.
- Do not treat `NG_Constants.txt` entries as object OCB meanings unless a specific object/slot mapping is independently confirmed.
- The UI label should consistently be `OCB`, matching Tomb Editor / TRLE / NGLE terminology.
- TRNG runtime actions that change or read an OCB should be documented separately from static object setup OCB meanings.

## Useful confirmed TRNG memory relation

`MEMORY_ITEM.txt` confirms:

```text
17: OCB Code (The value you typed in OCB of this item) (Short)
```

This is useful for future TRNG compatibility notes because it confirms the OCB is stored and exposed as a short item-memory field.

## Relation to current TEN work

Current TEN files remain the authoritative source for TEN-specific object behaviour:

- `docs/status/TEN_Object_OCB_SourceScan_Generated.md`
- `docs/status/TEN_Object_OCB_SymbolCatalog_Generated.md`
- `docs/status/TEN_Object_OCB_CuratedCatalog.md`
- `docs/status/TEN_Object_OCB_ReviewedProviderSeed.json`

TRNG reference material can later be used to add compatibility hints, especially where legacy/TRNG behaviour overlaps with current TEN source behaviour.

## Pending follow-up

If a real TRNG object-specific OCB list is found later, scan it separately and add another reviewed source file, for example:

- `docs/status/TRNG_Object_OCB_Catalog.md`
- `docs/status/TRNG_Object_OCB_ReviewedProviderSeed.json`

That future catalog should include object/slot name, OCB value, meaning, source line and confidence level, just like the TEN reviewed seed.
