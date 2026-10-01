using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using CptnFabulous.ObjectPool;

public class SmokeNavMeshModifier : MonoBehaviour
{
    [SerializeField] DetectionProfile smokeMask;
    [SerializeField] LayerMask terrainMask = ~0;
    [SerializeField] NavMeshModifierVolume volumePrefab;
    [SerializeField] float refreshTimer = 0.2f;
    [SerializeField] float cellWidth = 1;

    Octree<NavMeshModifierVolume> octree;
    Matrix4x4 octreeToWorldMatrix;
    float lastTimeRefreshed;

    NavMeshSurface[] navMeshes => TerrainGrid.current.navMeshes;

    void Start()
    {
        Setup();
    }
    void Update()
    {
        // Trigger refreshing on a timer to save performance, since it doesn't need to be done every frame
        if (Time.time - lastTimeRefreshed > refreshTimer)
        {
            RefreshMesh();
            lastTimeRefreshed = Time.time;
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (octree == null) return;

        Gizmos.matrix = octreeToWorldMatrix;
        octree.DrawGizmos();
    }

    void Setup()
    {
        // Create octree, and determine how big it needs to be
        octree = new Octree<NavMeshModifierVolume>();
        TerrainGrid.GenerateOctreeDataForLevelBounds(cellWidth, out _, out int subdivisions, out octreeToWorldMatrix);
        octree.subdivisions = subdivisions;

        // Assign delegates to octree
        octree.checkOctant = DoesOctantContainSmoke; // Use box checks to look for smoke colliders
        octree.onOctantRefreshed = CheckToAssignVolume; // Assign NavMeshModifierVolumes for each leaf or full octant
        octree.onOctantRemoved = RemoveVolume; // Clear unwanted volumes

        // TO DO: create an object pool with this transform as the pool parent.
        ObjectPool.TryCreateObjectPool(volumePrefab);
        volumePrefab.gameObject.SetActive(false);

        // Reset transform position, rotation and scale
        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        transform.localScale = Vector3.one;
    }
    void RefreshMesh()
    {
        // Refresh octree (the delegates inside it will do the appropriate checks and assign the necessary volumes)
        octree.Refresh();

        // Rebuild NavMesh
        for (int i = 0; i < navMeshes.Length; i++)
        {
            NavMeshSurface mesh = navMeshes[i];
            if (mesh == null) continue;
            if (mesh.enabled == false) continue;
            if (mesh.navMeshData == null) continue;
            // Update NavMesh (do not rebuild the entire thing as that chews through processing budget)
            mesh.UpdateNavMesh(mesh.navMeshData);
        }
    }

    bool DoesOctantContainSmoke(BoundsInt bounds)
    {
        return DoesOctantContainColliders(bounds.min, bounds.max, smokeMask.mask);
    }
    bool DoesOctantContainColliders(Vector3Int min, Vector3Int max, LayerMask layerMask)
    {
        GetWorldExtents(min, max, octreeToWorldMatrix, out Vector3 centre, out Vector3 halfExtents);
        return Physics.CheckBox(centre, halfExtents, Quaternion.identity, layerMask);
    }
    void CheckToAssignVolume(Octant<NavMeshModifierVolume> octant, ref NavMeshModifierVolume volume)
    {
        // Don't have a volume if space is empty, or needs to be broken down further
        if (octant.type == OctantType.Branch) return;
        if (octant.type == OctantType.Empty) return;

        // Ensure a volume is present (instantiate if necessary, but do not duplicate)
        if (volume == null) volume = ObjectPool.RequestObject(volumePrefab);

        // Set volume position and size
        GetWorldExtents(octant.min, octant.max, octreeToWorldMatrix, out Vector3 centre, out Vector3 halfExtents);
        volume.transform.localPosition = centre;
        volume.size = 2 * halfExtents;
    }
    void RemoveVolume(Octant<NavMeshModifierVolume> octant)
    {
        ObjectPool.DismissObject(octant.leafData);
    }

    void GetWorldExtents(Vector3Int min, Vector3Int max, Matrix4x4 matrix, out Vector3 centre, out Vector3 halfExtents)
    {
        // Create bounds
        Bounds bounds = new Bounds();
        bounds.min = min;
        bounds.max = max;
        // Convert position and size values from local to world space
        centre = matrix.MultiplyPoint3x4(bounds.center);
        halfExtents = matrix.MultiplyVector(bounds.extents);
    }
}