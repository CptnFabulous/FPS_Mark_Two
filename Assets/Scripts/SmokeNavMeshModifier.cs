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

    public int refreshesPerFrame = 200;

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

        // Everything done synchronously on one frame
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

        
        // Attempt at doing everything async, but with different speeds for scanning and processing
        
        if (refreshNavMeshVolumes == null)
        {
            // Set the enumerator to start processing cells
            //refreshNavMeshVolumes = UpdateVolumesAsync(true, refreshesPerFrame * 2, refreshesPerFrame);
            //refreshNavMeshVolumes = UpdateVolumesAsync(refreshesPerFrame, Time.unscaledDeltaTime);
            refreshNavMeshVolumes = UpdateVolumesAsync(refreshesPerFrame, 64);
            lastTimeRefreshed = Time.unscaledTime;
        }

        // Since we have an IEnumerator, iterate through it
        if (refreshNavMeshVolumes.MoveNext()) return;

        // Operation finished, dispose and delete enumerator, to prepare for the next time it needs to be refreshed
        //Debug.Log($"Cells refreshed, time taken = {Time.unscaledTime - lastTimeRefreshed}");
        refreshNavMeshVolumes.Dispose();
        refreshNavMeshVolumes = null;

        
        return;
        


        // Scan and process async, based only on refreshes per frame
        
        for (int i = 0; i < refreshesPerFrame; i++)
        {
            if (refreshNavMeshVolumes == null)
            {
                refreshNavMeshVolumes = UpdateVolumesAsync(false, refreshesPerFrame * 2, refreshesPerFrame);
            }

            // Since we have an IEnumerator, iterate through it
            if (refreshNavMeshVolumes.MoveNext())
            {
                continue;
            }

            // Operation finished, dispose and delete enumerator, to prepare for the next time it needs to be refreshed
            refreshNavMeshVolumes.Dispose();
            refreshNavMeshVolumes = null;
            break;
        }
        return;
        


        // Scan sync, process async, based on refreshes per frame
        
        for (int i = 0; i < refreshesPerFrame; i++)
        {
            if (refreshNavMeshVolumes == null)
            {
                octree.Scan();
                // Set the enumerator to start processing cells
                refreshNavMeshVolumes = octree.baseContainer.CellIterator(true);
                cellsRefreshed = 0;
                lastTimeRefreshed = Time.time;
            }

            // Since we have an IEnumerator, iterate through it
            if (refreshNavMeshVolumes.MoveNext())
            {
                cellsRefreshed++;
                continue;
            }

            // All cells have been processed! Update the navmeshes
            Debug.Log($"{cellsRefreshed} cells refreshed, time taken = {Time.time - lastTimeRefreshed}");
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

        return;
        


        // Scan sync, process async, done over fixed timeframe instead of processes per frame

        // Trigger refreshing on a timer to save performance, since it doesn't need to be done every frame
        if (Time.time - lastTimeRefreshed > refreshDelay)
        {
            //if (cellsRefreshed < totalCellCount) Debug.Log($"Cells refreshed = {cellsRefreshed}/{totalCellCount}, time taken = {Time.time - lastTimeRefreshed}/{refreshDelay}");
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



    IEnumerator<Octant<NavMeshModifierVolume>> UpdateVolumesAsync(int iterationsPerFrame, /*float deltaTime, */int maxCubeSize)
    {
        /*
        // How do I calculate the best number of octants to iterate at once?
        // Calculate how long it'll take to refresh, based on max possible iterations and iterations per frame

        int maxPossibleCells = octree.maxPossibleUniqueCells;
        float estimatedRefreshTime = deltaTime * (maxPossibleCells / iterationsPerFrame);

        // Start depth at 0, increase by 1 until (timeToRefresh / (8 to the power of depth)) <= max desired lag for updating a specific section
        int depth = 0;
        while (estimatedRefreshTime / Mathf.Pow(8, depth) > refreshDelay)
        {
            depth++;
        }
        */


        // Figure out minimum subdivisions until the cubes are small enough to process all at once
        int depth = 0;
        while (Octree<object>.CalculateGridSize(octree.subdivisions - depth) > maxCubeSize)
        {
            depth++;
        }



        // TO DO: with this code, for some reason the old NavMeshModifierVolumes don't seem to be disappearing!
        //throw new System.NotImplementedException();

        //Debug.Log($"Starting refresh, {maxPossibleCells}, {estimatedRefreshTime}, {depth}, {Mathf.Pow(8, depth)}");
        Debug.Log($"Starting SmokeNavMeshModifier FullRefreshIterator, {depth}");
        int progress = 0;
        IEnumerator<Octant<NavMeshModifierVolume>> enumerator = octree.baseContainer.FullRefreshIterator(depth);
        while (enumerator.MoveNext())
        {
            progress++;
            if (progress % iterationsPerFrame == 0) yield return null;
        }
        enumerator.Dispose();

        // All cells have been processed! Update the navmeshes
        //Debug.Log($"{cellsRefreshed} cells refreshed, time taken = {Time.time - lastTimeRefreshed}/{refreshDelay}");
        for (int n = 0; n < navMeshes.Length; n++)
        {
            NavMeshSurface mesh = navMeshes[n];
            if (mesh == null) continue;
            if (mesh.enabled == false) continue;
            if (mesh.navMeshData == null) continue;
            // Update NavMesh instead of rebuilding it, the latter chews through processing budget
            mesh.UpdateNavMesh(mesh.navMeshData);
        }
    }

    IEnumerator<Octant<NavMeshModifierVolume>> UpdateVolumesAsync(bool usePerFrameValues, int scansPerFrame, int processesPerFrame)
    {
        int progress = 0;

        progress = 0;
        IEnumerator<Octant<NavMeshModifierVolume>> scanEnumerator = octree.ScanIterator();
        while (scanEnumerator.MoveNext())
        {
            if (usePerFrameValues)
            {
                progress++;
                if (progress % scansPerFrame == 0) yield return scanEnumerator.Current;
            }
            else
            {
                yield return scanEnumerator.Current;
            }
        }
        scanEnumerator.Dispose();
        progress = 0;

        IEnumerator<Octant<NavMeshModifierVolume>> refreshNavMeshVolumes = octree.baseContainer.CellIterator(true);
        while (refreshNavMeshVolumes.MoveNext())
        {
            if (usePerFrameValues)
            {
                progress++;
                if (progress % scansPerFrame == 0) yield return refreshNavMeshVolumes.Current;
            }
            else
            {
                yield return refreshNavMeshVolumes.Current;
            }
        }
        refreshNavMeshVolumes.Dispose();
        progress = 0;

        // All cells have been processed! Update the navmeshes
        //Debug.Log($"{cellsRefreshed} cells refreshed, time taken = {Time.time - lastTimeRefreshed}/{refreshDelay}");
        for (int n = 0; n < navMeshes.Length; n++)
        {
            NavMeshSurface mesh = navMeshes[n];
            if (mesh == null) continue;
            if (mesh.enabled == false) continue;
            if (mesh.navMeshData == null) continue;
            // Update NavMesh instead of rebuilding it, the latter chews through processing budget
            mesh.UpdateNavMesh(mesh.navMeshData);
        }
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
        /*
        // Don't have a volume if space is empty, or needs to be broken down further
        if (octant.type == OctantType.Invalid || octant.type == OctantType.Branch)
        {
            Debug.Log("Octant should not have a volume, ensuring deletion");
            return RemoveVolume(octant);
        }
        */

        NavMeshModifierVolume volume = octant.leafData;

        // Ensure a volume is present (instantiate if necessary, but do not duplicate)
        if (volume == null)
        {
            volume = ObjectPool.RequestObject(volumePrefab);
            volume.transform.parent = transform;
            volume.name = $"New volume";

            /*
            IndividualObjectPool pool = ObjectPool.activePools[volumePrefab];
            Debug.Log($"Volume summoned, count = {pool.activePrefabs.Count}, {pool.onStandby.Count}");
            */
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
        
        if (volume == null)
        {
            //Debug.Log("No octant to delete");
            return null;
        }

        volume.name = $"Deleted volume: {octant.key}, {octant.type}";
        ObjectPool.DismissObject(volume);

        /*
        IndividualObjectPool pool = ObjectPool.activePools[volumePrefab];
        Debug.Log($"Volume dismissed, count = {pool.activePrefabs.Count}, {pool.onStandby.Count}");
        */
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