using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelTest : MonoBehaviour
{
    public VoxelMesh mesh;
    [Range(0, 1)] public float value1;
    [Range(0, 1)] public float value2;
    [Range(0, 1)] public float value3;
    [Range(0, 1)] public float value4;
    [Range(0, 1)] public float value5;
    [Range(0, 1)] public float value6;
    [Range(0, 1)] public float value7;
    [Range(0, 1)] public float value8;

    float[,,] values = new float[2, 2, 2];

    private void Update()
    {
        Vector3Int dimensions = new Vector3Int(2, 2, 2);
        values[0, 0, 0] = value1;
        values[0, 1, 0] = value2;
        values[0, 0, 1] = value3;
        values[0, 1, 1] = value4;
        values[1, 0, 0] = value5;
        values[1, 1, 0] = value6;
        values[1, 0, 1] = value7;
        values[1, 1, 1] = value8;

        mesh.Calculate(values, dimensions, 1);
    }
}
