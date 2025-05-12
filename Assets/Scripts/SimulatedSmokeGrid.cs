using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulatedSmokeGrid : MonoBehaviour
{
    [SerializeField] float timestep = 0.01f;
    [SerializeField] float maxDensityPerWorldUnit = 1;
    [SerializeField] float spreadSpeed = 1;
    [SerializeField] float decaySpeed = 0.2f;
    [SerializeField] SmokeChunk chunkPrefab;

    float lastTimeSimulated = 0;
    SmokeChunk[,,] _chunks = null;

    public SmokeChunk[,,] chunks => _chunks;
    public TerrainGrid terrainData => TerrainGrid.current;
    public int gridScale => terrainData.resolutionScale;
    public float decaySpeedPerSpace => decaySpeed / gridScale;
    public float maxDensityPerSpace => maxDensityPerWorldUnit / gridScale;
    public float deltaTime => timestep;
    
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
        Vector3Int chunkGridSize = terrainData.chunkGridSize;
        Debug.Log($"Generating smoke grid, dimensions = {chunkGridSize}");
        _chunks = new SmokeChunk[chunkGridSize.x, chunkGridSize.y, chunkGridSize.z];
        MiscFunctions.IterateThrough3DGrid(chunks, (x, y, z) =>
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
        for (int i = 0; i < missedSteps; i++)
        {
            // TO DO: prep for simulation step again
            // TO DO: move smoke based on velocity
            
            // Iterate through all chunks with smoke in them, and run their simulation step
            MiscFunctions.IterateThrough3DGrid(chunks, (x, y, z) =>
            {
                chunks[x, y, z].PrepareForSimulationStep();
            });
            MiscFunctions.IterateThrough3DGrid(chunks, (x, y, z) =>
            {
                chunks[x, y, z].SimulationStep();
            });

            lastTimeSimulated += timestep;
        }

        // TO DO: update mesh
    }
    
    public void IntroduceSmoke(Vector3 worldPosition, float amount)
    {
        if (amount <= 0) return;

        // Find appropriate chunk for that world position (if it exists)
        Vector3Int gridPos = terrainData.WorldToGridPosition(worldPosition);
        terrainData.GridToChunkCoords(gridPos, out Vector3Int chunkCoords, out Vector3Int coordsInChunk);
        if (MiscFunctions.IsIndexOutsideArray(chunks, chunkCoords)) return;

        // Ensure position isn't occupied by terrain
        if (terrainData.containsTerrain[gridPos.x, gridPos.y, gridPos.z]) return;

        // If not, add smoke
        SmokeChunk chunk = chunks[chunkCoords.x, chunkCoords.y, chunkCoords.z];
        chunk.AddSmoke(coordsInChunk, amount);
        //Debug.Log($"Inserting {amount} smoke into {gridPos}, new density = {chunk.GetDensity(coordsInChunk)}");
    }
    public void Clear() => MiscFunctions.IterateThrough3DGrid(chunks, (x, y, z) => chunks[x, y, z].Clear());
}