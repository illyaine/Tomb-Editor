using System;
using System.Collections.Generic;
using System.Linq;

namespace TombLib.LevelData.ObjectParameters
{
    public sealed class TenReviewedOcbObjectParameterProvider : IObjectParameterProvider
    {
        public const string ProviderId = "ten.reviewed.ocb";
        private const string RawOcbParameterId = "ocb.raw";

        public string Id => ProviderId;
        public string Name => "TEN reviewed OCB catalog";

        public IEnumerable<ObjectParameterDefinitionSet> GetDefinitionSets(ObjectParameterContext context)
        {
            if (!IsSupportedContext(context))
                yield break;

            int slotId = context.SlotId.Value;
            bool hasReviewedSet = true;

            if (slotId >= 850 && slotId <= 879)
                yield return BuildDoorSet(context);
            else if (slotId == 817)
                yield return BuildShootSwitch2Set(context);
            else if ((slotId >= 800 && slotId <= 829) || slotId == 831 || slotId == 832)
                yield return BuildGenericSwitchSet(context);
            else if (slotId == 830)
                yield return BuildPulleySet(context);
            else if (slotId >= 668 && slotId <= 683)
                yield return BuildPuzzleHoleSet(context);
            else if (slotId >= 684 && slotId <= 699)
                yield return BuildPuzzleDoneSet(context);
            else if ((slotId >= 393 && slotId <= 402) || (slotId >= 435 && slotId <= 444))
                yield return BuildPushableSet(context);
            else if (slotId == 359)
                yield return BuildFlameEmitterSet(context);
            else if (slotId == 360)
                yield return BuildFlameEmitter2Set(context);
            else if (slotId == 361)
                yield return BuildFlameEmitter3Set(context);
            else if (slotId == 369)
                yield return BuildDartEmitterSet(context);
            else if (slotId == 372 || slotId == 373)
                yield return BuildFallingBlockSet(context);
            else if (slotId == 374)
                yield return BuildCrumblingPlatformSet(context);
            else if (slotId == 1036)
                yield return BuildLensFlareSet(context);
            else if (slotId == 1057)
                yield return BuildEmberEmitterSet(context);
            else if (slotId == 1089)
                yield return BuildFireflyEmitterSet(context);
            else if (slotId == 220)
                yield return BuildWingedMutantSet(context);
            else if (slotId == 232 || slotId == 233)
                yield return BuildDragonSet(context);
            else if (slotId == 108)
                yield return BuildRaptorSet(context);
            else if (slotId == 253)
                yield return BuildSophiaLeighSet(context);
            else if (slotId == 283)
                yield return BuildImpSet(context);
            else
                hasReviewedSet = false;

            if (!hasReviewedSet)
                yield return BuildGenericRawOcbSet(context);
        }

        public IEnumerable<ObjectParameterValidationMessage> Validate(ObjectParameterContext context, ObjectParameterValueSet values)
        {
            if (values == null || !string.Equals(values.ProviderId, ProviderId, StringComparison.OrdinalIgnoreCase))
                yield break;

            ObjectParameterValue rawOcb = values.Values.FirstOrDefault(value => string.Equals(value.ParameterId, RawOcbParameterId, StringComparison.OrdinalIgnoreCase));
            if (rawOcb == null || string.IsNullOrWhiteSpace(rawOcb.Value))
                yield break;

            if (!short.TryParse(rawOcb.Value.Trim(), out _))
            {
                yield return new ObjectParameterValidationMessage
                {
                    Severity = ObjectParameterMessageSeverity.Error,
                    ParameterId = RawOcbParameterId,
                    Message = "OCB must be a signed 16-bit integer."
                };
            }
        }

        private static bool IsSupportedContext(ObjectParameterContext context)
        {
            return context != null &&
                   context.SlotId.HasValue &&
                   (string.Equals(context.ObjectTypeId, "MoveableInstance", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(context.ObjectTypeId, "StaticInstance", StringComparison.OrdinalIgnoreCase)) &&
                   string.Equals(context.EngineId, "TombEngine", StringComparison.OrdinalIgnoreCase);
        }

        private static ObjectParameterDefinitionSet BuildDoorSet(ObjectParameterContext context)
        {
            return BuildSet(
                context,
                "ten.ocb.generic.doors.common",
                "TEN OCB: Generic doors",
                "Reviewed TEN OCB values for generic door behaviour.",
                ObjectParameterMappingStatus.Mapped,
                new List<ObjectParameterPreset>
                {
                    Preset("raisableCogSwitchDoor", "Raisable cog-switch door", 1),
                    Preset("crowbarDoor", "Crowbar door", 2)
                },
                new List<ObjectParameterOcbDefinition>
                {
                    Ocb(1, "Raisable cog-switch door", "Door is handled as a raisable cog-switch door.", "Generic doors"),
                    Ocb(2, "Crowbar door", "Door is handled as a crowbar door.", "Generic doors")
                });
        }

        private static ObjectParameterDefinitionSet BuildShootSwitch2Set(ObjectParameterContext context)
        {
            return BuildSet(
                context,
                "ten.ocb.generic.switches.shoot_switch2",
                "TEN OCB: Shoot Switch 2",
                "Reviewed TEN OCB value for the special Shoot Switch 2 trigger and mesh branch.",
                ObjectParameterMappingStatus.Mapped,
                new List<ObjectParameterPreset>
                {
                    Preset("normal", "Normal shoot switch", 0),
                    Preset("special444", "Special trigger and mesh branch", 444)
                },
                new List<ObjectParameterOcbDefinition>
                {
                    Ocb(0, "Normal shoot switch", "Default shoot switch behaviour.", "Shoot Switch 2"),
                    Ocb(444, "Special trigger and mesh branch", "OCB 444 uses the special Shoot Switch 2 trigger test and mesh handling branch.", "Shoot Switch 2")
                });
        }

        private static ObjectParameterDefinitionSet BuildGenericSwitchSet(ObjectParameterContext context)
        {
            return BuildSet(
                context,
                "ten.ocb.generic.switches.generic_switch",
                "TEN OCB: Generic switch",
                "Reviewed TEN OCB values for generic switch type selection. Unknown values fall back to animation-ID mode in TEN.",
                ObjectParameterMappingStatus.Mapped,
                new List<ObjectParameterPreset>
                {
                    Preset("bigLever", "Big lever", 0),
                    Preset("smallLever", "Small lever", 1),
                    Preset("smallButton", "Small button", 2),
                    Preset("bigButton", "Big button", 3),
                    Preset("giantButton", "Giant button", 4),
                    Preset("valve", "Valve", 5),
                    Preset("wallHole", "Wall hole", 6),
                    Preset("customSwitch", "Custom switch using ItemFlags", 7)
                },
                new List<ObjectParameterOcbDefinition>
                {
                    Ocb(0, "Big lever", "SWT_BIG_LEVER", "Generic switch"),
                    Ocb(1, "Small lever", "SWT_SMALL_LEVER", "Generic switch"),
                    Ocb(2, "Small button", "SWT_SMALL_BUTTON", "Generic switch"),
                    Ocb(3, "Big button", "SWT_BIG_BUTTON", "Generic switch"),
                    Ocb(4, "Giant button", "SWT_GIANT_BUTTON", "Generic switch"),
                    Ocb(5, "Valve", "SWT_VALVE", "Generic switch"),
                    Ocb(6, "Wall hole", "SWT_WALL_HOLE", "Generic switch"),
                    Ocb(7, "Custom switch using ItemFlags", "SWT_CUSTOM; ItemFlags 4, 5 and 6 are advanced switch animation/offset fields and are not raw OCB values.", "Generic switch")
                });
        }

        private static ObjectParameterDefinitionSet BuildPulleySet(ObjectParameterContext context)
        {
            return BuildSet(
                context,
                "ten.ocb.generic.switches.pulley_switch",
                "TEN OCB: Pulley switch",
                "Raw OCB stores the required pull count. TEN converts OCB 0 to 1 during initialization.",
                ObjectParameterMappingStatus.Mapped,
                new List<ObjectParameterPreset>(),
                new List<ObjectParameterOcbDefinition>
                {
                    Ocb(0, "Default pull count", "TEN converts OCB 0 to one required pull during initialization.", "Pulley switch"),
                    Ocb(1, "Required pull count", "Positive OCB values are used as the required pull count.", "Pulley switch")
                });
        }

        private static ObjectParameterDefinitionSet BuildPuzzleHoleSet(ObjectParameterContext context)
        {
            return BuildSet(
                context,
                "ten.ocb.generic.puzzle_hole",
                "TEN OCB: Puzzle Hole",
                "Source-backed OCB values for puzzle-hole interaction animation flow. Some branches remain partial because exact builder-facing wording depends on puzzle setup.",
                ObjectParameterMappingStatus.Partial,
                new List<ObjectParameterPreset>(),
                new List<ObjectParameterOcbDefinition>
                {
                    Ocb(0, "Normal puzzle use flow", "OCB 0 uses the normal puzzle/key use animation flow.", "Puzzle Hole", false, ObjectParameterOcbMode.FixedValue, ObjectParameterMappingStatus.Partial),
                    Ocb(1, "Animation-after/special flow", "Positive OCB values in the normal range use the animation-after/special flag flow. Use the exact raw value carefully.", "Puzzle Hole", false, ObjectParameterOcbMode.FixedValue, ObjectParameterMappingStatus.Partial),
                    Ocb(-1, "Custom Lara animation", "Negative OCB values use the absolute value as Lara animation number.", "Puzzle Hole")
                });
        }

        private static ObjectParameterDefinitionSet BuildPuzzleDoneSet(ObjectParameterContext context)
        {
            return BuildSet(
                context,
                "ten.ocb.generic.puzzle_done",
                "TEN OCB: Puzzle Done",
                "Source-backed OCB values for puzzle-done interaction and collision flow. Some branches remain partial because exact builder-facing wording depends on puzzle setup.",
                ObjectParameterMappingStatus.Partial,
                new List<ObjectParameterPreset>(),
                new List<ObjectParameterOcbDefinition>
                {
                    Ocb(0, "Normal puzzle done flow", "OCB 0 uses the normal puzzle/key done animation flow.", "Puzzle Done", false, ObjectParameterOcbMode.FixedValue, ObjectParameterMappingStatus.Partial),
                    Ocb(1, "Animation-after/special flow", "Positive OCB values in the normal range use the animation-after/special flag flow. Use the exact raw value carefully.", "Puzzle Done", false, ObjectParameterOcbMode.FixedValue, ObjectParameterMappingStatus.Partial),
                    Ocb(999, "Bypass normal collision", "OCB 999 bypasses the normal object collision path for puzzle done objects.", "Puzzle Done", false, ObjectParameterOcbMode.FixedValue, ObjectParameterMappingStatus.Partial),
                    Ocb(-1, "Custom Lara animation", "Negative OCB values use the absolute value as Lara animation number.", "Puzzle Done")
                });
        }

        private static ObjectParameterDefinitionSet BuildPushableSet(ObjectParameterContext context)
        {
            return BuildSet(
                context,
                "ten.ocb.generic.pushable.common",
                "TEN OCB: Pushable object flags",
                "Reviewed TEN OCB bit flags for pushable object behaviour.",
                ObjectParameterMappingStatus.Mapped,
                new List<ObjectParameterPreset>(),
                new List<ObjectParameterOcbDefinition>
                {
                    Ocb(1, "Enable falling behaviour", "Bit 0 / mask 1 enables falling behaviour.", "Pushable objects", true, ObjectParameterOcbMode.AdditiveFlags),
                    Ocb(2, "Disable automatic center alignment", "Bit 1 / mask 2 disables automatic center alignment.", "Pushable objects", true, ObjectParameterOcbMode.AdditiveFlags)
                });
        }

        private static ObjectParameterDefinitionSet BuildFlameEmitterSet(ObjectParameterContext context)
        {
            return BuildSet(
                context,
                "ten.ocb.effects.flame_emitter",
                "TEN OCB: Flame Emitter",
                "Reviewed source-backed Flame Emitter OCB values for normal and jet flame behaviour.",
                ObjectParameterMappingStatus.Partial,
                new List<ObjectParameterPreset>(),
                new List<ObjectParameterOcbDefinition>
                {
                    Ocb(0, "Normal flame", "Non-negative OCB values use the normal flame path. OCB 0 is the default normal flame.", "Flame Emitter", false, ObjectParameterOcbMode.FixedValue, ObjectParameterMappingStatus.Partial),
                    Ocb(-1, "Intermittent jet flame", "Negative OCB values select jet flame mode. Values whose absolute value does not end in bit pattern 2 or 7 use intermittent jet flames.", "Flame Emitter", false, ObjectParameterOcbMode.FixedValue, ObjectParameterMappingStatus.Partial),
                    Ocb(-2, "Constant jet flame", "Absolute OCB value with low bits 2 creates a constant jet flame.", "Flame Emitter", false, ObjectParameterOcbMode.FixedValue, ObjectParameterMappingStatus.Partial),
                    Ocb(-7, "Constant jet flame alternate", "Absolute OCB value with low bits 7 creates a constant jet flame and is also used by the placement offset branch.", "Flame Emitter", false, ObjectParameterOcbMode.FixedValue, ObjectParameterMappingStatus.Partial),
                    Ocb(-8, "Timed intermittent jet flame", "Negative OCB values whose absolute value is divisible by 8 use a longer pause timer based on OCB / 8.", "Flame Emitter", false, ObjectParameterOcbMode.FixedValue, ObjectParameterMappingStatus.Partial)
                });
        }

        private static ObjectParameterDefinitionSet BuildFlameEmitter2Set(ObjectParameterContext context)
        {
            return BuildSet(
                context,
                "ten.ocb.effects.flame_emitter2",
                "TEN OCB: Flame Emitter 2",
                "Reviewed TEN OCB values for Flame Emitter 2 size and movement behaviour.",
                ObjectParameterMappingStatus.Mapped,
                new List<ObjectParameterPreset>
                {
                    Preset("normalLargeWithLight", "Large flame with dynamic light", 0),
                    Preset("normalMedium", "Medium flame", 1),
                    Preset("movingFlameWithLight", "Moving flame with dynamic light", 2),
                    Preset("smallFlame", "Small flame", 3),
                    Preset("tinyFlame", "Tiny flame", 4),
                    Preset("legacyTinyFlame", "Legacy tiny flame", 123)
                },
                new List<ObjectParameterOcbDefinition>
                {
                    Ocb(0, "Large flame with dynamic light", "OCB 0 creates the default large Flame Emitter 2 fire and enables dynamic light.", "Flame Emitter 2"),
                    Ocb(1, "Medium flame", "OCB 1 creates a medium Flame Emitter 2 fire without the dynamic-light branch used by OCB 0 and 2.", "Flame Emitter 2"),
                    Ocb(2, "Moving flame with dynamic light", "OCB 2 skips the normal stationary flame spawn and moves the emitter forward while also using the dynamic-light branch.", "Flame Emitter 2"),
                    Ocb(3, "Small flame", "OCB 3 creates a smaller Flame Emitter 2 fire.", "Flame Emitter 2"),
                    Ocb(4, "Tiny flame", "OCB 4 creates a tiny Flame Emitter 2 fire.", "Flame Emitter 2"),
                    Ocb(123, "Legacy tiny flame", "OCB 123 follows the same tiny-flame size branch as OCB 4.", "Flame Emitter 2"),
                    Ocb(-1, "Trigger flipmap 1", "Negative OCB values are used as flipmap-trigger values; this preset represents -1. Use the raw value carefully for other flipmap numbers.", "Flame Emitter 2", false, ObjectParameterOcbMode.FixedValue, ObjectParameterMappingStatus.Partial, "Negative values map to flipmap numbers and need exact project-specific review.")
                });
        }

        private static ObjectParameterDefinitionSet BuildFlameEmitter3Set(ObjectParameterContext context)
        {
            return BuildSet(
                context,
                "ten.ocb.effects.flame_emitter3",
                "TEN OCB: Flame Emitter 3",
                "Reviewed source-backed Flame Emitter 3 OCB values for small fires and electricity arcs.",
                ObjectParameterMappingStatus.Partial,
                new List<ObjectParameterPreset>(),
                new List<ObjectParameterOcbDefinition>
                {
                    Ocb(0, "Small fire mode", "OCB 0 uses the small fire path.", "Flame Emitter 3"),
                    Ocb(2, "Forward electricity arc", "OCB 2 projects an electricity arc in front of the emitter.", "Flame Emitter 3", false, ObjectParameterOcbMode.FixedValue, ObjectParameterMappingStatus.Partial),
                    Ocb(3, "Arc to linked Animating3 targets", "OCB values >= 3 link to Animating3 objects with matching TriggerFlags and use them as electricity targets.", "Flame Emitter 3", false, ObjectParameterOcbMode.FixedValue, ObjectParameterMappingStatus.Partial),
                    Ocb(4, "Forward electricity arc alternate", "OCB 4 also projects an electricity arc in front of the emitter.", "Flame Emitter 3", false, ObjectParameterOcbMode.FixedValue, ObjectParameterMappingStatus.Partial)
                });
        }

        private static ObjectParameterDefinitionSet BuildDartEmitterSet(ObjectParameterContext context)
        {
            return BuildSet(
                context,
                "ten.ocb.generic.traps.dart_emitter",
                "TEN OCB: Dart emitter damage",
                "Signed raw OCB controls dart damage. Negative values mean poison dart damage using the absolute value.",
                ObjectParameterMappingStatus.Mapped,
                new List<ObjectParameterPreset>(),
                new List<ObjectParameterOcbDefinition>
                {
                    Ocb(0, "Default dart damage", "OCB 0 uses TEN's default dart damage.", "Dart emitter"),
                    Ocb(1, "Damage amount", "Positive OCB values are used as direct dart damage.", "Dart emitter"),
                    Ocb(-1, "Poison damage", "Negative OCB values create poison darts; the absolute value is used as damage.", "Dart emitter")
                });
        }

        private static ObjectParameterDefinitionSet BuildFallingBlockSet(ObjectParameterContext context)
        {
            return BuildSet(
                context,
                "ten.ocb.generic.traps.falling_block",
                "TEN OCB: Falling block",
                "Positive raw OCB values are used as a frame countdown before falling.",
                ObjectParameterMappingStatus.Mapped,
                new List<ObjectParameterPreset>(),
                new List<ObjectParameterOcbDefinition>
                {
                    Ocb(0, "Default/contact activation path", "Source-backed but kept as partial because the runtime path depends on additional block state.", "Falling block", false, ObjectParameterOcbMode.FixedValue, ObjectParameterMappingStatus.Partial, "Partial reviewed meaning."),
                    Ocb(1, "Frame countdown before falling", "Positive OCB values are used as frame countdown before falling.", "Falling block")
                });
        }

        private static ObjectParameterDefinitionSet BuildCrumblingPlatformSet(ObjectParameterContext context)
        {
            return BuildSet(
                context,
                "ten.ocb.generic.traps.crumbling_platform",
                "TEN OCB: Crumbling platform",
                "Signed raw OCB controls crumble delay. Negative values require a trigger and use the absolute value as frame delay.",
                ObjectParameterMappingStatus.Mapped,
                new List<ObjectParameterPreset>(),
                new List<ObjectParameterOcbDefinition>
                {
                    Ocb(0, "Default crumble delay", "OCB 0 uses TEN's default crumble delay.", "Crumbling platform"),
                    Ocb(1, "Frame delay", "Positive OCB values are used as frame delay.", "Crumbling platform"),
                    Ocb(-1, "Trigger required frame delay", "Negative OCB values require a trigger; the absolute value is used as frame delay.", "Crumbling platform")
                });
        }

        private static ObjectParameterDefinitionSet BuildLensFlareSet(ObjectParameterContext context)
        {
            return BuildSet(
                context,
                "ten.ocb.effects.lens_flare",
                "TEN OCB: Lens flare",
                "Raw OCB stores the fade start radius in blocks.",
                ObjectParameterMappingStatus.Mapped,
                new List<ObjectParameterPreset>(),
                new List<ObjectParameterOcbDefinition>
                {
                    Ocb(1, "Fade start radius in blocks", "Raw OCB value is used as fade start radius in blocks.", "Lens flare")
                });
        }

        private static ObjectParameterDefinitionSet BuildEmberEmitterSet(ObjectParameterContext context)
        {
            return BuildSet(
                context,
                "ten.ocb.effects.ember_emitter",
                "TEN OCB: Ember Emitter",
                "Reviewed TEN OCB values for Ember Emitter particle size and area spawning.",
                ObjectParameterMappingStatus.Mapped,
                new List<ObjectParameterPreset>(),
                new List<ObjectParameterOcbDefinition>
                {
                    Ocb(0, "Default ember size", "OCB 0 uses default ember size 1.0.", "Ember Emitter"),
                    Ocb(10, "Positive size value", "Positive OCB values are divided by 10.0 and used as ember particle size. OCB 10 means size 1.0.", "Ember Emitter"),
                    Ocb(-11, "Area/randomized ember spawning", "Negative OCB values use the area/randomized spawning branch and are clamped to at most -11.", "Ember Emitter")
                });
        }

        private static ObjectParameterDefinitionSet BuildFireflyEmitterSet(ObjectParameterContext context)
        {
            return BuildSet(
                context,
                "ten.ocb.effects.firefly_emitter",
                "TEN OCB: Firefly Emitter",
                "Source-backed Firefly Emitter OCB values. Positive and negative values alter the generated swarm effect.",
                ObjectParameterMappingStatus.Partial,
                new List<ObjectParameterPreset>(),
                new List<ObjectParameterOcbDefinition>
                {
                    Ocb(0, "Default additive fireflies", "OCB 0 uses the normal additive firefly sprite path and default area value.", "Firefly Emitter", false, ObjectParameterOcbMode.FixedValue, ObjectParameterMappingStatus.Partial),
                    Ocb(1, "Positive area/range value", "Positive OCB values are clamped into the Firefly area range and use the additive sprite path.", "Firefly Emitter", false, ObjectParameterOcbMode.FixedValue, ObjectParameterMappingStatus.Partial),
                    Ocb(-1, "Negative subtractive effect", "Negative OCB values are clamped into the Firefly area range and use the subtractive sprite path.", "Firefly Emitter", false, ObjectParameterOcbMode.FixedValue, ObjectParameterMappingStatus.Partial)
                });
        }

        private static ObjectParameterDefinitionSet BuildWingedMutantSet(ObjectParameterContext context)
        {
            return BuildSet(
                context,
                "ten.ocb.tr1.entity.winged_mutant",
                "TEN OCB: TR1 Winged Mutant",
                "Reviewed TEN OCB bit flags for the TR1 Winged Mutant. Partial weapon-disable bits are intentionally not exposed yet.",
                ObjectParameterMappingStatus.Mapped,
                new List<ObjectParameterPreset>(),
                new List<ObjectParameterOcbDefinition>
                {
                    Ocb(1, "Start aerial", "WMUTANT_OCB_START_AERIAL", "TR1 Winged Mutant", true, ObjectParameterOcbMode.AdditiveFlags),
                    Ocb(2, "Start inactive", "WMUTANT_OCB_START_INACTIVE", "TR1 Winged Mutant", true, ObjectParameterOcbMode.AdditiveFlags),
                    Ocb(4, "Start in pose", "WMUTANT_OCB_START_POSE", "TR1 Winged Mutant", true, ObjectParameterOcbMode.AdditiveFlags),
                    Ocb(8, "Disable wings/flying", "WMUTANT_OCB_NO_WINGS", "TR1 Winged Mutant", true, ObjectParameterOcbMode.AdditiveFlags)
                });
        }

        private static ObjectParameterDefinitionSet BuildDragonSet(ObjectParameterContext context)
        {
            return BuildSet(
                context,
                "ten.ocb.tr2.entity.dragon",
                "TEN OCB: TR2 Dragon",
                "Reviewed TEN OCB values for TR2 Dragon behaviour.",
                ObjectParameterMappingStatus.Mapped,
                new List<ObjectParameterPreset>
                {
                    Preset("normal", "Normal dragon behaviour", 0),
                    Preset("daggerRequired", "Requires dagger for kill flow", 1)
                },
                new List<ObjectParameterOcbDefinition>
                {
                    Ocb(0, "Normal dragon behaviour", "DRAGON_OCB_NORMAL", "TR2 Dragon"),
                    Ocb(1, "Requires dagger for kill flow", "DRAGON_OCB_DAGGER", "TR2 Dragon")
                });
        }

        private static ObjectParameterDefinitionSet BuildRaptorSet(ObjectParameterContext context)
        {
            return BuildSet(
                context,
                "ten.ocb.tr3.entity.raptor",
                "TEN OCB: TR3 Raptor",
                "Reviewed TEN OCB values for TR3 Raptor behaviour.",
                ObjectParameterMappingStatus.Mapped,
                new List<ObjectParameterPreset>
                {
                    Preset("normal", "Normal behaviour", 0),
                    Preset("enableJump", "Enable jump behaviour", 1)
                },
                new List<ObjectParameterOcbDefinition>
                {
                    Ocb(0, "Normal behaviour", "OCB_NORMAL_BEHAVIOUR", "TR3 Raptor"),
                    Ocb(1, "Enable jump behaviour", "OCB_ENABLE_JUMP", "TR3 Raptor")
                });
        }

        private static ObjectParameterDefinitionSet BuildSophiaLeighSet(ObjectParameterContext context)
        {
            return BuildSet(
                context,
                "ten.ocb.tr3.entity.sophia_leigh",
                "TEN OCB: TR3 Sophia Leigh",
                "Reviewed TEN OCB values for TR3 Sophia Leigh behaviour.",
                ObjectParameterMappingStatus.Mapped,
                new List<ObjectParameterPreset>
                {
                    Preset("normal", "Normal behaviour", 0),
                    Preset("tower", "Tower behaviour", 1),
                    Preset("towerWithVolume", "Tower behaviour with volume variant", 2)
                },
                new List<ObjectParameterOcbDefinition>
                {
                    Ocb(0, "Normal behaviour", "SophiaOCB::Normal", "TR3 Sophia Leigh"),
                    Ocb(1, "Tower behaviour", "SophiaOCB::Tower", "TR3 Sophia Leigh"),
                    Ocb(2, "Tower behaviour with volume variant", "SophiaOCB::TowerWithVolume", "TR3 Sophia Leigh")
                });
        }

        private static ObjectParameterDefinitionSet BuildImpSet(ObjectParameterContext context)
        {
            return BuildSet(
                context,
                "ten.ocb.tr5.entity.imp",
                "TEN OCB: TR5 Imp",
                "Reviewed TEN OCB values for TR5 Imp behaviour.",
                ObjectParameterMappingStatus.Mapped,
                new List<ObjectParameterPreset>
                {
                    Preset("climbUp", "Climb up", 1),
                    Preset("roll", "Roll", 2),
                    Preset("stoneAttack", "Stone attack", 3)
                },
                new List<ObjectParameterOcbDefinition>
                {
                    Ocb(1, "Climb up", "IMP_OCB_CLIMB_UP", "TR5 Imp"),
                    Ocb(2, "Roll", "IMP_OCB_ROLL", "TR5 Imp"),
                    Ocb(3, "Stone attack", "IMP_OCB_STONE_ATTACK", "TR5 Imp")
                });
        }

        private static ObjectParameterDefinitionSet BuildGenericRawOcbSet(ObjectParameterContext context)
        {
            short currentOcb = 0;
            if (context?.ObjectKey != null && context.ObjectKey.Ocb.HasValue)
                currentOcb = context.ObjectKey.Ocb.Value;

            return BuildSet(
                context,
                "ten.ocb.generic.raw." + context.SlotId.Value,
                "TEN OCB: Raw object OCB",
                "No reviewed object-specific OCB mapping is available yet. The raw OCB value is still shown and preserved.",
                ObjectParameterMappingStatus.Unknown,
                new List<ObjectParameterPreset>(),
                new List<ObjectParameterOcbDefinition>
                {
                    Ocb(currentOcb, "Current raw OCB", "Current raw OCB value. Meaning is not reviewed for this object yet.", "Raw OCB", false, ObjectParameterOcbMode.FixedValue, ObjectParameterMappingStatus.Unknown, "Review TEN source before documenting this object-specific OCB.")
                });
        }

        private static ObjectParameterDefinitionSet BuildSet(ObjectParameterContext context, string id, string name, string description, ObjectParameterMappingStatus status, List<ObjectParameterPreset> presets, List<ObjectParameterOcbDefinition> ocbDefinitions)
        {
            return new ObjectParameterDefinitionSet
            {
                Id = id,
                ProviderId = ProviderId,
                Name = name,
                Description = description,
                Source = ObjectParameterSource.Ten,
                MappingStatus = status,
                ObjectTypeId = context.ObjectTypeId,
                SlotId = context.SlotId,
                Groups = new List<ObjectParameterGroup>
                {
                    BuildRawOcbGroup(context, description, status)
                },
                Presets = presets,
                OcbDefinitions = ocbDefinitions
            };
        }

        private static ObjectParameterGroup BuildRawOcbGroup(ObjectParameterContext context, string description, ObjectParameterMappingStatus status)
        {
            return new ObjectParameterGroup
            {
                Id = "ocb",
                Name = "OCB",
                Description = description,
                Kind = ObjectParameterGroupKind.Basic,
                Parameters = new List<ObjectParameterDefinition>
                {
                    new ObjectParameterDefinition
                    {
                        Id = RawOcbParameterId,
                        Name = "Raw OCB",
                        Description = description,
                        Type = ObjectParameterType.Integer,
                        Source = ObjectParameterSource.Ten,
                        MappingStatus = status,
                        DefaultValue = GetCurrentOcb(context),
                        MinimumValue = short.MinValue.ToString(),
                        MaximumValue = short.MaxValue.ToString(),
                        Example = "0"
                    }
                }
            };
        }

        private static string GetCurrentOcb(ObjectParameterContext context)
        {
            if (context?.ObjectKey != null && context.ObjectKey.Ocb.HasValue)
                return context.ObjectKey.Ocb.Value.ToString();

            return "0";
        }

        private static ObjectParameterPreset Preset(string id, string name, short ocbValue)
        {
            return new ObjectParameterPreset
            {
                Id = id,
                Name = name,
                LegacyOcbValue = ocbValue,
                Values = new List<ObjectParameterValue>
                {
                    new ObjectParameterValue
                    {
                        ParameterId = RawOcbParameterId,
                        Value = ocbValue.ToString(),
                        Source = ObjectParameterSource.Ten,
                        MappingStatus = ObjectParameterMappingStatus.Mapped
                    }
                }
            };
        }

        private static ObjectParameterOcbDefinition Ocb(short value, string name, string description, string group, bool combinable = false, ObjectParameterOcbMode mode = ObjectParameterOcbMode.FixedValue, ObjectParameterMappingStatus status = ObjectParameterMappingStatus.Mapped, string warning = "")
        {
            return new ObjectParameterOcbDefinition
            {
                Value = value,
                Name = name,
                Description = description,
                Group = group,
                IsCombinable = combinable,
                Mode = mode,
                MappingStatus = status,
                Warning = warning
            };
        }
    }
}
