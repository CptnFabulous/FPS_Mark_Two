using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
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


    float coverCheckRaycastDistance => MiscFunctions.LengthOfDiagonal(gridSpacing, gridSpacing);
    public float coverCheckAngleSize => 360f / numberOfDirectionChecksForCover;
    #endregion

    public List<GridPoint> gridPoints
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

    /// <summary>
    /// Gets all grid points within a minimum and maximum radius of a certain position.
    /// </summary>
    /// <param name="centre"></param>
    /// <param name="minRadius"></param>
    /// <param name="maxRadius"></param>
    /// <returns></returns>
    public List<GridPoint> GetPoints(Vector3 centre, float minRadius, float maxRadius, bool onlyIncludeCover = false)
    {
        return gridPoints.FindAll(p =>
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
        });
    }
    /// <summary>
    /// Get a specified number of points within a minimum and maximum radius, sampled for somewhat even distribution
    /// </summary>
    /// <param name="number"></param>
    /// <param name="centre"></param>
    /// <param name="minRadius"></param>
    /// <param name="maxRadius"></param>
    /// <returns></returns>
    public GridPoint[] GetSpecificNumberOfPoints(int number, Vector3 centre, float minRadius, float maxRadius, bool onlyIncludeCover = false)
    {
        List<GridPoint> points = GetPoints(centre, minRadius, maxRadius, onlyIncludeCover);
        if (points.Count <= number) // Return all results if there are less than desired by the number
        {
            return points.ToArray();
        }

        List<GridPoint> desired = new List<GridPoint>();
        for (int i = 0; i < number; i++) // For the specified number of results
        {
            // Create an index by dividing the length by result number and then multiplying by the check number.
            // For example, 45 entries and 15 desired checks means the number increments by 3. When looking for the sixth entry, this would mean checking the eighteenth entry in the array.
            int index = Mathf.RoundToInt(points.Count / number * i);
            desired.Add(points[index]);
        }

        return desired.ToArray();
    }
    public List<GridPoint> GetGridPointsInArea(LevelArea rootArea)
    {
        if (rootArea == null) return null;
        return gridPoints.FindAll((p) => rootArea.Contains(p.position));
    }
}
