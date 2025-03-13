using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class MapScreen : MonoBehaviour
{
    public Player player;

    [Header("Rendering")]
    public Camera camera;
    public RawImage outputTextureDisplay;

    [Header("Terrain")]
    public TerrainMap terrainMap;
    public MeshRenderer mapRenderer;
    public MeshFilter mapMeshFilter;
    [SerializeField] string shaderBoundsMin = "_Local_Bounds_Min";
    [SerializeField] string shaderBoundsMax = "_Local_Bounds_Max";
    [SerializeField] string shader3DTexture = "_Areas_filled";

    Vector2Int lastResolution;

    public RenderTexture outputTexture { get; private set; }
    int renderLayer => mapRenderer.gameObject.layer;
    Camera playerCamera => player.movement.lookControls.worldViewCamera;

    private void Awake()
    {
        terrainMap.playerTransform = player.transform;
    }
    private void OnEnable()
    {
        // Assign mesh to renderer
        mapMeshFilter.mesh = terrainMap.mapMesh;

        // Assign values to map material
        Vector4 gridMinAsVector4 = new Vector4(terrainMap.boundsMin.x, terrainMap.boundsMin.y, terrainMap.boundsMin.z, 0);
        Vector4 gridMaxAsVector4 = new Vector4(terrainMap.boundsMax.x, terrainMap.boundsMax.y, terrainMap.boundsMax.z, 0);
        Material m = mapRenderer.material;
        m.SetVector(shaderBoundsMin, gridMinAsVector4);
        m.SetVector(shaderBoundsMax, gridMaxAsVector4);
        m.SetTexture(shader3DTexture, terrainMap.progressTexture);

        //RenderPipelineManager.beginCameraRendering += RenderIcons;
        camera.enabled = true;
    }
    private void OnDisable()
    {
        camera.enabled = false;
        //RenderPipelineManager.beginCameraRendering -= RenderIcons;
    }
    private void LateUpdate()
    {
        // Update output render texture
        Vector2Int currentResolution = new Vector2Int(playerCamera.scaledPixelWidth, playerCamera.scaledPixelHeight);
        if (outputTexture == null || lastResolution.x != currentResolution.x || lastResolution.y != currentResolution.y)
        {
            outputTexture = new RenderTexture(currentResolution.x, currentResolution.y, 32, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Default);
            camera.targetTexture = outputTexture;
            outputTextureDisplay.texture = outputTexture;
            lastResolution = currentResolution;
        }
    }
}