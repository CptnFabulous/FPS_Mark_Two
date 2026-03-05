using System.Collections.Generic;
using UnityEngine;

public class VoxelMeshParticleHandler : MonoBehaviour
{
    [SerializeField] VoxelMesh voxelMesh;
    [SerializeField] ParticleSystem particleEmitter;
    [SerializeField] float baseParticleEmissionRate = 1;

    void Refresh(Mesh mesh)
    {
        bool meshExists = mesh.vertexCount > 0;

        switch (meshExists, particleEmitter.isPlaying)
        {
            case (true, false):
                particleEmitter.Play();
                break;
            case (false, true):
                particleEmitter.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                break;
        }

        ParticleSystem.ShapeModule shapeModule = particleEmitter.shape;
        shapeModule.shapeType = ParticleSystemShapeType.Mesh;
        shapeModule.mesh = mesh;
        ParticleSystem.EmissionModule emissionModule = particleEmitter.emission;
        emissionModule.rateOverTime = baseParticleEmissionRate * (mesh.triangles.Length / 3);
    }
}

public partial class VoxelMesh : MonoBehaviour
{
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] MeshRenderer renderer;
    [SerializeField] MeshCollider collider;
#if UNITY_EDITOR
    [SerializeField] bool debugGizmos;
#endif

    Mesh mesh;// = new Mesh();
    Vector3Int dimensions;
    System.Func<Vector3Int, float> obtainValue;
    System.Func<Vector3, float> signedDistanceField;

    private void Awake()
    {
        mesh = new Mesh();
    }
    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (debugGizmos == false) return;
#endif
        if (mesh == null) return;

        Gizmos.matrix = transform.localToWorldMatrix;

        MiscFunctions.IterateThroughGrid(dimensions, (coords) =>
        {
            float value = obtainValue.Invoke(coords);

            float sdf = signedDistanceField.Invoke(coords);
            if (sdf > 0)
            {
                Gizmos.color = Color.Lerp(Color.black, Color.white, sdf / 1);
                Gizmos.DrawCube(coords, value * Vector3.one);
            }

        });

        for (int v = 0; v < mesh.vertexCount; v++)
        {
            Vector3 vertex = mesh.vertices[v];
            Vector3 normal = -NormalFromDerivatives(vertex, (pos) => MidpointBetweenGridSpaces(pos, obtainValue));

            Color rayColour = Color.white;
            for (int c = 0; c < 3; c++)
            {
                rayColour[c] = (normal[c] + 1) / 2;
            }
            Gizmos.color = rayColour;
            Gizmos.DrawRay(vertex, normal);
        }
    }


    public void Calculate(Vector3Int dimensions, System.Func<Vector3Int, float> obtainValue, System.Func<Vector3, float> signedDistanceField, int scale)
    {
        this.obtainValue = obtainValue;
        this.signedDistanceField = signedDistanceField;
        this.dimensions = dimensions;
        GenerateMesh(ref mesh, dimensions, obtainValue, signedDistanceField, scale);
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

        // Rendering
        if (meshFilter != null && renderer != null)
        {
            renderer.enabled = meshExists;
            if (meshExists) meshFilter.sharedMesh = mesh;
        }

        // Collision
        if (collider != null)
        {
            collider.enabled = meshExists;
            if (meshExists) collider.sharedMesh = mesh;
        }

        // TO DO: run event on mesh updated
    }

    #region Mesh generation

    // Static readonly values for stuff that will never change, to save processing power
    public static readonly Vector3Int[] adjacencies = new Vector3Int[6]
    {
        Vector3Int.left,
        Vector3Int.right,
        Vector3Int.down,
        Vector3Int.up,
        Vector3Int.back,
        Vector3Int.forward
    };
    public static readonly int[][] faceCornersForAdjacentSquares = new int[6][]
    {
        // Ints need to be oriented top left, top right, bottom left, bottom right
        new int[] { 3, 1, 2, 0 }, // Left
        new int[] { 4, 5, 6, 7 }, // Right
        new int[] { 0, 4, 2, 6 }, // Down
        new int[] { 1, 3, 5, 7 }, // Up
        new int[] { 0, 1, 4, 5 }, // Backward
        new int[] { 7, 3, 6, 2 }, // Forward
    };
    public static readonly Vector3Int[] corners = new Vector3Int[8]
    {
        //    3 -------- 7
        //   /|         /|
        //  / |        / |
        // 1 -------- 5  |
        // |  |       |  |
        // Y  2 ------|- 6
        // | Z        | /
        // |/         |/
        // 0 --X----- 4
        new Vector3Int(0, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 1, 1),
        new Vector3Int(1, 0, 0),
        new Vector3Int(1, 1, 0),
        new Vector3Int(1, 0, 1),
        new Vector3Int(1, 1, 1),
    };

    public static readonly (int, int)[] edges = new (int, int)[12]
    {
        // The second corner will always be after the first corner on whatever axis they run across.
        (0, 4), // X, 0, 0
        (1, 5), // X, 1, 0
        (2, 6), // X, 0, 1
        (3, 7), // X, 1, 1
        (0, 1), // 0, Y, 0
        (4, 5), // 1, Y, 0
        (2, 3), // 0, Y, 1
        (6, 7), // 1, Y, 1
        (0, 2), // 0, 0, Z
        (4, 6), // 1, 0, Z
        (1, 3), // 0, 1, Z
        (5, 7), // 1, 1, Z
    };

    static readonly Vector3 globalVertexOffset = new Vector3(0.5f, 0.5f, 0.5f);

    // These values aren't kept once the array is complete, but the arrays themselves are to reduce garbage collection
    static float[] valuesOfNeighbours = new float[6];
    static int[] cornerVertexListPositions = new int[4]; // The array positions of the four vertices used to form a face.
    static float[] valuesAroundVertex = new float[8];

    // Values that need to be saved to generate the final mesh.
    //static List<Vector3Int> existingVertexPositions = new List<Vector3Int>();
    static Dictionary<Vector3Int, int> vertexDictionary = new Dictionary<Vector3Int, int>();
    static Vector3[] vertices = new Vector3[65535];
    static Vector3[] vertexOffsets = new Vector3[65535];
    static Vector3[] vertexNormals = new Vector3[65535];
    static int[] triangles = new int[65535];
    static int createdTriangles;


    static int currentNumberOfVertices => vertexDictionary.Count;

    public static void GenerateMesh(ref Mesh mesh, Vector3Int dimensions, System.Func<Vector3Int, float> obtainValueAtPosition, System.Func<Vector3, float> obtainSignedDistanceField, int scale)
    {
        // Establish that no mesh data has been assigned yet
        //existingVertexPositions.Clear();
        vertexDictionary.Clear();
        //createdVertices = 0;
        createdTriangles = 0;

        // Perform necessary function to build mesh
        //GenerateCubeMesh(dimensions, obtainValueAtPosition);
        DualContouring(dimensions, obtainValueAtPosition, obtainSignedDistanceField);
        //BorkedPseudoDualContouringThing(dimensions, obtainValueAtPosition);

        FinaliseMesh(ref mesh, scale);
    }





    static void GenerateCubeMesh(Vector3Int dimensions, System.Func<Vector3Int, float> obtainValueAtPosition)
    {
        // Add faces for each grid position
        MiscFunctions.IterateThroughGrid(dimensions, (x, y, z) =>
        {
            // Get coordinates of grid space, as an easily modifiable vector
            Vector3Int coords = new Vector3Int(x, y, z);

            // Don't make any faces around this point if it's empty
            float spaceValue = obtainValueAtPosition(coords);
            if (spaceValue <= 0) return;

            // Get data on neighbouring faces first, so when creating faces we can check multiple neighbours at once
            int numberOfNeighbours = 0;
            for (int d = 0; d < 6; d++)
            {
                Vector3Int neighbour = coords + adjacencies[d];
                float value = obtainValueAtPosition(neighbour);
                valuesOfNeighbours[d] = value; // These values are overwritten for each grid space, but the array itself is reused to avoid garbage collection.
                if (value > 0) numberOfNeighbours++;
            }

            for (int n = 0; n < 6; n++)
            {
                // Get the value for the adjacent square
                // Don't proceed if the mesh there is filled.
                // There might theoretically be edge cases with tiny gaps of smoke, but that's not worth the hassle of generating those extra faces.
                if (valuesOfNeighbours[n] > 0) continue;

                // Figure out desired vertices for this face
                for (int c = 0; c < 4; c++)
                {
                    // Use cornersForFaces to get the correct corner indices for this face direction.
                    // Use those indices to get the correct offsets for the desired vertex positions.
                    // Vertex grid dimensions are equivalent to value grid dimensions plus one on each side
                    // To get the vertices on either side of a grid position on an axis, get that axis, then plus 1 for the other.
                    Vector3Int vertexGridPos = coords + corners[faceCornersForAdjacentSquares[n][c]];

                    /*
                    // Get the order of this vertex in the list. If it doesn't exist, add it.
                    int indexOrder = existingVertexPositions.IndexOf(vertexGridPos);
                    //int indexOrder = MiscFunctions.IndexOfInArray(existingVertexPositions, vertexGridPos, 0, createdVertices);
                    if (indexOrder < 0)
                    {
                        indexOrder = existingVertexPositions.Count;
                        existingVertexPositions.Add(vertexGridPos);
                        // TO DO: will existingVertexPositions be necessary, if all the vertex offsets are applied in this loop? 
                        vertices[indexOrder] = vertexGridPos - globalVertexOffset;
                        vertexOffsets[indexOrder] = Vector3.zero;
                    }
                    */

                    // Get the order of this vertex in the list. If it doesn't exist, add it.
                    if (vertexDictionary.TryGetValue(vertexGridPos, out int indexOrder) == false)
                    {
                        indexOrder = vertexDictionary.Count;
                        vertexDictionary.Add(vertexGridPos, indexOrder);
                        vertices[indexOrder] = vertexGridPos - globalVertexOffset;
                        vertexOffsets[indexOrder] = Vector3.zero;
                    }

                    // Write the desired vertices for each corner to an array
                    cornerVertexListPositions[c] = indexOrder;
                }

                // Create triangles based on the vertices we selected
                triangles[createdTriangles] = cornerVertexListPositions[0];
                triangles[createdTriangles + 1] = cornerVertexListPositions[1];
                triangles[createdTriangles + 2] = cornerVertexListPositions[2];
                triangles[createdTriangles + 3] = cornerVertexListPositions[3];
                triangles[createdTriangles + 4] = cornerVertexListPositions[2];
                triangles[createdTriangles + 5] = cornerVertexListPositions[1];
                createdTriangles += 6;
            }
        });

        #region Generate vertices

        // For each vertex, adjust position based on fill values of surrounding spaces
        //createdVertices = vertexDictionary.Count;
        for (int v = 0; v < currentNumberOfVertices; v++)
        {
            //vertices[v] += vertexOffsets[v];
            //bool vertexCalculated = FindVertexForDualContouringSpace(vertices[v], obtainValueAtPosition, out Vector3 vertex);
            bool vertexCalculated = DualContouringThingExceptUsingSeparateVerticesFromGridPositions(vertices[v], obtainValueAtPosition, out Vector3 vertex);
            vertices[v] = vertex;

        }

        #endregion
    }


















    /// <summary>
    /// Once vertices and triangles are calculated, adjust everything to 
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="scale"></param>
    static void FinaliseMesh(ref Mesh mesh, int scale)
    {
        // Clear mesh and cancel if there are no triangles to render
        if (currentNumberOfVertices < 3 || createdTriangles < 3)
        {
            mesh.Clear();
            return;
        }

        // Divide vertex positions to ensure they fit the grid scale.
        // As long as all the vertices have the same relative positions, and the indices stay the same, the mesh shape will be the same.
        for (int v = 0; v < currentNumberOfVertices; v++)
        {
            vertices[v] /= scale;

            Debug.DrawRay(vertices[v], vertexNormals[v], Color.green);
            for (int i = 0; i < 6; i++)
            {
                Debug.DrawRay(vertices[v], 0.1f * (Vector3)adjacencies[i], Color.blue);
            }
        }

        // Assign vertex and triangle data to mesh
        mesh.Clear();
        mesh.SetVertices(vertices, 0, currentNumberOfVertices);
        mesh.SetTriangles(triangles, 0, createdTriangles, 0, false);
        // TO DO: set UVs?
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }



    /// <summary>
    /// IMPORTANT NOTE: I don't really understand how this code works, it was copied from ChatGPT and I haven't properly tested it yet.
    /// </summary>
    /// <param name="posA"></param>
    /// <param name="posB"></param>
    /// <param name="posC"></param>
    /// <param name="normA"></param>
    /// <param name="normB"></param>
    /// <param name="normC"></param>
    /// <param name="intersectionPoint"></param>
    /// <returns></returns>
    public static bool DoPlanesIntersect(Vector3 posA, Vector3 posB, Vector3 posC, Vector3 normA, Vector3 normB, Vector3 normC, out Vector3 intersectionPoint)
    {
        float dotA = Vector3.Dot(posA, normA);
        float dotB = Vector3.Dot(posB, normB);
        float dotC = Vector3.Dot(posC, normC);

        Vector3 crossA = Vector3.Cross(normB, normC);
        float denominator = Vector3.Dot(normA, crossA);

        if (Mathf.Abs(denominator) < Mathf.Epsilon)
        {
            intersectionPoint = new Vector3();
            return false;
        }

        Vector3 crossB = Vector3.Cross(normC, normA);
        Vector3 crossC = Vector3.Cross(normA, normB);

        intersectionPoint = ((crossA * dotA) + (crossB * dotB) + (crossC * dotC)) / denominator;

        return true;
    }

    #endregion









    // I think that the 'intersect_function' is meant to be a stand-in for getting the value inbetween grid spaces.
    // Except separately on each axis.




    public static float MidpointBetweenGridSpaces(Vector3 position, System.Func<Vector3Int, float> obtainValueAtGridPoint, bool debug = false)
    {
        //return obtainValueAtGridPoint.Invoke(Vector3Int.RoundToInt(position));
        
        
        Vector3Int min = Vector3Int.FloorToInt(position);
        //Vector3Int max = Vector3Int.CeilToInt(position);
        Vector3 lerps = position - min;

        for (int i = 0; i < 8; i++)
        {
            Vector3Int coords = min + corners[i];
            valuesAroundVertex[i] = obtainValueAtGridPoint.Invoke(coords);
            //Debug.Log($"Midpoint check: {coords}, {valuesAroundVertex[i]}");
        }

        float x1 = Mathf.LerpUnclamped(valuesAroundVertex[0], valuesAroundVertex[4], lerps.x);
        float x2 = Mathf.LerpUnclamped(valuesAroundVertex[1], valuesAroundVertex[5], lerps.x);
        float x3 = Mathf.LerpUnclamped(valuesAroundVertex[2], valuesAroundVertex[6], lerps.x);
        float x4 = Mathf.LerpUnclamped(valuesAroundVertex[3], valuesAroundVertex[7], lerps.x);

        float y1 = Mathf.LerpUnclamped(x1, x2, lerps.y);
        float y2 = Mathf.LerpUnclamped(x3, x4, lerps.y);

        float z = Mathf.LerpUnclamped(y1, y2, lerps.z);

        if (debug)
        {
            //Debug.Log($"{position}, {min}, {min + Vector3Int.one}");
            //Debug.Log($"Midpoint value = {z}, from {valuesAroundVertex[0]}, {valuesAroundVertex[1]}, {valuesAroundVertex[2]}, {valuesAroundVertex[3]}, {valuesAroundVertex[4]}, {valuesAroundVertex[5]}, {valuesAroundVertex[6]}, {valuesAroundVertex[7]}");
        }

        return z;
    }

    static Vector3 NormalFromDerivatives(Vector3 position, System.Func<Vector3, float> f, float derivative = 0.01f)
    {
        float x = position.x;
        float y = position.y;
        float z = position.z;

        float divider = 2 / derivative;

        float xDiff = (f.Invoke(new(x + derivative, y, z)) - f.Invoke(new(x - derivative, y, z))) / divider;
        float yDiff = (f.Invoke(new(x, y + derivative, z)) - f.Invoke(new(x, y - derivative, z))) / divider;
        float zDiff = (f.Invoke(new(x, y, z + derivative)) - f.Invoke(new(x, y, z - derivative))) / divider;
        return new Vector3(xDiff, yDiff, zDiff).normalized;

        // This version takes up more lines, but could be considered easier to read and has less stuff repeated.
        /*
        Vector3 normal = new Vector3();
        for (int i = 0; i < 3; i++)
        {
            Vector3 plus = position;
            plus[i] += derivative;
            Vector3 minus = position;
            minus[i] -= derivative;

            normal[i] = (f.Invoke(plus) - f.Invoke(minus)) / divider;
        }
        return normal.normalized;
        */
    }









    static Vector3 FDerivative(Vector3 pos, float value, float derivative)
    {
        return NormalFromDerivatives(pos, (v) =>
        {
            return value - MiscFunctions.CubeRoot((v.x * v.x) + (v.y * v.y) + (v.z * v.z));
        }, derivative);
    }



}