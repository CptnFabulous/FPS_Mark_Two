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
    public MapCameraController cameraController;
    public RawImage outputTextureDisplay;

    [Header("Terrain")]
    public TerrainMap terrainMap;
    public MeshRenderer mapRenderer;
    public MeshFilter mapMeshFilter;
    [SerializeField] string shaderBoundsMin = "_Local_Bounds_Min";
    [SerializeField] string shaderBoundsMax = "_Local_Bounds_Max";
    [SerializeField] string shader3DTexture = "_Areas_filled";

    [Header("Icons")]
    [SerializeField] Sprite playerIcon;

    [Header("Objective markers")]
    [SerializeField] Sprite objectiveMarker;

    Vector2Int lastResolution;

    public RenderTexture outputTexture { get; private set; }
    int renderLayer => mapRenderer.gameObject.layer;
    public Camera camera => cameraController.camera;
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

        // Limit camera clip plane to reduce processing overhead
        camera.farClipPlane = (cameraController.targetBounds.extents.magnitude + cameraController.boundsMargin) * 2;

        // Set 'centre' values for camera control so it resets to the player's position
        Vector3 cameraPos = player.LookTransform.position - (10 * player.transform.forward);
        cameraController.defaultPosition = cameraPos;
        cameraController.defaultRotation = Quaternion.LookRotation(player.transform.position - cameraPos, Vector3.up);

        // Then reset camera
        Camera playerWorldCamera = player.movement.lookControls.worldViewCamera;
        cameraController.cameraTransform.localPosition = playerWorldCamera.transform.position;
        cameraController.cameraTransform.localRotation = playerWorldCamera.transform.rotation;
        cameraController.RecentreCamera();

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

        RenderIcon(playerIcon, Color.white, player.transform.position, Vector2.one);
    }
    void RenderIcon(Sprite sprite, Color colour, Vector3 position, Vector2 scale)
    {
        Vector3 iconPosition = position + (Vector3.up * scale.y);
        WorldSpaceIconDrawer.DrawIcon(sprite, colour, mapRenderer.transform, iconPosition, scale, renderLayer);
    }
}