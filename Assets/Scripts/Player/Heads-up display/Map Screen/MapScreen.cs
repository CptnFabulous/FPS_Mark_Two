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

    [Header("Doors")]
    [SerializeField] Mesh doorMesh;
    [SerializeField] Material doorMaterial;
    [SerializeField] Vector3 doorSize = Vector3.one;
    [SerializeField] Color doorColour = Color.green;
    [SerializeField] Color doorLockedColour = Color.red;
    [SerializeField] Color doorBlockedColour = Color.grey;

    [Header("Objective markers")]
    [SerializeField] Sprite objectiveMarker;

    Vector2Int lastResolution;

    MaterialPropertyBlock openDoor;
    MaterialPropertyBlock lockedDoor;
    MaterialPropertyBlock blockedDoor;

    public RenderTexture outputTexture { get; private set; }
    int renderLayer => mapRenderer.gameObject.layer;
    public Camera camera => cameraController.camera;
    Camera playerCamera => player.movement.lookControls.worldViewCamera;

    private void Awake()
    {
        terrainMap.playerTransform = player.transform;

        openDoor = new MaterialPropertyBlock();
        openDoor.SetColor("_Color", doorColour);
        lockedDoor = new MaterialPropertyBlock();
        lockedDoor.SetColor("_Color", doorLockedColour);
        blockedDoor = new MaterialPropertyBlock();
        blockedDoor.SetColor("_Color", doorBlockedColour);
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
        Transform playerWorldCameraTransform = player.movement.lookControls.worldViewCamera.transform;
        cameraController.defaultPosition = playerWorldCameraTransform.position;
        cameraController.defaultRotation = playerWorldCameraTransform.rotation;

        // Then reset camera
        Transform cameraAxisTransform = cameraController.cameraAxisTransform;
        /*
        Vector3 positionOffset = mapRenderer.transform.InverseTransformDirection(cameraAxisTransform.position - cameraController.camera.transform.position);
        cameraAxisTransform.localPosition = playerWorldCameraTransform.position + positionOffset;
        */
        cameraAxisTransform.localPosition = playerWorldCameraTransform.position;
        cameraAxisTransform.localRotation = playerWorldCameraTransform.rotation;
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

        foreach (Door door in terrainMap.doorsInScene)
        {
            // Check if the door's position on the map is filled in
            Vector3 worldPosition = door.transform.position;

            // Don't show the door if it's not in an area the player has explored
            Vector3Int gridPosition = terrainMap.TextureCoordinatesFromWorldPosition(worldPosition);
            if (terrainMap.autoMapped == false && terrainMap.GetFill(gridPosition) <= 0) continue;
            
            // Calculate transform data
            Vector3 position = mapRenderer.transform.TransformPoint(worldPosition);
            Quaternion rotation = door.transform.rotation * mapRenderer.transform.rotation;
            Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, doorSize);

            // Calculate correct colour to use
            MaterialPropertyBlock propertyBlock = door.isLocked ? (door.lockingMechanism == null ? blockedDoor : lockedDoor) : openDoor;

            // Draw icon for door
            Graphics.DrawMesh(doorMesh, matrix, doorMaterial, renderLayer, camera, 0, propertyBlock);
        }

        // Draw objective markers
        if (ObjectiveHandler.current != null)
        {
            foreach (Objective objective in ObjectiveHandler.current.allObjectives)
            {
                if (objective.status != ObjectiveStatus.Active) continue;
                if (objective.location == null) continue;

                RenderIcon(objectiveMarker, Color.white, objective.location.Value, Vector2.one);
            }
        }
    }
    void RenderIcon(Sprite sprite, Color colour, Vector3 position, Vector2 scale)
    {
        Vector3 iconPosition = position + (Vector3.up * scale.y);
        WorldSpaceIconDrawer.DrawIcon(sprite, colour, mapRenderer.transform, iconPosition, scale, renderLayer);
    }
}