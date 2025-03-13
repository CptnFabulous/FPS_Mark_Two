using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class TerrainMap : MonoBehaviour
{
    public Transform playerTransform;
    public bool autoMapped;

    [Header("Fill texture")]
    public Texture3D progressTexture;
    [SerializeField] AnimationCurve fillRadiusCurve = AnimationCurve.Linear(6, 1, 8, 0);
    [SerializeField] int resolutionScale = 1;
    [SerializeField] TextureFormat textureFormat = TextureFormat.ARGB32;
    [SerializeField] FilterMode filterMode = FilterMode.Point;

    Scene sceneOfMap;

    Vector3Int _boundsMin;
    Vector3Int _boundsMax;
    Vector3Int _textureResolution;

    Vector3 lastPosition;

    public Mesh mapMesh { get; private set; }
    public Vector3Int boundsMin => _boundsMin;
    public Vector3Int boundsMax => _boundsMax;
    public Vector3Int textureResolution => _textureResolution;


    //Color[,,] fillArray;

    public Door[] doorsInScene { get; private set; }

    private void OnEnable()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene == sceneOfMap) return;

        sceneOfMap = currentScene;
        GenerateMap();
    }
    private void Update()
    {
        if (playerTransform.position == lastPosition) return;
        lastPosition = playerTransform.position;

        // Get position of player, in map's local space
        Vector3Int playerLocalPosition = TextureCoordinatesFromWorldPosition(playerTransform.position);

        int detectionRadius = Mathf.RoundToInt(fillRadiusCurve[fillRadiusCurve.length - 1].time);

        // Calculate pixels that need to be checked this frame
        Vector3Int min = playerLocalPosition;
        Vector3Int max = playerLocalPosition;
        int gridRadius = detectionRadius * resolutionScale;
        for (int i = 0; i < 3; i++)
        {
            min[i] -= gridRadius;
            max[i] += gridRadius;
            // Values are clamped to ensure they don't exceed texture bounds.
            // This prevents null errors in case the player is close to or outside the map bounds
            min[i] = Mathf.Clamp(min[i], 0, textureResolution[i] - 1);
            max[i] = Mathf.Clamp(max[i], 0, textureResolution[i] - 1);
        }

        // Paint in texture based on where player has been
        UpdateTexture(min, max, (x, y, z) =>
        {
            Vector3Int coordinates = new Vector3Int(x, y, z);
            Color pixelColour = progressTexture.GetPixel(x, y, z);

            // No need to update fill if already filled in completely
            float oldFill = pixelColour.a;
            if (oldFill >= 1) return pixelColour;

            // If distance is too far away, return the standard colour
            float distance = Vector3.Distance(coordinates, playerLocalPosition);
            distance /= resolutionScale;
            if (distance > detectionRadius) return pixelColour;

            // Calculate area fill - if it's less than the current value, ignore
            float fill = fillRadiusCurve.Evaluate(distance);
            fill = Mathf.Clamp01(fill);
            if (fill <= oldFill) return pixelColour;

            /*
            // Ensure line of sight between player and point
            Vector3 worldPosition = WorldPositionFromTextureCoordinates(coordinates);
            bool lineOfSight = AIAction.LineOfSight(player.lookOrigin, worldPosition, geometryDetection, player.colliders);
            if (lineOfSight == false) return pixelColour;
            */

            return new Color(1, 1, 1, fill);
        });
    }
    
    void GenerateMap()
    {
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();

        // Create mesh from NavMesh data
        mapMesh = new Mesh();
        mapMesh.SetVertices(triangulation.vertices);
        mapMesh.SetIndices(triangulation.indices, MeshTopology.Triangles, 0);
        mapMesh.RecalculateBounds();
        mapMesh.RecalculateNormals();
        mapMesh.RecalculateTangents();

        // Generate texture from bounds
        Bounds bounds = mapMesh.bounds;

        _textureResolution = Vector3Int.zero;
        for (int i = 0; i < 3; i++)
        {
            _boundsMin[i] = Mathf.FloorToInt(bounds.min[i]);
            _boundsMax[i] = Mathf.CeilToInt(bounds.max[i]);
            _textureResolution[i] = boundsMax[i] - boundsMin[i];
            _textureResolution[i] *= resolutionScale;
        }
        progressTexture = new Texture3D(textureResolution.x, textureResolution.y, textureResolution.z, textureFormat, false);
        progressTexture.name = $"Progress texture for {playerTransform.name}";
        progressTexture.filterMode = filterMode;

        // Reset entire texture to be clear
        // TO DO: once saving and loading are implemented properly, have this data be assigned from the save file
        UpdateTexture(Vector3Int.zero, textureResolution, (_, _, _) => Color.clear);

        doorsInScene = FindObjectsOfType<Door>();
    }
    void UpdateTexture(Vector3Int min, Vector3Int max, System.Func<int, int, int, Color> colourAssignment)
    {
        for (int x = min.x; x < max.x; x++)
        {
            for (int y = min.y; y < max.y; y++)
            {
                for (int z = min.z; z < max.z; z++)
                {
                    Color c = colourAssignment.Invoke(x, y, z);
                    progressTexture.SetPixel(x, y, z, c);
                    //fillArray[x, y, z] = c;
                }
            }
        }
        progressTexture.Apply();
    }
    public Vector3Int TextureCoordinatesFromWorldPosition(Vector3 position)
    {
        Vector3Int rounded = Vector3Int.zero;
        for (int i = 0; i < 3; i++)
        {
            rounded[i] = Mathf.RoundToInt(position[i]);
            rounded[i] -= boundsMin[i];
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
        coordinates += boundsMin;
        return coordinates;
    }
    public float GetFill(Vector3Int coordinates)
    {
        return progressTexture.GetPixel(coordinates.x, coordinates.y, coordinates.z).a;
    }
}