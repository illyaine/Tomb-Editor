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
            else if ((slotId >= 800 && slotId <= 829) || slotId == 831 || slotId == 832)
                yield return BuildGenericSwitchSet(context);
            else if (slotId == 830)
                yield return BuildPulleySet(context);
            else if ((slotId >= 393 && slotId <= 402) || (slotId >= 435 && slotId <= 444))
                yield return BuildPushableSet(context);
            else if (slotId == 369)
                yield return BuildDartEmitterSet(context);
            else if (slotId == 372 || slotId == 373)
                yield return BuildFallingBlockSet(context);
            else if (slotId == 374)
                yield return BuildCrumblingPlatformSet(context);
            else if (slotId == 1036)
                yield return BuildLensFlareSet(context);
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
