using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TerrainMap : MonoBehaviour
{
    public Player player;
    
    public Transform playerTransform => player.transform;
    public bool autoMapped;

    [Header("Fill texture")]
    public Texture3D progressTexture;
    [SerializeField] AnimationCurve fillRadiusCurve = AnimationCurve.Linear(6, 1, 8, 0);
    [SerializeField] TextureFormat textureFormat = TextureFormat.ARGB32;
    [SerializeField] FilterMode filterMode = FilterMode.Point;

    Scene sceneOfMap;
    Vector3 lastPosition;

    static TerrainGrid terrainGrid => TerrainGrid.current;
    Vector3Int textureResolution => terrainGrid.textureResolution;

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
        Vector3Int playerLocalPosition = terrainGrid.TextureCoordinatesFromWorldPosition(playerTransform.position);

        int detectionRadius = Mathf.RoundToInt(fillRadiusCurve[fillRadiusCurve.length - 1].time);

        // Calculate pixels that need to be checked this frame
        Vector3Int min = playerLocalPosition;
        Vector3Int max = playerLocalPosition;
        int gridRadius = detectionRadius * terrainGrid.resolutionScale;
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
            //Color pixelColour = new Color(1, 1, 1, fillMap[x, y, z]);

            // No need to update fill if already filled in completely
            float oldFill = pixelColour.a;
            if (oldFill >= 1) return pixelColour;

            // If distance is too far away, return the standard colour
            float distance = Vector3.Distance(coordinates, playerLocalPosition);
            distance /= terrainGrid.resolutionScale;
            if (distance > detectionRadius) return pixelColour;

            // Calculate area fill - if it's less than the current value, ignore
            float fill = fillRadiusCurve.Evaluate(distance);
            fill = Mathf.Clamp01(fill);
            if (fill <= oldFill) return pixelColour;

            // TO DO: fix this, it's too stingy and ignores a lot of stuff the player should be able to see/reach
            /*
            // Ensure line of sight between player and point
            Vector3 worldPosition = WorldPositionFromTextureCoordinates(coordinates);
            bool lineOfSight = AIAction.LineOfSight(player.LookTransform.position, worldPosition, player.lookMask, player.colliders);
            if (lineOfSight == false) return pixelColour;
            */

            return new Color(1, 1, 1, fill);
        });
    }
    
    void GenerateMap()
    {
        
        progressTexture = new Texture3D(textureResolution.x, textureResolution.y, textureResolution.z, textureFormat, false);
        //fillMap = new float[textureResolution.x, textureResolution.y, textureResolution.z];
        progressTexture.name = $"Progress texture for {playerTransform.name}";
        progressTexture.filterMode = filterMode;

        // Reset entire texture to be clear
        // TO DO: once saving and loading are implemented properly, have this data be assigned from the save file
        UpdateTexture(Vector3Int.zero, textureResolution, (_, _, _) => Color.clear);
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
                }
            }
        }
        progressTexture.Apply();
    }
    
    public float GetFill(Vector3Int coordinates)
    {
        return progressTexture.GetPixel(coordinates.x, coordinates.y, coordinates.z).a;
    }
}