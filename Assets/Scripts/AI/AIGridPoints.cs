using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class AIGridPoints : MonoBehaviour
{
    #region Singleton for current level
    public static AIGridPoints Current => instance ??= FindObjectOfType<AIGridPoints>();
    static AIGridPoints instance;
    #endregion

    #region Generation variables
    public Bounds levelBounds = new Bounds(Vector3.zero, Vector3.one * 50);
    public LayerMask terrainDetection = ~0;
    public Vector3 floorEulerAngles;
    
    public float gridSpacing = 1;
    public float minAgentHeight = 2;

    public Vector2Int GridSize
    {
        get
        {
            int x = Mathf.FloorToInt(levelBounds.size.x / gridSpacing);
            int z = Mathf.FloorToInt(levelBounds.size.z / gridSpacing);
            return new Vector2Int(x, z);
        }
    }
    public Quaternion floorRotation => Quaternion.Euler(floorEulerAngles);
    public Vector3 floorNormal => floorRotation * Vector3.up;


    public struct GridPoint
    {
        public Vector3 position;
        public bool isCover;
    }

    
    List<GridPoint> gridPoints
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
        Gizmos.DrawWireCube(levelBounds.center, levelBounds.size);

        if (gridPoints != null)
        {
            for (int i = 0; i < gridPoints.Count; i++)
            {
                Gizmos.color = gridPoints[i].isCover ? Color.green : Color.red;
                Gizmos.DrawRay(gridPoints[i].position, floorNormal);
            }
        }
    }

    #region Generation

    [ContextMenu("Force regeneration")]
    public void Generate()
    {
        _points = GenerateGrid(levelBounds);
    }
    List<GridPoint> GenerateGrid(Bounds levelBounds)
    {
        //levelBounds.extents += new Vector3(raycastHeightPadding, raycastHeightPadding, raycastHeightPadding);

        float halfHeight = minAgentHeight / 2;
        float paddingUpFromFloor = 0.1f;
        Vector3 halfExtents = new Vector3(gridSpacing, halfHeight - paddingUpFromFloor, gridSpacing);
        List<GridPoint> newPoints = new List<GridPoint>();

        for (int x = 0; x < GridSize.x; x++)
        {
            for (int z = 0; z < GridSize.y; z++)
            {
                float positionX = x * gridSpacing;
                float positionZ = z * gridSpacing;

                Vector3 origin = new Vector3(levelBounds.min.x + positionX, levelBounds.max.y, levelBounds.min.z + positionZ);
                float distance = levelBounds.size.y - minAgentHeight;
                while (Physics.Raycast(origin, -floorNormal, out RaycastHit rh, distance, terrainDetection))
                {
                    // Reset the positions for the next raycast
                    origin = rh.point + (minAgentHeight * -floorNormal);
                    distance -= rh.distance;

                    GridPoint newPoint = new GridPoint();
                    newPoint.position = rh.point;

                    // Calculate cover
                    Collider[] thingsSurrounding = Physics.OverlapBox(newPoint.position + (halfHeight * floorNormal), halfExtents, floorRotation, terrainDetection);
                    if (thingsSurrounding.Length > 0)
                    {
                        newPoint.isCover = true;
                    }

                    newPoints.Add(newPoint);
                }
                
            }
        }

        //newPoints.Sort((lhs, rhs) => lhs.position.x.CompareTo(rhs.position.x));
        //newPoints.Sort((lhs, rhs) => lhs.position.y.CompareTo(rhs.position.y));
        //newPoints.Sort((lhs, rhs) => lhs.position.z.CompareTo(rhs.position.z));

        return newPoints;
    }
    }

    /// <summary>
    /// Gets all grid points within a minimum and maximum radius of a certain position.
    /// </summary>
    /// <param name="centre"></param>
    /// <param name="minRadius"></param>
    /// <param name="maxRadius"></param>
    /// <returns></returns>
    public GridPoint[] GetPoints(Vector3 centre, float minRadius, float maxRadius, bool onlyIncludeCover = false)
    {
        List<GridPoint> points = new List<GridPoint>(gridPoints);
        points.RemoveAll(p =>
        {
            float distance = Vector3.Distance(p.position, centre);
            return distance < minRadius || distance > maxRadius;
        });
        
        if (onlyIncludeCover) points.RemoveAll(p => p.isCover == false);

        return points.ToArray();
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
        GridPoint[] points = GetPoints(centre, minRadius, maxRadius, onlyIncludeCover);
        if (points.Length <= number) // Return all results if there are less than desired by the number
        {
            return points;
        }

        List<GridPoint> desired = new List<GridPoint>();
        for (int i = 0; i < number; i++) // For the specified number of results
        {
            // Create an index by dividing the length by result number and then multiplying by the check number.
            // For example, 45 entries and 15 desired checks means the number increments by 3. When looking for the sixth entry, this would mean checking the eighteenth entry in the array.
            int index = Mathf.RoundToInt(points.Length / number * i);
            desired.Add(points[index]);
        }

        return desired.ToArray();
    }
}
