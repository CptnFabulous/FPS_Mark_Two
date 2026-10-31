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
        octree.matrix = transform.localToWorldMatrix;
        octree.Refresh();
    }
    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        octree.DrawGizmos();
    }

    bool CheckIfOccupied(Bounds bounds) => Physics.CheckBox(bounds.center, bounds.extents, Quaternion.identity, layers);
}
