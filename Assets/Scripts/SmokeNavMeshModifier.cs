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
    [SerializeField, Range(0, 1)] float refreshTimeMultiplier = 1f;
    [SerializeField] float cellWidth = 1;

    Octree<NavMeshModifierVolume> octree;
    Octree<bool> terrainOctree;
    Matrix4x4 octreeToWorldMatrix;
    float lastTimeRefreshed;
    int totalCellCount;

    IEnumerator<Octant<NavMeshModifierVolume>> refreshNavMeshVolumes;
    int cellsRefreshed;

    NavMeshSurface[] navMeshes => TerrainGrid.current.navMeshes;

    void Start()
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
        octree.refreshOctant = CheckToAssignVolume; // Assign NavMeshModifierVolumes for each leaf or full octant
        octree.clearOctant = RemoveVolume; // Clear unwanted volumes

        // Create an object pool with this transform as the pool parent.
        ObjectPool.TryCreateObjectPool(volumePrefab);
        volumePrefab.gameObject.SetActive(false);

        // Reset transform position, rotation and scale
        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        transform.localScale = Vector3.one;
    }
    void Update()
    {
        /*
        // Trigger refreshing on a timer to save performance, since it doesn't need to be done every frame
        if (Time.time - lastTimeRefreshed > refreshDelay)
        {
            // Scan and process all cells
            octree.Refresh();
            // Update navmeshes
            for (int n = 0; n < navMeshes.Length; n++)
            {
                NavMeshSurface mesh = navMeshes[n];
                if (mesh == null) continue;
                if (mesh.enabled == false) continue;
                if (mesh.navMeshData == null) continue;
                // Update NavMesh (do not rebuild the entire thing as that chews through processing budget)
                mesh.UpdateNavMesh(mesh.navMeshData);
            }
            // Reset timer
            lastTimeRefreshed = Time.time;
        }

        return;
        */
        

        // Trigger refreshing on a timer to save performance, since it doesn't need to be done every frame
        if (Time.time - lastTimeRefreshed > refreshDelay)
        {
            /*
            if (cellsRefreshed < totalCellCount)
            {
                Debug.Log($"Cells refreshed = {cellsRefreshed}/{totalCellCount}, time taken = {Time.time - lastTimeRefreshed}/{refreshDelay}");
            }
            */
            // Figure out which cells 
            octree.Scan();

            lastTimeRefreshed = Time.time;

            // Figure out how many cells we need to process
            totalCellCount = 0;
            IEnumerator<Octant<NavMeshModifierVolume>> iterator = octree.baseContainer.CellIterator(false);
            while (iterator.MoveNext()) totalCellCount++;
            iterator.Dispose();
            // Set the enumerator to start processing cells
            refreshNavMeshVolumes = octree.baseContainer.CellIterator(true);
            cellsRefreshed = 0;
        }

        // Iterate through octree, updating NavMeshModifierVolumes
        if (refreshNavMeshVolumes == null) return;

        // Calculate how many frames we need to refresh over, and how many cells to refresh per frame
        // If timescale or time multiplier is set to zero, just refresh everything at once.
        int cellsToRefreshPerFrame = int.MaxValue;
        if (refreshTimeMultiplier > 0 && Time.timeScale > 0)
        {
            // Err on the higher side, we'd rather get them done too early than too late.
            float framesToIterateOver = refreshDelay / Time.deltaTime * refreshTimeMultiplier;
            cellsToRefreshPerFrame = Mathf.CeilToInt(totalCellCount / framesToIterateOver);
        }

        //Debug.Log($"{totalCellCount}, {framesToIterateOver}, {cellsToRefreshPerFrame}");

        for (int i = 0; i < cellsToRefreshPerFrame; i++)
        {
            // Iterate through enumerator up to a count of 'cellsRefreshedPerFrame', or until the enumerator ends.
            if (refreshNavMeshVolumes.MoveNext())
            {
                //Octant<NavMeshModifierVolume> octant = refreshNavMeshVolumes.Current;
                //Debug.Log($"{octant.type}, {octant.leafData != null}");
                cellsRefreshed++;
                continue;
            }

            // All cells have been processed! Update the navmeshes
            Debug.Log($"Cells refreshed = {cellsRefreshed}/{totalCellCount}" +
                $"\nTime taken = {Time.time - lastTimeRefreshed}/{refreshDelay}" +
                $"\nRough refreshes per frame {cellsToRefreshPerFrame}");
            for (int n = 0; n < navMeshes.Length; n++)
            {
                NavMeshSurface mesh = navMeshes[n];
                if (mesh == null) continue;
                if (mesh.enabled == false) continue;
                if (mesh.navMeshData == null) continue;
                // Update NavMesh instead of rebuilding it, the latter chews through processing budget
                mesh.UpdateNavMesh(mesh.navMeshData);
            }

            // Cancel loop and delete enumerator, to prepare for the next time it needs to be refreshed
            refreshNavMeshVolumes.Dispose();
            refreshNavMeshVolumes = null;
            break;
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (octree == null) return;

        Gizmos.matrix = octreeToWorldMatrix;
        octree.DrawGizmos();
    }

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
        return matchingOctant.type != OctantType.Invalid;
    }
    NavMeshModifierVolume CheckToAssignVolume(Octant<NavMeshModifierVolume> octant)
    {
        // Don't have a volume if space is empty, or needs to be broken down further
        if (octant.type == OctantType.Branch) return RemoveVolume(octant);
        if (octant.type == OctantType.Invalid) return RemoveVolume(octant);

        NavMeshModifierVolume volume = octant.leafData;

        // Ensure a volume is present (instantiate if necessary, but do not duplicate)
        if (volume == null)
        {
            volume = ObjectPool.RequestObject(volumePrefab);
            volume.transform.parent = transform;
            volume.name = $"New volume";
        }
        else
        {
            volume.name = $"Existing volume";
        }

        volume.name += $": {octant.key}, {octant.type}";


        // Set volume position and size
        GetWorldExtents(octant.min, octant.max, octreeToWorldMatrix, out Vector3 centre, out Vector3 halfExtents);
        volume.transform.localPosition = centre;
        volume.size = 2 * halfExtents;

        return volume;
    }
    NavMeshModifierVolume RemoveVolume(Octant<NavMeshModifierVolume> octant)
    {
        

        NavMeshModifierVolume volume = octant.leafData;

        if (volume != null)
        {
            volume.name = $"Deleted volume: {octant.key}, {octant.type}";
        }

        ObjectPool.DismissObject(volume);
        return null;
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