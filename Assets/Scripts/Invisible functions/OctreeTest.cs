using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CptnFabulous.MiscUtility;

[ExecuteAlways]
public class OctreeTest : MonoBehaviour
{
    public Octree<Vector3> octree = new Octree<Vector3>();
    public int subdivisions = 5;
    public LayerMask layers;

    private void Update()
    {
        octree.subdivisions = subdivisions;
        octree.checkOctant = CheckIfOccupied;
        octree.Refresh();
    }
    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        octree.DrawGizmos();
    }

    bool CheckIfOccupied(Vector3Int min, Vector3Int max)
    {
        Bounds bounds = new Bounds();
        bounds.min = min;
        bounds.max = max;

        // Convert position and size values from local to world space
        Matrix4x4 matrix = transform.localToWorldMatrix;
        Vector3 centre = matrix.MultiplyPoint3x4(bounds.center);
        Vector3 halfExtents = matrix.MultiplyVector(bounds.extents);

        return Physics.CheckBox(centre, halfExtents, Quaternion.identity, layers);
    }
}
