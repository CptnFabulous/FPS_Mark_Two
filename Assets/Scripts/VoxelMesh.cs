using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class VoxelMesh : MonoBehaviour
{
    MeshFilter meshFilter;
    MeshRenderer renderer;
    MeshCollider collider;
    Mesh mesh;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        renderer = GetComponent<MeshRenderer>();
        collider = GetComponent<MeshCollider>();
        mesh = new Mesh();
    }
    

    public void Calculate(float[,,] values, Vector3Int dimensions, int scale)
    {
        DualContouring(ref mesh, values, dimensions, scale);
        Refresh();
    }
    public void Clear()
    {
        mesh.Clear();
        Refresh();
    }

    void Refresh()
    {
        bool meshExists = mesh.vertexCount > 0;
        renderer.enabled = meshExists;
        collider.enabled = meshExists;
        if (!meshExists) return;
        meshFilter.sharedMesh = mesh;
        collider.sharedMesh = mesh;
    }

    #region Mesh generation

    static readonly Vector3Int[] adjacencies = new Vector3Int[6]
    {
        Vector3Int.left,
        Vector3Int.right,
        Vector3Int.down,
        Vector3Int.up,
        Vector3Int.back,
        Vector3Int.forward
    };
    static readonly int[][] faceCornersForAdjacentSquares = new int[6][]
    {
        // Ints need to be oriented top left, top right, bottom left, bottom right
        new int[] { 3, 1, 2, 0 }, // Left
        new int[] { 4, 5, 6, 7 }, // Right
        new int[] { 0, 4, 2, 6 }, // Down
        new int[] { 1, 3, 5, 7 }, // Up
        new int[] { 0, 1, 4, 5 }, // Backward
        new int[] { 7, 3, 6, 2 }, // Forward
    };
    static readonly Vector3Int[] corners = new Vector3Int[8]
    {
        new Vector3Int(0, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 1, 1),
        new Vector3Int(1, 0, 0),
        new Vector3Int(1, 1, 0),
        new Vector3Int(1, 0, 1),
        new Vector3Int(1, 1, 1)
    };

    static List<Vector3Int> existingVertexPositions = new List<Vector3Int>();
    static int[] cornerVertexListPositions = new int[4];

    static Vector3[] vertices = new Vector3[1024];
    static int[] triangles = new int[1024 * 3];
    static int createdVertices;
    static int createdTriangles;


    public static void DualContouring(ref Mesh mesh, float[,,] values, int scale)
    {
        Vector3Int dimensions = new Vector3Int();
        for (int i = 0; i < 3; i++)
        {
            dimensions[i] = values.GetLength(i);
        }
        DualContouring(ref mesh, values, dimensions, scale);
    }
    public static void DualContouring(ref Mesh mesh, float[,,] values, Vector3Int dimensions, int scale)
    {
        existingVertexPositions.Clear();
        createdVertices = 0;
        createdTriangles = 0;

        float GetNeighbouringValue(Vector3Int neighbour)
        {
            // Get the mesh value for the adjacent square
            float value = 0;
            if (MiscFunctions.IsIndexOutsideArray(dimensions, neighbour))
            {
                // TO DO: check for adjacent chunk, and get adjacent value there
                value = 0;
            }
            else
            {
                value = values[neighbour.x, neighbour.y, neighbour.z];
            }
            return value;
        }

        // Add faces for each grid position
        MiscFunctions.IterateThroughGrid(dimensions, (x, y, z) =>
        {
            if (values[x, y, z] <= 0) return;

            // Check neighbouring faces
            Vector3Int coords = new Vector3Int(x, y, z);
            for (int d = 0; d < 6; d++)
            {
                // If there's a value there, continue.
                Vector3Int neighbour = coords + adjacencies[d];

                // Get the value for the adjacent square
                float value = GetNeighbouringValue(neighbour);
                // Don't proceed if the mesh there is filled.
                // There might theoretically be edge cases with tiny gaps of smoke, but that's not worth the hassle of generating those extra faces.
                if (value > 0) continue;

                // Calculate desired vertices and their orders in the list
                for (int c = 0; c < 4; c++)
                {
                    // Use cornersForFaces to get the correct corner indices for this face direction.
                    // Use those indices to get the correct offsets for the desired vertex positions.
                    Vector3Int vertexGridPos = coords + corners[faceCornersForAdjacentSquares[d][c]];
                    // Vertex grid dimensions are equivalent to value grid dimensions plus one on each side
                    // To get the vertices on either side of a grid position on an axis, get that axis, then plus 1 for the other.

                    // Get the position of this vertex position in the list. If it doesn't exist, add it and get that position.
                    // We're saving the grid positions so we can use them later for extra calculations, to shift the actual vertex position.
                    int indexOrder = existingVertexPositions.IndexOf(vertexGridPos);
                    if (indexOrder < 0)
                    {
                        indexOrder = existingVertexPositions.Count;
                        existingVertexPositions.Add(vertexGridPos);
                    }
                    // Write the list positions of the desired vertices to an array
                    cornerVertexListPositions[c] = indexOrder;
                }

                // Write triangles to match list order
                triangles[createdTriangles] = cornerVertexListPositions[0];
                triangles[createdTriangles + 1] = cornerVertexListPositions[1];
                triangles[createdTriangles + 2] = cornerVertexListPositions[2];
                triangles[createdTriangles + 3] = cornerVertexListPositions[2];
                triangles[createdTriangles + 4] = cornerVertexListPositions[1];
                triangles[createdTriangles + 5] = cornerVertexListPositions[3];
                createdTriangles += 6;
            }
        });

        #region Generate vertices

        // For each vertex, adjust position based on fill values of surrounding spaces
        Vector3 offset = new Vector3(0.5f, 0.5f, 0.5f);
        createdVertices = existingVertexPositions.Count;
        for (int v = 0; v < createdVertices; v++)
        {
            // Desired vertex is equivalent to grid position, minus 0.5 on each side so they actually go inbetween/around the desired grid spaces
            Vector3Int vertexPos = existingVertexPositions[v];
            Vector3 vertex = vertexPos - offset;






            // TO DO: do all the happy horseshit to shift the vertex position based on the fill values of the grid squares it's a part of


            // IDEA TO TRY: make the average a float, and instead of just adding 1, add the read value to the average
            // Also try not multiplying the position input into the average
            // Also try having the average 'position' be an offset added onto vertex afterwards, rather than the average of every added position




            // If there's only one filled space to move towards, it needs to move towards the centre

            // When there are two blocks, and one is shrunk, the moving vertices converge at about the centre and then continue past each other, resulting in an inverted mesh.

            // I think the averaging is fine, but I somehow need to make the strength of each square influence the outcome.
            // E.g. it's an average between all directions, but if one or more spaces are equally high-value it'll move towards the middle ground between those.
            // If a space has the smallest value, it has the least sway. If there's no value, there's no sway.


            
            float maxWeight = 0;
            for (int c = 0; c < 8; c++)
            {
                // Get each grid position around the vertex
                // 'Next' position index on that axis is just the vertex position index
                // 'Previous position index is the same but -1
                Vector3Int gridPos = vertexPos - Vector3Int.one + corners[c];

                // Get weight of value
                float weight = GetNeighbouringValue(gridPos);
                weight = Mathf.Clamp01(weight);
                maxWeight += weight;
            }
            Vector3 weightedAverage = Vector3.zero;
            for (int c = 0; c < 8; c++)
            {
                // Get each grid position around the vertex
                // 'Next' position index on that axis is just the vertex position index
                // 'Previous position index is the same but -1
                Vector3Int gridPos = vertexPos - Vector3Int.one + corners[c];

                // Get weight of value
                float weight = GetNeighbouringValue(gridPos);
                weight = Mathf.Clamp01(weight);
                weight /= maxWeight;

                Vector3 cornerOffset = gridPos - vertex;
                cornerOffset *= (1 - weight);
                //cornerOffset *= (2 * (1 - weight));
                weightedAverage += cornerOffset;
            }
            // No need to divide by the max, since each part of the offset is divided already
            vertex += weightedAverage;
            





            /*
            float maxWeight = 0;
            Vector3 weightedAverage = Vector3.zero;
            for (int c = 0; c < 8; c++)
            {
                // Get each grid position around the vertex
                // 'Next' position index on that axis is just the vertex position index
                // 'Previous position index is the same but -1
                Vector3Int gridPos = vertexPos - Vector3Int.one + corners[c];

                // Get weight of value
                float weight = GetNeighbouringValue(gridPos);
                weight = Mathf.Clamp01(weight);
                maxWeight += weight;

                Vector3 cornerOffset = gridPos - vertex;
                cornerOffset *= (1 - weight);
                weightedAverage += cornerOffset;
            }
            weightedAverage /= maxWeight;
            vertex += weightedAverage;
            */

            /*
            int numOfFilledSpaces = 0;
            Vector3 average = Vector3.zero;
            for (int c = 0; c < 8; c++)
            {
                // TO DO: make sure the code for getting the correct grid points actually works properly?
                
                // Get each grid position around the vertex
                // 'Next' position index on that axis is just the vertex position index
                // 'Previous position index is the same but -1
                Vector3Int gridPos = vertexPos - Vector3Int.one + corners[c];

                // Check vertex
                float value = GetNeighbouringValue(gridPos);
                //if (value > 0) continue;
                if (value <= 0) continue;

                value = Mathf.Clamp01(value);
                numOfFilledSpaces++;

                // Lerp from centre to desired position based on 1 - value
                // When value is at 1, vertex is at fully inflated position
                // When value is at zero, vertex is fully shrunk back towards the 'previous vertex' as if there's nothing there.
                Vector3 cornerOffset = 2 * (gridPos - vertex);
                average += (1 - value) * cornerOffset;
            }

            if (numOfFilledSpaces > 0)
            {
                average /= numOfFilledSpaces;
                vertex += average;
            }
            */

            vertices[v] = vertex;
        }

        #endregion

        // Divide vertex positions to ensure they fit the grid scale.
        // As long as all the vertices have the same relative positions, and the indices stay the same, the mesh shape will be the same.
        for (int v = 0; v < vertices.Length; v++) vertices[v] /= scale;

        // Assign vertex and triangle data to mesh
        mesh.Clear();
        mesh.SetVertices(vertices, 0, createdVertices);
        mesh.SetTriangles(triangles, 0, createdTriangles, 0, false);
        // TO DO: set normals?
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }

    #endregion
}