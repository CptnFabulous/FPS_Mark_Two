using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelTest : MonoBehaviour
{
    public VoxelMesh mesh;
    public AnimationCurve noiseCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public Vector3 noiseOffsetDirection = Vector3.forward;
    public float noiseScale = 5;
    public float time;

    [Header("2x2 grid")]
    [Range(0, 1)] public float value1;
    [Range(0, 1)] public float value2;
    [Range(0, 1)] public float value3;
    [Range(0, 1)] public float value4;
    [Range(0, 1)] public float value5;
    [Range(0, 1)] public float value6;
    [Range(0, 1)] public float value7;
    [Range(0, 1)] public float value8;
    

    float[,,] values = null;
    Vector3Int dimensions = new Vector3Int(2, 2, 2);
    //Vector3Int dimensions = new Vector3Int(5, 5, 5);

    HashSet<Vector3> edgePoints = new HashSet<Vector3>();


    private void Awake()
    {
        values = new float[dimensions.x, dimensions.y, dimensions.z];
    }

    private void Update()
    {
        //Debug.Log($"Calculating voxel test on frame {Time.frameCount}");
        
        
        values[0, 0, 0] = value1;
        values[0, 1, 0] = value2;
        values[0, 0, 1] = value3;
        values[0, 1, 1] = value4;
        values[1, 0, 0] = value5;
        values[1, 1, 0] = value6;
        values[1, 0, 1] = value7;
        values[1, 1, 1] = value8;
        
        
        /*
        MiscFunctions.IterateThroughGrid(dimensions, (coords) =>
        {
            Vector3 noiseCoords = coords;
            noiseCoords += time * noiseOffsetDirection;
            noiseCoords *= noiseScale;


            float noiseX = Mathf.PerlinNoise1D(noiseCoords.x);
            float noiseY = Mathf.PerlinNoise1D(noiseCoords.y);
            float noiseZ = Mathf.PerlinNoise1D(noiseCoords.z);
            
            //float noise = (noiseX + noiseY + noiseZ) / 3;

            float noise = Mathf.PerlinNoise(noiseCoords.x, noiseCoords.z);







            noise = noiseCurve.Evaluate(noise);
            //Debug.Log($"Assigning values: {coords}, {noise}");

            values[coords.x, coords.y, coords.z] = noise;
        });
        */
        

        // Calculate edge points for signed distance fields
        SmokeChunk.FindFillEdgesForSignedDistanceField(dimensions, (pos) => GetValue(pos, false), edgePoints);
        // Calculate mesh
        mesh.Calculate(dimensions, GetValueAtCoordinates, GetSignedDistanceField, 1);
    }
    float GetSignedDistanceField(Vector3 position)
    {
        return SmokeChunk.SignedDistanceFieldInChunk(position, (pos) => GetValue(pos, true), edgePoints);
    }
    float GetValue(Vector3 position, bool debug)
    {
        return VoxelMesh.MidpointBetweenGridSpaces(position, GetValueAtCoordinates, debug);
    }
    float GetValueAtCoordinates(Vector3Int coords)
    {
        bool outsideArray = MiscFunctions.IsIndexOutsideArray(dimensions, coords);

        //Debug.Log($"Checking {coords}, outside array = {outsideArray}");

        if (outsideArray) return 0;
        return values[coords.x, coords.y, coords.z];
    }
}
