using System;
using System.Numerics;

namespace TombLib.LevelData
{
    // All possible shapes are reduced to these three basics.
    // If we need a plane volume, it will be a box with one axis scale set to minimum.
    // If we need legacy "trigger" volume, it will be a box with one axis scale set to maximum.

    public enum VolumeShape : byte
    {
        Box, Sphere, Undefined
    }

    // Possible activator flags. If none is set, volume is disabled.

    [Flags]
    public enum VolumeActivators : int
    {
        None = 0,
        Player = 1,
        NPCs = 2,
        OtherMoveables = 4,
        Statics = 8,
        Flybys = 16,
        PhysicalObjects = 32 // Future-proofness for Bullet
    }

    public class SphereVolumeInstance : VolumeInstance, IScaleable
    {
        private const float _defaultScale = 8.0f;
        private const float _minSize = 128.0f;

        public float Size => Scale * _minSize;

        public float DefaultScale => Level.SectorSizeUnit;
        public float Scale
        {
            get { return _scale; }
            set
            {
                if (value >= 1.0f && value <= _minSize)
                    _scale = value;
            }
        }
        private float _scale = _defaultScale;

        public override string ToString()
        {
            return "Sphere Volume" + GetScriptIDOrName(true) +
                   " (" + (Room?.ToString() ?? "NULL") + ")" + "\n" +
                   (EventSet as VolumeEventSet)?.GetDescription() ?? string.Empty;
        }

        public override string ShortName() => "Sphere volume" + GetScriptIDOrName() + " (" + (Room?.ToString() ?? "NULL") + ")";
    }

    public class BoxVolumeInstance : VolumeInstance, ISizeable, IRotateableYXRoll
    {
        protected const float _maxSize = ushort.MaxValue;
        protected const float _minSize = 32.0f;

        public Vector3 DefaultSize => new Vector3(Level.SectorSizeUnit);

        public Vector3 Size
        {
            get { return _size; }
            set
            {
                _size = new Vector3(MathC.Clamp(value.X, _minSize, _maxSize),
                                    MathC.Clamp(value.Y, _minSize, _maxSize),
                                    MathC.Clamp(value.Z, _minSize, _maxSize));
            }
        }
        private Vector3 _size = new Vector3(Level.SectorSizeUnit);

        public float RotationY
        {
            get { return _rotationY; }
            set { _rotationY = (float)(value - Math.Floor(value / 360.0) * 360.0); }
        }
        private float _rotationY = 0.0f;

        public float RotationX
        {
            get { return _rotationX; }
            set { _rotationX = (float)(value - Math.Floor(value / 360.0) * 360.0); }
        }
        private float _rotationX = 0.0f;

        public float Roll
        {
            get { return _roll; }
            set { _roll = (float)(value - Math.Floor(value / 360.0) * 360.0); }
        }
        private float _roll = 0.0f;

        public override string ToString()
        {
            if (this.IsEffectBox())
            {
                var graph = this.GetGraphEvent();
                var effectCount = graph == null ? 0 : VisualScripting.TriggerNode.LinearizeNodes(graph.Nodes).Count;
                return "Effect Box" + GetScriptIDOrName(true) +
                       " (" + (Room?.ToString() ?? "NULL") + ")" + "\n" +
                       effectCount + (effectCount == 1 ? " effect node" : " effect nodes");
            }

            return "Box Volume" + GetScriptIDOrName(true) +
                   " (" + (Room?.ToString() ?? "NULL") +")" + "\n" +
                   (EventSet as VolumeEventSet)?.GetDescription() ?? string.Empty;
        }

        public override string ShortName()
        {
            return this.IsEffectBox()
                ? "Effect box" + GetScriptIDOrName() + " (" + (Room?.ToString() ?? "NULL") + ")"
                : "Box volume" + GetScriptIDOrName() + " (" + (Room?.ToString() ?? "NULL") + ")";
        }
    }

    public abstract class VolumeInstance : PositionAndScriptBasedObjectInstance, ISpatial
    {
        public VolumeShape Shape()
        {
            if (this is BoxVolumeInstance) return VolumeShape.Box;
            if (this is SphereVolumeInstance) return VolumeShape.Sphere;
            return VolumeShape.Undefined;
        }

        public bool Enabled { get; set; } = true;
        public bool DetectInAdjacentRooms { get; set; } = false;

        public EventSet EventSet { get; set; }

        public override ObjectInstance Clone()
        {
            var clone = (VolumeInstance)base.Clone();

            // Ordinary trigger volumes intentionally share reusable event sets. Effect boxes,
            // however, own their graph. A cloned/stamped box therefore receives a deep copy
            // and a new private identifier.
            if (this.IsEffectBox() && EventSet != null)
            {
                clone.EventSet = EventSet.Clone();
                clone.EventSet.Name = EffectBoxUtils.CreateEventSetName();
            }

            return clone;
        }

        public override void AddToRoom(Level level, Room room)
        {
            base.AddToRoom(level, room);

            if (this.IsEffectBox() && string.IsNullOrWhiteSpace(LuaName))
                AllocateNewLuaName();

            // Deep-cloned effect boxes carry an event set which is not yet part of the target
            // level settings. Register it when the object enters the room.
            if (this.IsEffectBox() && EventSet is VolumeEventSet effectSet &&
                !level.Settings.VolumeEventSets.Contains(effectSet))
            {
                level.Settings.VolumeEventSets.Add(effectSet);
            }
        }

        public override void CopyDependentLevelSettings(Room.CopyDependentLevelSettingsArgs args)
        {
            base.CopyDependentLevelSettings(args);

            if (EventSet == null)
                return;

            if (args.DestinationLevelSettings.VolumeEventSets.Contains(EventSet))
                return;

            args.DestinationLevelSettings.VolumeEventSets.Add(EventSet);
        }

        public abstract string ShortName();
    }
}
