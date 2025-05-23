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
    [SerializeField] Transform playerIndicator;

    [Header("Doors")]
    [SerializeField] Mesh doorMesh;
    [SerializeField] Material doorMaterial;
    [SerializeField] Vector3 doorSize = Vector3.one;
    [SerializeField] Color doorColour = Color.green;
    [SerializeField] Color doorLockedColour = Color.red;
    [SerializeField] Color doorBlockedColour = Color.grey;
    //[SerializeField] RectTransform doorLegend;

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

    TerrainGrid terrainGrid => TerrainGrid.current;

    private void Awake()
    {
        terrainMap.player = player;

        openDoor = new MaterialPropertyBlock();
        openDoor.SetColor("_Color", doorColour);
        lockedDoor = new MaterialPropertyBlock();
        lockedDoor.SetColor("_Color", doorLockedColour);
        blockedDoor = new MaterialPropertyBlock();
        blockedDoor.SetColor("_Color", doorBlockedColour);
    }
    private void OnEnable()
    {
        //terrainMap.UpdateTextureToMatch3DArray();
        
        // Assign mesh to renderer
        mapMeshFilter.mesh = terrainGrid.mapMesh;

        // Assign values to map material
        Vector4 gridMinAsVector4 = new Vector4(terrainGrid.worldBoundsMin.x, terrainGrid.worldBoundsMin.y, terrainGrid.worldBoundsMin.z, 0);
        Vector4 gridMaxAsVector4 = new Vector4(terrainGrid.worldBoundsMax.x, terrainGrid.worldBoundsMax.y, terrainGrid.worldBoundsMax.z, 0);
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

        camera.enabled = true;
    }
    private void OnDisable()
    {
        camera.enabled = false;
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

        playerIndicator.localPosition = player.transform.position;
        playerIndicator.localRotation = player.transform.rotation;

        foreach (Door door in terrainGrid.doorsInScene)
        {
            // Check if the door's position on the map is filled in
            Vector3 worldPosition = door.transform.position;

            // Don't show the door if it's not in an area the player has explored
            Vector3Int gridPosition = terrainGrid.WorldToGridPosition(worldPosition);
            if (terrainMap.autoMapped == false && terrainMap.GetFill(gridPosition) <= 0) continue;
            
            worldPosition.y += doorSize.y * 0.5f;
            Matrix4x4 matrix = Matrix4x4.TRS(worldPosition, door.transform.rotation, doorSize);
            matrix = mapRenderer.transform.localToWorldMatrix * matrix;

            // Calculate correct colour to use
            MaterialPropertyBlock propertyBlock = door.isLocked ? (door.lockingMechanism == null ? blockedDoor : lockedDoor) : openDoor;

            // Draw icon for door
            Graphics.DrawMesh(doorMesh, matrix, doorMaterial, renderLayer, camera, 0, propertyBlock);
        }
        /*
        Matrix4x4 doorLegendMatrix = doorLegend.localToWorldMatrix;
        doorLegendMatrix *= Matrix4x4.TRS(Vector3.zero, Quaternion.identity, doorSize);
        Graphics.DrawMesh(doorMesh, doorLegendMatrix, doorMaterial, renderLayer, camera, 0, lockedDoor);
        */
        //Graphics.Dr

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