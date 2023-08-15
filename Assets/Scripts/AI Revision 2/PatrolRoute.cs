using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class PatrolRoute : MonoBehaviour
{
    public Transform[] points;
    public bool endToEnd;


    NavMeshPath[] paths;

    [ContextMenu("Regenerate paths")]
    void GeneratePaths()
    {
        int length = points.Length;
        paths = new NavMeshPath[length];
        for (int i = 0; i < length; i++)
        {
            if (paths[i] == null) paths[i] = new NavMeshPath();

            Vector3 a = points[i].position;
            Vector3 b = points[MiscFunctions.LoopIndex(i + 1, length)].position;
            NavMesh.CalculatePath(a, b, NavMesh.AllAreas, paths[i]);
        }
    }

    private void OnValidate()
    {
        GeneratePaths();
    }
    private void OnDrawGizmosSelected()
    {
        if (points == null) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < points.Length; i++)
        {
            if (paths[i] != null && paths[i].status != NavMeshPathStatus.PathInvalid)
            {
                Color c = paths[i].status == NavMeshPathStatus.PathComplete ? Color.green : Color.yellow;
                DrawNavMeshDebugLine(paths[i].corners, c);
            }
            else
            {
                int b = MiscFunctions.LoopIndex(i + 1, points.Length);
                DrawDebugArrow(points[i].position, points[b].position, Color.red);
            }
        }
    }



    public static void DrawNavMeshDebugLine(Vector3[] corners, Color colour, float time = 0)
    {
        if (corners.Length <= 1) return;
        
        for (int i = 1; i < corners.Length; i++)
        {
            DrawDebugArrow(corners[i - 1], corners[i], colour, time);
        }
    }

    public static void DrawDebugArrow(Vector3 a, Vector3 b, Color colour, float time = 0)
    {
        Debug.DrawLine(a, b, colour, time);

        Vector3 direction = 0.5f * (a - b).normalized;

        Quaternion l = Quaternion.Euler(0, 135, 0);
        Quaternion r = Quaternion.Euler(0, -135, 0);
        Debug.DrawRay(b, l * -direction, colour, time);
        Debug.DrawRay(b, r * -direction, colour, time);
    }
}
