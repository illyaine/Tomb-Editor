using System;
using TombLib.LevelData.SectorEnums;

namespace TombLib.LevelData.SectorStructs
{
    public sealed class SubdividedSectorSurface : ICloneable
    {
        public const int MinimumSubdivisions = 2;
        public const int MaximumSubdivisions = 8;

        private readonly int[,] _heights;

        public int Subdivisions { get; }
        public int VertexCount => Subdivisions + 1;

        public int this[int x, int z]
        {
            get
            {
                ValidateVertexCoordinate(x, z);
                return _heights[x, z];
            }
            set
            {
                ValidateVertexCoordinate(x, z);
                _heights[x, z] = value;
            }
        }

        public SubdividedSectorSurface(int subdivisions)
        {
            ValidateSubdivisions(subdivisions);

            Subdivisions = subdivisions;
            _heights = new int[VertexCount, VertexCount];
        }

        private SubdividedSectorSurface(int subdivisions, int[,] heights)
        {
            Subdivisions = subdivisions;
            _heights = heights;
        }

        public static bool IsSupportedSubdivisionCount(int subdivisions)
        {
            return subdivisions >= MinimumSubdivisions &&
                   subdivisions <= MaximumSubdivisions &&
                   (subdivisions & (subdivisions - 1)) == 0;
        }

        public static SubdividedSectorSurface FromLegacySurface(SectorSurface surface, int subdivisions)
        {
            if (surface.DiagonalSplit != DiagonalSplit.None)
                throw new ArgumentException("Subdivided terrain currently supports only legacy surfaces without diagonal steps.", nameof(surface));

            var result = new SubdividedSectorSurface(subdivisions);
            result.InitializeFromLegacySurface(surface);
            return result;
        }

        public int[,] CopyHeights()
        {
            return (int[,])_heights.Clone();
        }

        public SubdividedSectorSurface Clone()
        {
            return new SubdividedSectorSurface(Subdivisions, CopyHeights());
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        private void InitializeFromLegacySurface(SectorSurface surface)
        {
            bool diagonalIsXEqualsZ = surface.SplitDirectionIsXEqualsZ;

            for (int x = 0; x < VertexCount; x++)
            {
                float normalizedX = (float)x / Subdivisions;

                for (int z = 0; z < VertexCount; z++)
                {
                    float normalizedZ = (float)z / Subdivisions;
                    _heights[x, z] = SampleLegacySurface(surface, normalizedX, normalizedZ, diagonalIsXEqualsZ);
                }
            }
        }

        private static int SampleLegacySurface(SectorSurface surface, float x, float z, bool diagonalIsXEqualsZ)
        {
            float height;

            if (diagonalIsXEqualsZ)
            {
                if (x >= z)
                    height = surface.XnZn + (surface.XpZn - surface.XnZn) * x + (surface.XpZp - surface.XpZn) * z;
                else
                    height = surface.XnZn + (surface.XpZp - surface.XnZp) * x + (surface.XnZp - surface.XnZn) * z;
            }
            else
            {
                if (x + z <= 1.0f)
                    height = surface.XnZn + (surface.XpZn - surface.XnZn) * x + (surface.XnZp - surface.XnZn) * z;
                else
                    height = surface.XpZp + (surface.XnZp - surface.XpZp) * (1.0f - x) + (surface.XpZn - surface.XpZp) * (1.0f - z);
            }

            return (int)Math.Round(height, MidpointRounding.AwayFromZero);
        }

        private static void ValidateSubdivisions(int subdivisions)
        {
            if (!IsSupportedSubdivisionCount(subdivisions))
                throw new ArgumentOutOfRangeException(nameof(subdivisions), $"Subdivision count must be a power of two from {MinimumSubdivisions} to {MaximumSubdivisions}.");
        }

        private void ValidateVertexCoordinate(int x, int z)
        {
            if (x < 0 || x >= VertexCount)
                throw new ArgumentOutOfRangeException(nameof(x));
            if (z < 0 || z >= VertexCount)
                throw new ArgumentOutOfRangeException(nameof(z));
        }
    }
}
