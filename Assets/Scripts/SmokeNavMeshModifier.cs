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
    [SerializeField] float refreshDelay = 0.5f;
    [SerializeField] float cellWidth = 1;

    Octree<NavMeshModifierVolume> octree;
    Octree<bool> terrainOctree;
    Matrix4x4 octreeToWorldMatrix;
    float lastTimeRefreshed;
    int totalCellCount;



    //Dictionary<BoundsInt, bool> cellContainsTerrain = new Dictionary<BoundsInt, bool>();


    IEnumerator refreshNavMeshVolumes;
    int cellsRefreshed;

    NavMeshSurface[] navMeshes => TerrainGrid.current.navMeshes;

    void Start()
    {
        Setup();
    }
    void Update()
    {
        // Trigger refreshing on a timer to save performance, since it doesn't need to be done every frame
        if (Time.time - lastTimeRefreshed > refreshDelay)
        {
            // Start refreshing.
            // Create a new iterator and keep track of the last time we refreshed this.
            octree.Scan();

            // Figure out how many cells we need to process
            totalCellCount = 0;
            IEnumerator iterator = octree.baseContainer.CellIterator();
            while (iterator.MoveNext()) totalCellCount++;

            refreshNavMeshVolumes = octree.baseContainer.ProcessCells();
            lastTimeRefreshed = Time.time;
            cellsRefreshed = 0;
        }


        // Iterate through octree, updating NavMeshModifierVolumes
        if (refreshNavMeshVolumes != null)
        {
            // Calculate how many frames we need to refresh over, and how many cells to refresh per frame
            float framesToIterateOver = 1;
            if (Time.timeScale != 0) framesToIterateOver = refreshDelay / Time.deltaTime;
            int cellsToRefreshPerFrame = Mathf.CeilToInt(totalCellCount / framesToIterateOver);

            Debug.Log($"{totalCellCount}, {framesToIterateOver}, {cellsToRefreshPerFrame}");

            for (int i = 0; i < cellsToRefreshPerFrame; i++)
            {
                // Iterate through enumerator up to a count of 'cellsRefreshedPerFrame', or until the enumerator ends.
                if (refreshNavMeshVolumes.MoveNext())
                {
                    cellsRefreshed++;
                    continue;
                }

                // Once the enumerator has ended, delete it and update the navmeshes
                for (int n = 0; n < navMeshes.Length; n++)
                {
                    NavMeshSurface mesh = navMeshes[n];
                    if (mesh == null) continue;
                    if (mesh.enabled == false) continue;
                    if (mesh.navMeshData == null) continue;
                    // Update NavMesh (do not rebuild the entire thing as that chews through processing budget)
                    mesh.UpdateNavMesh(mesh.navMeshData);
                }

                refreshNavMeshVolumes = null;
                Debug.Log(cellsRefreshed);

                break;
            }
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

        // Set up a second, identically-sized octree, and do a single check for terrain
        terrainOctree = new Octree<bool>();
        terrainOctree.subdivisions = subdivisions;
        terrainOctree.checkOctant = ScanForLevelGeometry;
        terrainOctree.Refresh();

        // Assign delegates to octree
        octree.checkOctant = DoesOctantContainSmoke; // Use box checks to look for smoke colliders
        octree.doesCheckMatter = DoesOctantContainLevelGeometry;
        octree.onOctantRefreshed = CheckToAssignVolume; // Assign NavMeshModifierVolumes for each leaf or full octant
        octree.onOctantRemoved = RemoveVolume; // Clear unwanted volumes

        // TO DO: create an object pool with this transform as the pool parent.
        ObjectPool.TryCreateObjectPool(volumePrefab);
        volumePrefab.gameObject.SetActive(false);

        // Reset transform position, rotation and scale
        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        transform.localScale = Vector3.one;
    }

    /*
    void RefreshMesh()
    {
        // TO DO: make this code work over multiple frames? Even if it's only done on every occasional frame, it still causes lag spikes.
        // Since when you split up an octant it just acts like a smaller octant, I should probably be able to set up some code to only refresh and iterate through parts of the octant per frame.
        // Maybe the refresh function can have ratio floats representing the start and fraction, and based off the volume determine how much it can iterate through

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
    */

    bool DoesOctantContainSmoke(BoundsInt bounds)
    {
        return DoesOctantContainColliders(bounds.min, bounds.max, smokeMask.mask);
    }
    bool ScanForLevelGeometry(BoundsInt bounds)
    {
        // TO DO: I think I need to improve the check further.
        // Maybe instead of just checking for level geometry, I check for NavMesh geometry specifically.
        return DoesOctantContainColliders(bounds.min, bounds.max, terrainMask);
    }
    bool DoesOctantContainColliders(Vector3Int min, Vector3Int max, LayerMask layerMask)
    {
        GetWorldExtents(min, max, octreeToWorldMatrix, out Vector3 centre, out Vector3 halfExtents);
        return Physics.CheckBox(centre, halfExtents, Quaternion.identity, layerMask);
    }
    bool DoesOctantContainLevelGeometry(Octant<NavMeshModifierVolume> octant)
    {

        Octant<bool> matchingOctant = terrainOctree.FindMatchingOctant(octant);
        if (matchingOctant == null) return false;
        return matchingOctant.type != OctantType.Empty;
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