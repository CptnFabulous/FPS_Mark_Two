using CptnFabulous.MiscUtility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class AIGridPoints : MonoBehaviour
{
    public struct GridPoint
    {
        public Vector3 position => navMeshData.position;
        public Vector3[] coverDirections { get; private set; }
        public bool isCover => coverDirections.Length > 0;
        public NavMeshHit navMeshData { get; private set; }

        public GridPoint(NavMeshHit navMeshData, Vector3[] coverDirections)
        {
            this.navMeshData = navMeshData;
            this.coverDirections = coverDirections;
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
    public Bounds levelBounds = new Bounds(Vector3.zero, Vector3.one * 50);
    
    public Vector3 floorEulerAngles;
    
    public float gridSpacing = 1;
    public float minAgentHeight = 2;

    [Header("Cover")]
    public float halfCoverHeight = 0.9f;
    public int numberOfDirectionChecksForCover = 8;

    List<GridPoint> _points = null;

    float raycastHeightPadding = 5;

    public Bounds bounds => levelBounds;
    public Vector2Int GridSize
    {
        get
        {
            int x = Mathf.FloorToInt(bounds.size.x / gridSpacing);
            int z = Mathf.FloorToInt(bounds.size.z / gridSpacing);
            return new Vector2Int(x, z);
        }
    }
    public Quaternion floorRotation => Quaternion.Euler(floorEulerAngles);
    public Vector3 floorNormal => floorRotation * Vector3.up;


    float coverCheckRaycastDistance => MathUtility.LengthOfDiagonal(gridSpacing, gridSpacing);
    public float coverCheckAngleSize => 360f / numberOfDirectionChecksForCover;
    #endregion

    public IReadOnlyList<GridPoint> gridPoints
    {
        get
        {
            if (_points == null)
            {
                Generate();
                /*
                // Set up listeners so whenever the scene (and terrain) change, the grid points are updated
                SceneManager.sceneLoaded += (_, _) => Generate();
                SceneManager.sceneUnloaded += (_) => Generate();
                */
            }
            return _points;
        }
    }

    public void OnDrawGizmosSelected()
    {
        if (MiscFunctions.CurrentCameraNotMain()) return;
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(bounds.center, bounds.size);

        if (gridPoints != null)
        {
            for (int i = 0; i < gridPoints.Count; i++)
            {
                Gizmos.color = gridPoints[i].isCover ? Color.green : Color.red;
                Gizmos.DrawRay(gridPoints[i].position, floorNormal);

                foreach (Vector3 direction in gridPoints[i].coverDirections)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawRay(gridPoints[i].position + floorNormal, direction * coverCheckRaycastDistance);
                }
                
            }
        }
    }

    #region Generation

    [ContextMenu("Force regeneration")]
    public void Generate()
    {
        _points = GenerateGrid(bounds);
    }
    List<GridPoint> GenerateGrid(Bounds levelBounds)
    {
        List<GridPoint> newPoints = new List<GridPoint>();

        for (int x = 0; x < GridSize.x; x++)
        {
            for (int z = 0; z < GridSize.y; z++)
            {
                float positionX = x * gridSpacing;
                float positionZ = z * gridSpacing;

                Vector3 origin = new Vector3(levelBounds.min.x + positionX, levelBounds.max.y, levelBounds.min.z + positionZ);
                float distance = levelBounds.size.y - minAgentHeight;

                // Calculate extra distance for raycasts
                origin.y += raycastHeightPadding;
                distance += raycastHeightPadding * 2;
                //distance = Mathf.Max(distance, raycastHeightPadding * 2);

                //Debug.DrawRay(origin, -floorNormal * distance, Color.black, 20);
                while (Physics.Raycast(origin, -floorNormal, out RaycastHit rh, distance, environmentMask))
                {
                    // Reset the positions for the next raycast
                    origin = rh.point + (minAgentHeight * -floorNormal);
                    distance -= rh.distance;

                    // Check that this point is on the NavMesh
                    if (NavMesh.SamplePosition(rh.point, out NavMeshHit hit, gridSpacing, ~0) == false) continue;

                    Vector3[] coverDirections = CoverCheck(hit.position).ToArray(); // Calculate cover points
                    newPoints.Add(new GridPoint(hit, coverDirections)); // Assemble values and add to the list
                }
                
            }
        }

        /*
        // Sort points on each position axis
        for (int i = 0; i < 3; i++)
        {
            newPoints.Sort((lhs, rhs) => lhs.position[i].CompareTo(rhs.position[i]));
        }
        */

        return newPoints;
    }
    public List<Vector3> CoverCheck(Vector3 position)
    {
        List<Vector3> directions = new List<Vector3>();

        
        Vector3 rayOrigin = position + (halfCoverHeight * floorNormal);
        //Vector3 rayOrigin = position + (minAgentHeight / 2 * floorNormal);

        for (int i = 0; i < numberOfDirectionChecksForCover; i++)
        {
            Vector3 rayDirection = Quaternion.Euler(0, coverCheckAngleSize * i, 0) * Vector3.forward;
            if (Physics.Raycast(rayOrigin, rayDirection, out _, coverCheckRaycastDistance, environmentMask))
            {
                directions.Add(rayDirection);
            }
        }

        return directions;
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
    public int GetPointsNonAlloc(System.Func<GridPoint, bool> criteria, ref GridPoint[] collection, int maxNumberOfChecks = -1)
    {
        int validPoints = 0;
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

    /// <summary>
    /// Gets all grid points within a minimum and maximum radius of a certain position.
    /// </summary>
    /// <param name="centre"></param>
    /// <param name="minRadius"></param>
    /// <param name="maxRadius"></param>
    /// <returns></returns>
    public int GetPointsInRadius(ref GridPoint[] collection, Vector3 centre, float minRadius, float maxRadius, bool onlyIncludeCover = false)
    {
        return GetPointsNonAlloc((p) => IsWithinRadius(p, centre, minRadius, maxRadius, onlyIncludeCover), ref collection);
    }
    public int GetGridPointsInArea(LevelArea rootArea, ref GridPoint[] collection)
    {
        if (rootArea == null) return -1;
        return GetPointsNonAlloc((p) => rootArea.Contains(p.position), ref collection);
    }








    public bool IsWithinRadius(GridPoint p, Vector3 centre, float minRadius, float maxRadius, bool onlyIncludeCover = false)
    {
        // If checking for cover, exclude points that aren't cover
        if (onlyIncludeCover && p.isCover == false)
        {
            return false;
        }

        // Check that the distance is correct
        float distance = Vector3.Distance(p.position, centre);
        if (distance < minRadius || distance > maxRadius)
        {
            return false;
        }

        // Point is valid
        return true;
    }
}
