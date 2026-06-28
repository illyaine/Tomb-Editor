using System;
using System.Numerics;

namespace TombLib.LevelData
{
    public enum LightQuality : byte
    {
        Default,
        Low,
        Medium,
        High,
        HDR
    }

    public enum LightType : byte
    {
        Point,
        Shadow,
        Spot,
        Effect,
        Sun,
        FogBulb,
        HDR
    }

    public enum HDRLightMode : byte
    {
        LightAndEffects,
        LightOnly,
        EffectsOnly
    }

    public class LightInstance : PositionBasedObjectInstance, IColorable, IReplaceable, IRotateableYX
    {
        private const float HDRMarker = -1000.0f;
        private const float HDRTransportIntensity = 0.000001f;
        private const float HDRModeSectorDegrees = 120.0f;
        private const float HDRIntensityDegreesPerUnit = 10.0f;
        private const float HDRGlareDegreesPerUnit = 10.0f;

        private LightQuality _quality = LightQuality.Default;
        private LightType _type;
        private float _intensity = 0.5f;
        private float _innerRange = 1.0f;
        private float _outerRange = 5.0f;
        private float _innerAngle = 20.0f;
        private float _outerAngle = 25.0f;
        private float _rotationX;
        private float _rotationY;

        public LightQuality Quality
        {
            get { return _quality; }
            set { _quality = value; }
        }

        public LightType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public bool IsHDRLight => Quality == LightQuality.HDR;
        public LightType DisplayType => IsHDRLight ? LightType.HDR : Type;

        public Vector3 Color { get; set; } = new Vector3(1.0f, 1.0f, 1.0f);

        // The existing TEN spot-light record is used as a backwards-compatible
        // transport container until the versioned HDR-light extension block is
        // available. TEN recognizes the negative inner-angle marker and does not
        // interpret this record as a physical spotlight.
        public float Intensity
        {
            get { return IsHDRLight ? HDRTransportIntensity : _intensity; }
            set { _intensity = value; }
        }

        public float InnerRange
        {
            get { return IsHDRLight ? HDRPhysicalRange : _innerRange; }
            set { _innerRange = value; }
        }

        public float OuterRange
        {
            get { return IsHDRLight ? HDRSourceSize : _outerRange; }
            set { _outerRange = value; }
        }

        public float InnerAngle
        {
            get { return IsHDRLight ? HDRMarker - Math.Max(HDRCoreIntensity, 0.0f) : _innerAngle; }
            set { _innerAngle = value; }
        }

        public float OuterAngle
        {
            get { return IsHDRLight ? Math.Max(HDRHaloIntensity, 0.0f) : _outerAngle; }
            set { _outerAngle = value; }
        }

        public bool Enabled { get; set; } = true;
        public bool IsObstructedByRoomGeometry { get; set; } = true;
        public bool IsDynamicallyUsed { get; set; } = true;
        public bool IsStaticallyUsed { get; set; } = true;
        public bool IsUsedForImportedGeometry { get; set; } = true;
        public bool CastDynamicShadows { get; set; } = false;

        // TEN HDR-light values shown by the editor. Physical range and source size
        // are in sectors. Core, halo and glare are independent visible layers.
        public HDRLightMode HDRMode { get; set; } = HDRLightMode.LightAndEffects;
        public float HDRPhysicalIntensity { get; set; } = 1.0f;
        public float HDRPhysicalRange { get; set; } = 5.0f;
        public float HDRSourceSize { get; set; } = 0.125f;
        public float HDRCoreIntensity { get; set; } = 4.0f;
        public float HDRHaloIntensity { get; set; } = 1.4f;
        public float HDRGlareIntensity { get; set; } = 0.8f;

        public bool CanCastDynamicShadows => CastDynamicShadows &&
            ((IsHDRLight && HDRMode != HDRLightMode.EffectsOnly) ||
             (!IsHDRLight && (Type == LightType.Spot || Type == LightType.Point)));

        /// <summary> Degrees in the range [-90, 90]. </summary>
        public float RotationX
        {
            get
            {
                if (!IsHDRLight)
                    return _rotationX;

                return Math.Max(0.0f, Math.Min(8.0f, HDRGlareIntensity)) * HDRGlareDegreesPerUnit;
            }
            set { _rotationX = Math.Max(-90, Math.Min(90, value)); }
        }

        /// <summary> Degrees in the range [0, 360). </summary>
        public float RotationY
        {
            get
            {
                if (!IsHDRLight)
                    return _rotationY;

                var mode = Math.Max(0, Math.Min(2, (int)HDRMode));
                var intensity = Math.Max(0.0f, Math.Min(10.0f, HDRPhysicalIntensity));
                return mode * HDRModeSectorDegrees + intensity * HDRIntensityDegreesPerUnit;
            }
            set { _rotationY = (float)(value - Math.Floor(value / 360.0) * 360.0); }
        }

        public LightInstance(LightType type)
        {
            if (type == LightType.HDR)
            {
                // Keep the serialized base type compatible with the existing TEN
                // room-light compiler. LightQuality.HDR and the angle marker retain
                // the editor identity and identify the record to TEN.
                Type = LightType.Spot;
                Quality = LightQuality.HDR;
                IsStaticallyUsed = false;
                IsUsedForImportedGeometry = false;
                return;
            }

            Type = type;
            switch (type)
            {
                case LightType.Shadow:
                    Intensity *= -1;
                    break;

                case LightType.Effect:
                    InnerRange = 0.99f;
                    OuterRange = 1.0f;
                    IsDynamicallyUsed = false;
                    break;

                case LightType.FogBulb:
                    IsObstructedByRoomGeometry = false;
                    IsStaticallyUsed = false;
                    IsUsedForImportedGeometry = false;
                    break;
            }
        }

        public override string ToString()
        {
            return "Light " + DisplayType +
                ", X = " + WorldPosition.X +
                ", Y = " + -WorldPosition.Y +
                ", Z = " + WorldPosition.Z;
        }

        public override void AddToRoom(Level level, Room room)
        {
            base.AddToRoom(level, room);
        }

        public override void RemoveFromRoom(Level level, Room room)
        {
            base.RemoveFromRoom(level, room);
        }

        public string PrimaryAttribDesc => "Colour";
        public string SecondaryAttribDesc => "Light type";

        public bool ReplaceableEquals(IReplaceable other, bool withProperties = false)
        {
            var otherInstance = other as LightInstance;
            return otherInstance?.Color == Color &&
                (!withProperties || otherInstance?.DisplayType == DisplayType);
        }

        public bool Replace(IReplaceable other, bool withProperties)
        {
            var thatColor = (LightInstance)other;
            if (!ReplaceableEquals(other) && Color != thatColor?.Color)
            {
                Color = thatColor.Color;
                return true;
            }

            return false;
        }
    }
}