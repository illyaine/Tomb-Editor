using System;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

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
        private const string TombEditorAssemblyName = "TombEditor";
        private const float HDRMarker = -0.001f;
        private const float HDRCoreTransportScale = 0.001f;
        private const float HDRTransportIntensityScale = 0.00000001f;
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
            set
            {
                if (value == _quality)
                    return;

                if (value == LightQuality.HDR)
                {
                    // PRJ2 stores HDR lights in the existing light record. The loader
                    // fills the raw backing fields before it reaches the quality byte,
                    // so decode the transport values at this transition point.
                    if (_innerAngle <= HDRMarker)
                    {
                        HDRPhysicalRange = Math.Max(_innerRange, 0.01f);
                        HDRSourceWidth = Math.Max(_outerRange, 0.01f);
                        HDRSourceHeight = Math.Max(0.01f, Math.Min(16.0f,
                            _intensity / HDRTransportIntensityScale - 1.0f));
                        HDRCoreIntensity = Math.Max((HDRMarker - _innerAngle) / HDRCoreTransportScale, 0.0f);
                        HDRHaloIntensity = Math.Max(_outerAngle, 0.0f);
                        HDRGlareIntensity = Math.Max(Math.Abs(_rotationX) / HDRGlareDegreesPerUnit, 0.0f);

                        var encodedYaw = _rotationY - (float)Math.Floor(_rotationY / 360.0f) * 360.0f;
                        var modeIndex = Math.Max(0, Math.Min(2, (int)Math.Floor(encodedYaw / HDRModeSectorDegrees)));
                        HDRMode = (HDRLightMode)modeIndex;
                        HDRPhysicalIntensity = Math.Max(0.0f, Math.Min(10.0f,
                            (encodedYaw - modeIndex * HDRModeSectorDegrees) / HDRIntensityDegreesPerUnit));
                    }
                }
                else if (_quality == LightQuality.HDR)
                {
                    // Converting an HDR light back to a conventional light should
                    // yield useful values instead of exposing the transport marker.
                    _intensity = HDRPhysicalIntensity;
                    _innerRange = 0.0f;
                    _outerRange = HDRPhysicalRange;
                    _innerAngle = 20.0f;
                    _outerAngle = 25.0f;
                    _rotationX = 0.0f;
                    _rotationY = 0.0f;
                }

                _quality = value;
            }
        }

        public LightType Type
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get
            {
                // Keep the transport type private to TombLib. TombEditor itself sees
                // HDR as a real editor type, while PRJ2 and all compilers continue to
                // receive the compatible Spot record used by the current file format.
                if (IsHDRLight &&
                    string.Equals(Assembly.GetCallingAssembly().GetName().Name,
                        TombEditorAssemblyName,
                        StringComparison.Ordinal))
                {
                    return LightType.HDR;
                }

                return _type;
            }
            set { _type = value == LightType.HDR ? LightType.Spot : value; }
        }

        public bool IsHDRLight => Quality == LightQuality.HDR;
        public LightType DisplayType => IsHDRLight ? LightType.HDR : Type;
        public string DisplayName => IsHDRLight ? "HDR Light" : Type + " Light";

        public bool IsAvailableForGameVersion(TRVersion.Game gameVersion)
        {
            return !IsHDRLight || gameVersion == TRVersion.Game.TombEngine;
        }

        public Vector3 Color { get; set; } = new Vector3(1.0f, 1.0f, 1.0f);

        // The existing TEN spot-light record is used as a backwards-compatible
        // transport container until the versioned HDR-light extension block is
        // available. TEN recognizes the tiny negative inner-angle marker and does
        // not interpret this record as a physical spotlight.
        public float Intensity
        {
            get
            {
                if (!IsHDRLight)
                    return _intensity;

                var sourceHeight = Math.Max(0.01f, Math.Min(16.0f, HDRSourceHeight));
                return HDRTransportIntensityScale * (1.0f + sourceHeight);
            }
            set { _intensity = value; }
        }

        public float InnerRange
        {
            get { return IsHDRLight ? HDRPhysicalRange : _innerRange; }
            set { _innerRange = value; }
        }

        public float OuterRange
        {
            get { return IsHDRLight ? HDRSourceWidth : _outerRange; }
            set { _outerRange = value; }
        }

        public float InnerAngle
        {
            get
            {
                return IsHDRLight
                    ? HDRMarker - Math.Max(HDRCoreIntensity, 0.0f) * HDRCoreTransportScale
                    : _innerAngle;
            }
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

        // TEN HDR-light values shown by the editor. Physical range, source width and
        // source height are in sectors. Core, halo and glare are independent layers.
        public HDRLightMode HDRMode { get; set; } = HDRLightMode.LightAndEffects;
        public float HDRPhysicalIntensity { get; set; } = 1.0f;
        public float HDRPhysicalRange { get; set; } = 5.0f;
        public float HDRSourceWidth { get; set; } = 0.125f;
        public float HDRSourceHeight { get; set; } = 0.125f;
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
            set { _rotationY = (float)(value - Math.Floor(value / 360.0f) * 360.0f); }
        }

        public LightInstance(LightType type)
        {
            if (type == LightType.HDR)
            {
                // Keep the serialized base type compatible with the existing TEN
                // room-light compiler. LightQuality.HDR and the angle marker retain
                // the editor identity and identify the record to TEN.
                _type = LightType.Spot;
                Quality = LightQuality.HDR;
                IsStaticallyUsed = false;
                IsUsedForImportedGeometry = false;
                return;
            }

            _type = type;
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

        public string ShortName()
        {
            return DisplayName;
        }

        public override string ToString()
        {
            return DisplayName +
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