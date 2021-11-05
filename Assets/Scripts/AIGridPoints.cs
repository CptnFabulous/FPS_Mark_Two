using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIGridPoints : MonoBehaviour
{
    public static AIGridPoints Handler
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AIGridPoints>();
            }
            return instance;
        }
    }
    static AIGridPoints instance;
    
    public Bounds levelBounds;
    public float gridSpacing = 2;
    public LayerMask terrainDetection = ~0;
    List<Vector3> gridPoints;
    public Vector2Int GridSize
    {
        get
        {
            int x = Mathf.FloorToInt(levelBounds.size.x / gridSpacing);
            int z = Mathf.FloorToInt(levelBounds.size.z / gridSpacing);
            return new Vector2Int(x, z);
        }
    }

    private void Awake()
    {
        GenerateGrid();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(levelBounds.center, levelBounds.size);
    }

    public void GenerateGrid()
    {
        List<Vector3> newPoints = new List<Vector3>();

        for (int x = 0; x < GridSize.x; x++)
        {
            for (int z = 0; z < GridSize.y; z++)
            {
                float positionX = x * gridSpacing;
                float positionZ = z * gridSpacing;
                Vector3 origin = new Vector3(levelBounds.min.x + positionX, levelBounds.max.y, levelBounds.min.z + positionZ);
                RaycastHit[] terrainHit = Physics.RaycastAll(origin, Vector3.down,levelBounds.size.y, terrainDetection);
                for (int i = 0; i < terrainHit.Length; i++)
                {
                    // Uses hit point to account for height and generates a new Vector3 with said height but at the correct grid position.
                    Vector3 point = origin;
                    point.y = terrainHit[i].point.y;
                    newPoints.Add(point);
                    //Debug.DrawRay(point, Vector3.up, Color.cyan, 30);
                }
            }
        }

        gridPoints = newPoints;
    }

    /// <summary>
    /// Gets all grid points within a minimum and maximum radius of a certain position.
    /// </summary>
    /// <param name="centre"></param>
    /// <param name="minRadius"></param>
    /// <param name="maxRadius"></param>
    /// <returns></returns>
    public Vector3[] GetPoints(Vector3 centre, float minRadius, float maxRadius)
    {
        List<Vector3> points = new List<Vector3>(gridPoints);
        points.RemoveAll(p => Vector3.Distance(p, centre) < minRadius || Vector3.Distance(p, centre) > maxRadius);
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
    public Vector3[] GetSpecificNumberOfPoints(int number, Vector3 centre, float minRadius, float maxRadius)
    {
        Vector3[] points = GetPoints(centre, minRadius, maxRadius);
        if (points.Length <= number) // Return all results if there are less than desired by the number
        {
            return points;
        }

        List<Vector3> desired = new List<Vector3>();
        for (int i = 0; i < number; i++) // For the specified number of results
        {
            // Create an index by dividing the length by result number and then multiplying by the check number.
            // For example, 45 entries and 15 desired checks means the number increments by 3. When looking for the sixth entry, this would mean checking the eighteenth entry in the array.
            int index = Mathf.RoundToInt(points.Length / number * i);
            desired.Add(points[index]);
            Debug.DrawRay(points[index], Vector3.up * 3, Color.red, 30);
        }


        return desired.ToArray();
    }
}
