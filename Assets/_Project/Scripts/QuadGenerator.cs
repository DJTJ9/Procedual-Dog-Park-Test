using UnityEngine;
using UnityEngine.Serialization;

public class QuadGenerator : MonoBehaviour
{
    public int Subdivisions = 16; // Anzahl der Unterteilungen

    void Start()
    {
        GenerateMesh();
    }

    void GenerateMesh()
    {
        int vertsPerRow = Subdivisions + 1;
        int totalVertices = vertsPerRow * vertsPerRow;
        int totalTriangles = Subdivisions * Subdivisions * 2 * 3;

        Vector3[] vertices = new Vector3[totalVertices];
        int[] triangles = new int[totalTriangles];
        Vector2[] uvs = new Vector2[totalVertices];

        float stepSize = 1f / Subdivisions;
        float offset = 0.5f; // Damit der Mittelpunkt bei (0,0) liegt
        int vertIndex = 0, triIndex = 0;

        // Vertices & UVs
        for (int y = 0; y < vertsPerRow; y++)
        {
            for (int x = 0; x < vertsPerRow; x++)
            {
                vertices[vertIndex] = new Vector3(x * stepSize - offset, y * stepSize - offset, 0);
                uvs[vertIndex] = new Vector2(x * stepSize, y * stepSize);
                vertIndex++;
            }
        }

        // Triangles
        for (int y = 0; y < Subdivisions; y++)
        {
            for (int x = 0; x < Subdivisions; x++)
            {
                int bottomLeft = y * vertsPerRow + x;
                int bottomRight = bottomLeft + 1;
                int topLeft = bottomLeft + vertsPerRow;
                int topRight = topLeft + 1;

                triangles[triIndex++] = bottomLeft;
                triangles[triIndex++] = topLeft;
                triangles[triIndex++] = topRight;

                triangles[triIndex++] = bottomLeft;
                triangles[triIndex++] = topRight;
                triangles[triIndex++] = bottomRight;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }
}
