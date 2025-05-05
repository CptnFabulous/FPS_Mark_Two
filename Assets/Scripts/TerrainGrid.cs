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

    public LayerMask terrainDetection;
    public Vector3 boundsMargins = Vector3.one;
    public int resolutionScale = 1;
    public Texture3D debugTerrainTexture;

    bool generated = false;
    Bounds _bounds;
    BoundsInt _gridBounds;
    bool[,,] _occupiedByTerrain;
    Vector3Int _textureResolution;
    
    public Mesh mapMesh { get; private set; }
    public Bounds bounds => _bounds;
    public BoundsInt gridBounds => _gridBounds;
    public Vector3Int gridBoundsMin => gridBounds.min;
    public Vector3Int gridBoundsMax => gridBounds.max;
    public bool[,,] containsTerrain => _occupiedByTerrain;
    public Vector3Int textureResolution => _textureResolution;

    public Door[] doorsInScene { get; private set; }

    private void Awake()
    {
        GenerateMap();
    }
    private void OnDrawGizmosSelected()
    {
        if (generated == false) return;

        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }

    void GenerateMap()
    {
        if (generated == false) return;

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

        _bounds = mapMesh.bounds;
        // Expand bounds by margins, to ensure nothing is left out
        _bounds.extents += boundsMargins;

        Vector3Int min = new Vector3Int();
        Vector3Int max = new Vector3Int();
        for (int i = 0; i < 3; i++)
        {
            min[i] = Mathf.FloorToInt(bounds.min[i]);
            max[i] = Mathf.CeilToInt(bounds.max[i]);
        }
        _gridBounds.min = min;
        _gridBounds.max = max;

        _textureResolution = Vector3Int.zero;
        for (int i = 0; i < 3; i++)
        {
            _textureResolution[i] = max[i] - min[i];
            _textureResolution[i] *= resolutionScale;
        }

        #endregion

        #region Create 3D texture from geometry

        Vector3 checkBoxHalfExtents = 0.5f / resolutionScale * Vector3.one;

        _occupiedByTerrain = new bool[textureResolution.x, textureResolution.y, textureResolution.z];
        debugTerrainTexture = new Texture3D(textureResolution.x, textureResolution.y, textureResolution.z, TextureFormat.ARGB32, false);
        for (int x = 0; x < textureResolution.x; x++)
        {
            for (int y = 0; y < textureResolution.y; y++)
            {
                for (int z = 0; z < textureResolution.z; z++)
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

        generated = true;
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
