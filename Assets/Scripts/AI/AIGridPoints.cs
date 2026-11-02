using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIGridPoints : MonoBehaviour
{
    public readonly struct GridPoint
    {
        public readonly NavMeshHit navMeshData;
        readonly List<Vector3> _cover;
        readonly List<RaycastHit> _coverHits;

        public IReadOnlyList<Vector3> coverDirections => _cover;
        public IReadOnlyList<RaycastHit> coverHits => _coverHits;
        public bool valid => navMeshData.hit;
        public Vector3 position => navMeshData.position;
        public bool isCover => coverDirections != null && coverDirections.Count > 0;

        public GridPoint(AIGridPoints origin, Vector3 position, float gridSpacing, ref HashSet<Vector3> alreadyTaken)
        {
            //Debug.Log("Creating new grid point");
            navMeshData = new NavMeshHit();
            _cover = null;
            _coverHits = null;

            // Check that this point is on the NavMesh
            bool success = NavMesh.SamplePosition(position, out NavMeshHit hit, gridSpacing, ~0);
            if (!success) return;

            if (alreadyTaken.Contains(hit.position)) return;

            /*
            // Launch a raycast (that can hit backfaces) to check for obstructions
            Vector3 rayOrigin = hit.position + (0.1f * origin.floorNormal);
            Vector3 direction = position - rayOrigin;
            bool obstructionDetected = Physics.Raycast(rayOrigin, direction, out _, direction.magnitude, origin.environmentMask);
            if (obstructionDetected) return;
            */

            // TO DO: I think this can be changed to not store so much unnecessary data.
            // Since the possible cover check directions are always the same, I could use a flag system to keep track of them.
            // This would only work with up to 32 distinct cover directions, but I think that would be total overkill anyway.

            // I feel like I should also make cover data into a dictionary, and not need the grid point struct.
            // Or maybe not? Because there's also stuff like NavMeshHit data in it.
            // I'll keep that as is. But updating the cover system to use flags would be pretty cool.

            navMeshData = hit;
            origin.CoverCheck(hit.position, out _cover, out _coverHits);

            alreadyTaken.Add(hit.position);
        }
    }

    #region Singleton for current level
    public static AIGridPoints Current => instance ??= FindObjectOfType<AIGridPoints>();
    static AIGridPoints instance;
    /// <summary>
    /// An array that can be used to quickly sample points from, without creating more garbage. This is accessible from anywhere, so if another sample operation uses this the previous data will be wiped.
    /// </summary>
    public static GridPoint[] sampleArray = new GridPoint[8192];

    #endregion

    #region Generation variables

    [Header("Casting criteria")]
    public LayerMask environmentMask = ~0;
    
    public Vector3 floorEulerAngles;
    
    public float gridSpacing = 1;
    public float minAgentHeight = 2;

    [Header("Cover")]
    public float halfCoverHeight = 0.9f;
    public int numberOfDirectionChecksForCover = 8;

    HashSet<Vector3> positionsWithPoints = new HashSet<Vector3>();
    Octree<GridPoint> octree = new Octree<GridPoint>();
    List<GridPoint> _points = null;

    public Quaternion floorRotation => Quaternion.Euler(floorEulerAngles);
    public Vector3 floorNormal => floorRotation * Vector3.up;
    float coverCheckRaycastDistance => new Vector2(gridSpacing, gridSpacing).magnitude;
    public float coverCheckAngleSize => 360f / numberOfDirectionChecksForCover;
    #endregion

    public IReadOnlyList<GridPoint> gridPoints
    {
        get
        {
            if (_points == null) Generate();
            return _points;
        }
    }

    public void OnDrawGizmosSelected()
    {
        if (MiscFunctions.CurrentCameraNotMain()) return;

        if (octree == null) return;

        Vector3 coverOffset = halfCoverHeight * floorNormal;

        void DrawGridPoint(GridPoint gridPoint)
        {
            Gizmos.color = gridPoint.isCover ? Color.green : Color.red;
            Gizmos.DrawRay(gridPoint.position, floorNormal);

            if (!gridPoint.isCover) return;

            Gizmos.color = Color.cyan;
            foreach (Vector3 direction in gridPoint.coverDirections)
            {
                Gizmos.DrawRay(gridPoint.position + coverOffset, direction * coverCheckRaycastDistance);
            }
        }

        IEnumerator<Octant<GridPoint>> enumerator = octree.baseContainer.CellIterator(false);
        while (enumerator.MoveNext())
        {
            // Ignore if grid point is invalid
            GridPoint gridPoint = enumerator.Current.leafData;
            if (gridPoint.valid == false) continue;

            // Draw cube representing octant
            Gizmos.color = Color.grey;
            Bounds bounds = enumerator.Current.transformedBounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);

            // Draw line connecting the cube to the actual start point
            Gizmos.DrawLine(bounds.center, gridPoint.position);

            // Draw the actual grid point and its data
            DrawGridPoint(gridPoint);
        }
        enumerator.Dispose();
    }

    #region Generation

    [ContextMenu("Force regeneration")]
    public void Generate()
    {
        GenerateOctree(out octree);
    }

    public void GenerateOctree(out Octree<GridPoint> octree)
    {
        // Ensure map is generated
        TerrainGrid.current.GenerateMap();

        positionsWithPoints.Clear();

        // Create new octree
        octree = new Octree<GridPoint>();
        TerrainGrid.GenerateOctreeDataForLevelBounds(gridSpacing, out _, out int subdivisions, out Matrix4x4 octreeToWorldMatrix);
        octree.subdivisions = subdivisions;
        octree.matrix = octreeToWorldMatrix;
        // Check for valid level geometry
        octree.checkOctant = (bounds) => Physics.CheckBox(bounds.center, bounds.extents, floorRotation, environmentMask);
        // Sample a NavMesh position within said octant (make sure to do every leaf octant, even if an entire parent octant is full)
        octree.checkForEntirelyFullOctants = false;
        octree.refreshOctant = (octant) =>
        {
            Bounds bounds = octant.transformedBounds;
            float distance = 0.5f * new Vector3(gridSpacing, gridSpacing, gridSpacing).magnitude;
            return new GridPoint(this, bounds.center, distance, ref positionsWithPoints);
        };
        octree.clearOctant = (_) => new GridPoint();

        // Trigger full refresh
        octree.Refresh();

        // Iterate through created points, adding all valid ones to the list
        if (_points == null) _points = new List<GridPoint>();
        _points.Clear();
        IEnumerator<Octant<GridPoint>> enumerator = octree.baseContainer.CellIterator(false);
        while (enumerator.MoveNext())
        {
            GridPoint gridPoint = enumerator.Current.leafData;
            if (gridPoint.valid) _points.Add(gridPoint);
        }
        enumerator.Dispose();

        Debug.Log($"Generated grid points using octree, count = {_points.Count}");
    }

    public void CoverCheck(Vector3 position, out List<Vector3> directions, out List<RaycastHit> hits)
    {
        directions = new List<Vector3>();
        hits = new List<RaycastHit>();

        
        Vector3 rayOrigin = position + (halfCoverHeight * floorNormal);

        for (int i = 0; i < numberOfDirectionChecksForCover; i++)
        {
            Vector3 rayDirection = Quaternion.Euler(0, coverCheckAngleSize * i, 0) * Vector3.forward;
            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit rh, coverCheckRaycastDistance, environmentMask))
            {
                directions.Add(rayDirection);
                hits.Add(rh);
            }
        }
    }
    #endregion

    public void GetPoints(System.Func<GridPoint, bool> criteria, ref List<GridPoint> collection)
    {
        for (int i = 0; i < gridPoints.Count; i++)
        {
            // Ignore if criteria not met
            bool valid = criteria.Invoke(gridPoints[i]);
            if (!valid) continue;
            // Add point to list
            collection.Add(gridPoints[i]);
        }
    }
    public int GetPointsNonAlloc(ref GridPoint[] collection, System.Func<GridPoint, bool> criteria, int maxNumberOfChecks = -1, System.Func<Bounds, bool> childrenWorthChecking = null)
    {
        int validPoints = 0;

        
        // If we don't care about the octants, just iterate through everything linearly.
        if (childrenWorthChecking == null)
        {
            // Iterate through list linearly
            for (int i = 0; i < gridPoints.Count; i++)
            {
                // Ignore if criteria not met
                bool valid = criteria.Invoke(gridPoints[i]);
                if (!valid) continue;

                // Add point
                collection[validPoints] = gridPoints[i];
                validPoints++;

                // Cancel once we can't take any more
                if (validPoints >= collection.Length) break;
                if (maxNumberOfChecks >= 0 && validPoints >= maxNumberOfChecks) break;
            }

            return validPoints;
        }


        // Iterate through octree. Ignore if empty or criteria is not met, always allow leaf octants
        IEnumerator<Octant<GridPoint>> iterator = octree.GetEnumerator((o) =>
        {
            // Ignore empty octants
            if (o.type == OctantType.Invalid) return false;
            // If no criteria is needed, allow all
            if (childrenWorthChecking == null) return true;
            // Otherwise check if criteria is met
            return childrenWorthChecking.Invoke(o.transformedBounds);
        });
        while (iterator.MoveNext())
        {
            Octant<GridPoint> octant = iterator.Current;

            // Check that a valid grid point exists
            GridPoint gridPoint = octant.leafData;
            if (!gridPoint.valid) continue;

            //Debug.Log("Valid grid point");

            // Ensure grid point meets criteria
            bool valid = criteria.Invoke(gridPoint);
            if (!valid) continue;

            //Debug.Log("Meets criteria");

            // Add point
            collection[validPoints] = gridPoint;
            validPoints++;

            // Cancel once we can't take any more
            if (validPoints >= collection.Length) break;
            if (maxNumberOfChecks >= 0 && validPoints >= maxNumberOfChecks) break;
        }
        iterator.Dispose();

        return validPoints;
    }

    /// <summary>
    /// Gets all grid points within a minimum and maximum radius of a certain position.
    /// </summary>
    /// <param name="centre"></param>
    /// <param name="minRadius"></param>
    /// <param name="maxRadius"></param>
    /// <returns></returns>
    public int GetPointsInRadius(ref GridPoint[] collection, Vector3 centre, float minRadius, float maxRadius, bool onlyIncludeCover = false)
    {
        return GetPointsNonAlloc(ref collection, (p) => IsWithinRadius(p, centre, minRadius, maxRadius, onlyIncludeCover));
    }
    public int GetGridPointsInArea(ref GridPoint[] collection, LevelArea rootArea)
    {
        if (rootArea == null) return -1;
        return GetPointsNonAlloc(ref collection, (p) => rootArea.Contains(p.position));
    }








    public static bool IsWithinRadius(GridPoint p, Vector3 centre, float minRadius, float maxRadius, bool onlyIncludeCover = false)
    {
        // If checking for cover, exclude points that aren't cover
        if (onlyIncludeCover && p.isCover == false)
        {
            //Debug.Log($"Distance check disqualified, point is not cover. {p.coverDirections}");
            return false;
        }

        return IsWithinRadius(p.position, centre, minRadius, maxRadius);
    }
    public static bool IsWithinRadius(Vector3 point, Vector3 centre, float minRadius, float maxRadius)
    {
        // Check that the distance is correct
        float distance = Vector3.Distance(point, centre);
        //Debug.Log($"Distance check: {distance}, {minRadius}, {maxRadius}");
        if (distance < minRadius) return false;
        if (distance > maxRadius) return false;

        // Point is valid
        return true;
    }











    
    
    

    /*
    public static bool MeshCheckBox(Mesh mesh, Vector3 centre, Vector3 halfExtents)
    {
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            // Check if any of the triangles' vertices is inside the zone
            // Check if a line drawn between any of the two vertices intersects the zone
        }
    }
    */
}
