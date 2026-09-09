using System;
using System.Numerics;

namespace TombLib.LevelData.SectorStructs
{
    public sealed class SubdividedSectorMesh
    {
        public Vector3[] Vertices { get; }
        public Vector2[] TextureCoordinates { get; }
        public int[] Indices { get; }
        public int TriangleCount => Indices.Length / 3;

        private SubdividedSectorMesh(Vector3[] vertices, Vector2[] textureCoordinates, int[] indices)
        {
            Vertices = vertices;
            TextureCoordinates = textureCoordinates;
            Indices = indices;
        }

        public static SubdividedSectorMesh FromSurface(SubdividedSectorSurface surface)
        {
            if (surface == null)
                throw new ArgumentNullException(nameof(surface));

            int vertexCountPerSide = surface.VertexCount;
            var vertices = new Vector3[vertexCountPerSide * vertexCountPerSide];
            var textureCoordinates = new Vector2[vertices.Length];
            float step = Level.SectorSizeUnit / surface.Subdivisions;

            for (int z = 0; z < vertexCountPerSide; z++)
            {
                for (int x = 0; x < vertexCountPerSide; x++)
                {
                    int index = z * vertexCountPerSide + x;
                    vertices[index] = new Vector3(x * step, surface[x, z], z * step);
                    textureCoordinates[index] = new Vector2(
                        (float)x / surface.Subdivisions,
                        (float)z / surface.Subdivisions);
                }
            }

            var indices = new int[surface.Subdivisions * surface.Subdivisions * 6];
            int indexOffset = 0;

            for (int z = 0; z < surface.Subdivisions; z++)
            {
                for (int x = 0; x < surface.Subdivisions; x++)
                {
                    int vertex0 = z * vertexCountPerSide + x;
                    int vertex1 = vertex0 + 1;
                    int vertex2 = vertex0 + vertexCountPerSide;
                    int vertex3 = vertex2 + 1;

                    indices[indexOffset++] = vertex0;
                    indices[indexOffset++] = vertex2;
                    indices[indexOffset++] = vertex1;
                    indices[indexOffset++] = vertex1;
                    indices[indexOffset++] = vertex2;
                    indices[indexOffset++] = vertex3;
                }
            }

            return new SubdividedSectorMesh(vertices, textureCoordinates, indices);
        }
    }
}
