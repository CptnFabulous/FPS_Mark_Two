using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class TerrainGrid : MonoBehaviour
{
    #region Singleton

    public static TerrainGrid current
    {
        get
        {
            Scene currentScene = SceneManager.GetActiveScene();
            if (lastLoadedScene != currentScene)
            {
                _current = FindObjectOfType<TerrainGrid>();
                lastLoadedScene = currentScene;
            }
            return _current;
        }
    }

    static TerrainGrid _current;
    static Scene lastLoadedScene;

    #endregion

    public int resolutionScale = 1;
    public LayerMask terrainDetection;
    public Texture3D debugTerrainTexture;

    Vector3Int _gridBoundsMin;
    Vector3Int _gridBoundsMax;
    bool[,,] _occupiedByTerrain;
    Vector3Int _textureResolution;
    
    public Mesh mapMesh { get; private set; }
    public Bounds bounds { get; private set; }
    public Vector3Int gridBoundsMin => _gridBoundsMin;
    public Vector3Int gridBoundsMax => _gridBoundsMax;
    public bool[,,] containsTerrain => _occupiedByTerrain;
    public Vector3Int textureResolution => _textureResolution;

    public Door[] doorsInScene { get; private set; }

    private void Awake()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        #region Create mesh from NavMesh data

        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        mapMesh = new Mesh();
        mapMesh.SetVertices(triangulation.vertices);
        mapMesh.SetIndices(triangulation.indices, MeshTopology.Triangles, 0);
        mapMesh.RecalculateBounds();
        mapMesh.RecalculateNormals();
        mapMesh.RecalculateTangents();

        #endregion

        #region Create grid bounds

        bounds = mapMesh.bounds;
        _textureResolution = Vector3Int.zero;
        for (int i = 0; i < 3; i++)
        {
            _gridBoundsMin[i] = Mathf.FloorToInt(bounds.min[i]);
            _gridBoundsMax[i] = Mathf.CeilToInt(bounds.max[i]);
            _textureResolution[i] = gridBoundsMax[i] - gridBoundsMin[i];
            _textureResolution[i] *= resolutionScale;
        }

        #endregion

        #region Create 3D texture from geometry

        debugTerrainTexture = new Texture3D(textureResolution.x, textureResolution.y, textureResolution.z, TextureFormat.ARGB32, false);
        _occupiedByTerrain = new bool[textureResolution.x, textureResolution.y, textureResolution.z];

        Vector3 checkBoxHalfExtents = 0.5f / resolutionScale * Vector3.one;

        Vector3Int min = Vector3Int.zero;
        Vector3Int max = textureResolution;
        for (int x = min.x; x < max.x; x++)
        {
            for (int y = min.y; y < max.y; y++)
            {
                for (int z = min.z; z < max.z; z++)
                {
                    Vector3 worldPos = WorldPositionFromTextureCoordinates(new Vector3Int(x, y, z));
                    bool containsTerrain = Physics.CheckBox(worldPos, checkBoxHalfExtents, Quaternion.identity, terrainDetection);
                    _occupiedByTerrain[x, y, z] = containsTerrain;

                    debugTerrainTexture.SetPixel(x, y, z, containsTerrain ? Color.white : Color.clear);
                }
            }
        }
        debugTerrainTexture.Apply();

        #endregion

        doorsInScene = FindObjectsOfType<Door>();
    }
    public Vector3Int TextureCoordinatesFromWorldPosition(Vector3 position)
    {
        Vector3Int rounded = Vector3Int.zero;
        for (int i = 0; i < 3; i++)
        {
            rounded[i] = Mathf.RoundToInt(position[i]);
            rounded[i] -= gridBoundsMin[i];
            rounded[i] *= resolutionScale;
        }
        return rounded;
    }
    public Vector3 WorldPositionFromTextureCoordinates(Vector3Int coordinates)
    {
        for (int i = 0; i < 3; i++)
        {
            coordinates[i] /= resolutionScale;
        }
        coordinates += gridBoundsMin;
        return coordinates;
    }
}
