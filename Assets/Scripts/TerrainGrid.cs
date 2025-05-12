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
    [SerializeField] Vector3Int _chunkSize = new Vector3Int(10, 10, 10);
    public Texture3D debugTerrainTexture;

    bool generated = false;

    BoundsInt _worldBounds;
    Vector3Int _gridSize;
    Vector3Int _chunkGridSize;
    bool[,,] _occupiedByTerrain;
    
    public Mesh mapMesh { get; private set; }
    public BoundsInt worldBounds => _worldBounds;

    public Vector3Int worldBoundsMin => worldBounds.min;
    public Vector3Int worldBoundsMax => worldBounds.max;


    public Vector3Int gridSize => _gridSize;
    public Vector3Int chunkGridSize => _chunkGridSize;
    public Vector3Int chunkSize => _chunkSize;
    public bool[,,] containsTerrain => _occupiedByTerrain;

    public Door[] doorsInScene { get; private set; }

    private void Awake()
    {
        GenerateMap();
    }
    private void OnDrawGizmos()
    {
        if (generated == false) return;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(worldBounds.center, worldBounds.size);
    }

    void GenerateMap()
    {
        if (generated) return;

        #region Create mesh from NavMesh data

        Debug.Log("Creating mesh");

        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        mapMesh = new Mesh();
        mapMesh.SetVertices(triangulation.vertices);
        mapMesh.SetIndices(triangulation.indices, MeshTopology.Triangles, 0);
        mapMesh.RecalculateBounds();
        mapMesh.RecalculateNormals();
        mapMesh.RecalculateTangents();

        #endregion

        #region Create grid bounds

        Debug.Log("Creating grid bounds");

        Bounds meshBounds = mapMesh.bounds;
        // Expand bounds by margins, to ensure nothing is left out
        meshBounds.extents += boundsMargins;

        Vector3Int min = new Vector3Int();
        Vector3Int max = new Vector3Int();
        for (int i = 0; i < 3; i++)
        {
            min[i] = Mathf.FloorToInt(meshBounds.min[i]);
            max[i] = Mathf.CeilToInt(meshBounds.max[i]);
        }
        _worldBounds.min = min;
        _worldBounds.max = max;

        // Multiply int bounds size by resolution scale to get grid scale
        for (int i = 0; i < 3; i++)
        {
            _gridSize[i] = _worldBounds.size[i] * resolutionScale;
        }

        #endregion

        #region Sets up appropriate number of chunks for grid size (and increases bounds and grid sizes to match chunks)

        Debug.Log("Setting up chunks");

        Vector3Int newGridSize = new Vector3Int();
        Vector3Int newWorldBoundsSize = new Vector3Int();
        for (int i = 0; i < 3; i++)
        {
            // Figure out number of chunks needed to cover volume, then expand bounds if number of chunks results in a higher volume.
            float numOfChunks = (float)_gridSize[i] / (float)chunkSize[i];
            _chunkGridSize[i] = Mathf.CeilToInt(numOfChunks);
            newGridSize[i] = _chunkGridSize[i] * chunkSize[i];
            // Adjust world bounds size to proportionally represent changes to grid size
            // If the grid size is always the world bounds size multiplied by the resolution scale, I can just divide the new value to get a proportional world-scale size.
            newWorldBoundsSize[i] = newGridSize[i] / resolutionScale;
        }
        _gridSize = newGridSize;

        // Adjust world bounds so size is changed, but centre is as close to original centre as possible
        Bounds b = new Bounds(_worldBounds.center, newWorldBoundsSize);
        Vector3Int newWorldBoundsMin = Vector3Int.RoundToInt(b.min);
        _worldBounds = new BoundsInt(newWorldBoundsMin, newWorldBoundsSize);

        #endregion

        #region Pre-cache what parts of the level are occupied by terrain

        Debug.Log("Pre-caching terrain grid");

        Vector3 checkBoxHalfExtents = 0.5f / resolutionScale * Vector3.one;

        _occupiedByTerrain = new bool[gridSize.x, gridSize.y, gridSize.z];
        debugTerrainTexture = new Texture3D(gridSize.x, gridSize.y, gridSize.z, TextureFormat.ARGB32, false);
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                for (int z = 0; z < gridSize.z; z++)
                {
                    Vector3 worldPos = GridToWorldPosition(new Vector3Int(x, y, z));
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
    public Vector3Int WorldToGridPosition(Vector3 position)
    {
        Vector3Int rounded = Vector3Int.zero;
        for (int i = 0; i < 3; i++)
        {
            rounded[i] = Mathf.RoundToInt(position[i]);
            rounded[i] -= worldBoundsMin[i];
            rounded[i] *= resolutionScale;
        }
        return rounded;
    }
    public Vector3 GridToWorldPosition(Vector3Int coordinates)
    {
        for (int i = 0; i < 3; i++)
        {
            coordinates[i] /= resolutionScale;
        }
        coordinates += worldBoundsMin;
        return coordinates;
    }
    public void GridToChunkCoords(Vector3Int gridCoords, out Vector3Int chunkLocation, out Vector3Int locationInChunk)
    {
        chunkLocation = new Vector3Int();
        locationInChunk = new Vector3Int();
        for (int i = 0; i < 3; i++)
        {
            chunkLocation[i] = Mathf.FloorToInt(gridCoords[i] / chunkSize[i]);
            locationInChunk[i] = gridCoords[i] % chunkSize[i];
        }
    }
    public Vector3Int ChunkToGridCoords(Vector3Int chunkLocation, Vector3Int locationInChunk)
    {
        Vector3Int gridCoords = new Vector3Int();
        for (int i = 0; i < 3; i++)
        {
            gridCoords[i] = chunkLocation[i] * chunkSize[i];
            gridCoords[i] += locationInChunk[i];
        }

        return gridCoords;
    }
}
