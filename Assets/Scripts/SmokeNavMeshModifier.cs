using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using CptnFabulous.ObjectPool;

public class SmokeNavMeshModifier : MonoBehaviour
{
    [SerializeField] LayerMask smokeMask = ~0;
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
            // Update NavMesh (do not rebuild the entire thing as that chews through processing budget)
            mesh.UpdateNavMesh(mesh.navMeshData);
        }
    }

    bool DoesOctantContainSmoke(Vector3Int min, Vector3Int max)
    {
        GetWorldExtents(min, max, octreeToWorldMatrix, out Vector3 centre, out Vector3 halfExtents);
        return Physics.CheckBox(centre, halfExtents, Quaternion.identity, smokeMask);
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

// This is an old version.
// It uses the density controller's grid spaces to determine where smoke is
// But my new octree-based version is both more performant and way cooler to show off.
/*
public class SmokeNavMeshModifier : MonoBehaviour
{
    public NavMeshSurface[] navMeshes;
    public int particleDensityThreshold = 0;
    public NavMeshModifierVolume volumePrefab;
    public float refreshTimer = 0.2f;


    NavMeshModifierVolume[] activeVolumes = new NavMeshModifierVolume[1024];
    int activeVolumeCount;

    float lastTimeRefreshed;

    // Start is called before the first frame update
    void Start()
    {
        //volumePrefab = new GameObject("Smoke NavMesh Modifier Volume").AddComponent<NavMeshModifierVolume>();
        volumePrefab.center = Vector3.zero;

        float gridSize = SmokeParticleDensityController.GetSingleton().gridSpaceSize;
        volumePrefab.size = new Vector3(gridSize, gridSize, gridSize);

        // TO DO: create an object pool with this transform as the pool parent.
        ObjectPool.CreateObjectPool(volumePrefab);
        volumePrefab.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - lastTimeRefreshed > refreshTimer)
        {
            RefreshMesh();
            lastTimeRefreshed = Time.time;
        }
    }



    public void RefreshMesh()
    {
        SmokeParticleDensityController controller = SmokeParticleDensityController.GetSingleton();

        // For each grid space that currently has particles, add it to a list.
        int spacesWithSmoke = 0;
        for (int i = 0; i < controller.gridSpaces.Count; i++)
        {
            SmokeParticleDensityController.ParticleGridSpace gridSpace = controller.gridSpaces.Values.ElementAt(i);
            if (gridSpace.numberOfParticles <= particleDensityThreshold) continue;

            // For each grid space with a certain particle density, ensure a modifier volume is assigned.
            if (activeVolumes[spacesWithSmoke] == null)
            {
                activeVolumes[spacesWithSmoke] = ObjectPool.RequestObject(volumePrefab);
            }

            // Position the volume to the corresponding point in space.
            activeVolumes[spacesWithSmoke].transform.localPosition = gridSpace.worldPosition;

            // Keep track of how many volumes are needed
            spacesWithSmoke++;
        }

        // If there are less modifier volumes needed compared to last frame, dismiss any extras
        for (int i = spacesWithSmoke; i < activeVolumeCount; i++)
        {
            ObjectPool.DismissObject(activeVolumes[i]);
        }

        // Refresh how many volumes are present this frame
        activeVolumeCount = spacesWithSmoke;

        // Rebuild navmeshes
        for (int i = 0; i < navMeshes.Length; i++)
        {
            NavMeshSurface mesh = navMeshes[i];
            if (mesh == null) continue;
            if (mesh.enabled == false) continue;
            mesh.BuildNavMesh();
        }
    }
}
*/