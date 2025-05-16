using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulatedSmokeGrid : MonoBehaviour
{
    [SerializeField] float _timestep = 0.01f;
    [SerializeField] float maxDensityPerWorldUnit = 1;
    [SerializeField] float spreadSpeed = 1;
    [SerializeField] float decaySpeed = 0.2f;
    [SerializeField] SmokeChunk chunkPrefab;
    public bool showDebugData;

    float lastTimeSimulated = 0;
    SmokeChunk[,,] _chunks = null;

    public SmokeChunk[,,] chunks => _chunks;
    public Vector3Int chunkGridSize => terrainData.chunkGridSize;
    public TerrainGrid terrainData => TerrainGrid.current;
    public int gridScale => terrainData.resolutionScale;
    public float decaySpeedPerSpace => decaySpeed / gridScale;
    public float maxDensityPerSpace => maxDensityPerWorldUnit / gridScale;
    public float timestep => _timestep;
    
    private void OnEnable()
    {
        lastTimeSimulated = Time.time;
    }
    private void Start()
    {
        GenerateChunks();
    }

    void GenerateChunks()
    {
        Debug.Log($"Generating smoke grid, dimensions = {chunkGridSize}");
        _chunks = new SmokeChunk[chunkGridSize.x, chunkGridSize.y, chunkGridSize.z];
        MiscFunctions.IterateThroughGrid(chunkGridSize, (x, y, z) =>
        {
            Vector3Int gridPos = terrainData.ChunkToGridCoords(new Vector3Int(x, y, z), Vector3Int.zero);
            Vector3 worldPos = terrainData.GridToWorldPosition(gridPos);
            chunks[x, y, z] = Instantiate(chunkPrefab, worldPos, Quaternion.identity, transform);
            chunks[x, y, z].gameObject.SetActive(true);
        });
    }

    void Update()
    {
        float timeSinceLastSimulation = Time.time - lastTimeSimulated;
        int missedSteps = Mathf.FloorToInt(timeSinceLastSimulation / timestep);
        if (missedSteps <= 0) return;

        for (int i = 0; i < missedSteps; i++)
        {
            SimulationStep();
            lastTimeSimulated += timestep;
        }

        // TO DO: update mesh
        MiscFunctions.IterateThroughGrid(chunkGridSize, (x, y, z) => chunks[x, y, z].UpdateMesh());
    }
    void SimulationStep()
    {
        // TO DO: I think that data is being moved into adjacent squares before data is shifted from newGrid to oldGrid.
        // I think I need to do the data shifting in each chunk, before I start changing things.

        // TO DO: prep for simulation step
        // TO DO: move smoke based on velocity

        // Prep for simulation step again (this needs to be done each time an operation is performed that affects multiple spaces/chunks at once)
        MiscFunctions.IterateThroughGrid(chunkGridSize, (x, y, z) => chunks[x, y, z].PrepareForStep());
        // Dissipate smoke
        MiscFunctions.IterateThroughGrid(chunkGridSize, (x, y, z) => chunks[x, y, z].SpreadStep());
    }

    public void IntroduceSmoke(Vector3 worldPosition, float amount)
    {
        if (amount <= 0) return;

        // Find appropriate chunk for that world position (if it exists)
        Vector3Int gridPos = terrainData.WorldToGridPosition(worldPosition);
        terrainData.GridToChunkCoords(gridPos, out Vector3Int chunkCoords, out Vector3Int coordsInChunk);
        if (MiscFunctions.IsIndexOutsideArray(chunkGridSize, chunkCoords)) return;

        // Ensure position isn't occupied by terrain
        if (terrainData.containsTerrain[gridPos.x, gridPos.y, gridPos.z]) return;

        // If not, add smoke
        SmokeChunk chunk = chunks[chunkCoords.x, chunkCoords.y, chunkCoords.z];
        chunk.AddSmoke(coordsInChunk, amount);
        //Debug.Log($"Inserting {amount} smoke into {gridPos}, new density = {chunk.GetDensity(coordsInChunk)}");
    }
    
    public void Clear() => MiscFunctions.IterateThroughGrid(chunkGridSize, (x, y, z) => chunks[x, y, z].Clear());
}