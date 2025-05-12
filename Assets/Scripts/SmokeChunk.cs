using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class SmokeChunk : MonoBehaviour
{
    public static Vector3Int[] availableNeighboursNonAlloc = new Vector3Int[26];
    public static readonly Vector3Int[] neighbourOffsets = new Vector3Int[26]
    {
        new Vector3Int(-1, -1, -1),
        new Vector3Int(0, -1, -1),
        new Vector3Int(1, -1, -1),

        new Vector3Int(-1, 0, -1),
        new Vector3Int(0, 0, -1),
        new Vector3Int(1, 0, -1),

        new Vector3Int(-1, 1, -1),
        new Vector3Int(0, 1, -1),
        new Vector3Int(1, 1, -1),



        new Vector3Int(-1, -1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(1, -1, 0),

        new Vector3Int(-1, 0, 0),
        //new Vector3Int(0, 0, 0),
        new Vector3Int(1, 0, 0),

        new Vector3Int(-1, 1, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(1, 1, 0),



        new Vector3Int(-1, -1, 1),
        new Vector3Int(0, -1, 1),
        new Vector3Int(1, -1, 1),

        new Vector3Int(-1, 0, 1),
        new Vector3Int(0, 0, 1),
        new Vector3Int(1, 0, 1),

        new Vector3Int(-1, 1, 1),
        new Vector3Int(0, 1, 1),
        new Vector3Int(1, 1, 1),
    };

    SimulatedSmokeGrid parentGrid;
    float[,,] grid;
    float[,,] _oldGrid;
    Vector3Int _chunkPositionInGrid;

    public Vector3Int size => TerrainGrid.current.chunkSize;
    public Vector3Int chunkPositionInGrid => _chunkPositionInGrid;
    public bool smokeIsPresent { get; private set; } = false;

    public TerrainGrid terrainData => TerrainGrid.current;
    float maxDensityPerSpace => parentGrid.maxDensityPerSpace;
    float decaySpeed => parentGrid.decaySpeedPerSpace;
    float deltaTime => parentGrid.deltaTime;
    int gridScale => parentGrid.gridScale;

    private void Awake()
    {
        parentGrid = GetComponentInParent<SimulatedSmokeGrid>();
        grid = new float[size.x, size.y, size.z];
        _oldGrid = new float[size.x, size.y, size.z];
        _chunkPositionInGrid = MiscFunctions.IndexOfIn3DArray(parentGrid.chunks, this);
    }
    void OnDrawGizmos()
    {
        if (!smokeIsPresent) return;

        Gizmos.color = Color.blue;
        Vector3 boundsScale = size / gridScale;
        Gizmos.DrawWireCube(transform.position + boundsScale * 0.5f, boundsScale);

        IterateThroughGrid((x, y, z) =>
        {
            float density = grid[x, y, z];
            float oldDensity = _oldGrid[x, y, z];
            float fillRatio = density / maxDensityPerSpace;
            Color c = fillRatio > 1 ? Color.red : Color.gray;

            Vector3Int gridPosition = terrainData.ChunkToGridCoords(chunkPositionInGrid, new Vector3Int(x, y, z));
            Vector3 position = terrainData.GridToWorldPosition(gridPosition);
            Vector3 scale = Vector3.one / gridScale;
            scale *= Mathf.Clamp01(fillRatio);

            Gizmos.color = c;
            Gizmos.DrawCube(position, scale);
        });
    }

    public float GetDensity(Vector3Int position) => grid[position.x, position.y, position.z];
    public void AddSmoke(Vector3Int position, float amount)
    {
        grid[position.x, position.y, position.z] += amount;
        smokeIsPresent = true;
    }

    public void PrepareForStep()
    {

        if (smokeIsPresent == false) return;
        // TO DO: I think that data is being moved into adjacent squares before data is shifted from newGrid to oldGrid.
        // I think I need to do the data shifting in each chunk, before I start changing things.


        // Move current data into old grid, and clear main grid to be populated
        IterateThroughGrid((x, y, z) =>
        {
            _oldGrid[x, y, z] = grid[x, y, z];
            grid[x, y, z] = 0;
        });
    }
    public void DissipationStep()
    {
        if (smokeIsPresent == false) return;

        // Spread smoke
        IterateThroughGrid((x, y, z) =>
        {
            CheckToDissipatePressure(x, y, z, out float newPressure);
            // Value is added rather than replaced, so that smoke donated from other spaces isn't overwritten.
            grid[x, y, z] += newPressure;
        });

        // Smoke density of all spaces decays over time, so if new smoke isn't introduced everything dissipates
        IterateThroughGrid((x, y, z) => grid[x, y, z] = Mathf.MoveTowards(grid[x, y, z], 0, decaySpeed * deltaTime));

        // Put script to sleep if there's no smoke to simulate.
        // When more smoke is introduced, that'll automatically wake it up
        smokeIsPresent = false;
        IterateThroughGrid((x, y, z) =>
        {
            if (grid[x, y, z] > 0) smokeIsPresent = true;
        });
    }
    public void Clear() => IterateThroughGrid((x, y, z) => grid[x, y, z] = 0);

    /// <summary>
    /// Spreads smoke from spaces with an overly high pressure, then returns the corrected value for that space.
    /// </summary>
    void CheckToDissipatePressure(int x, int y, int z, out float density)
    {
        // Do nothing if there's no smoke
        density = _oldGrid[x, y, z];
        if (density <= 0) return;

        #region Check if pressure is too high

        // Get max acceptable density. If space is occupied terrain, there should be no smoke there at all.
        float maxAcceptablePressure = terrainData.containsTerrain[x, y, z] ? 0 : maxDensityPerSpace;
        // No need to spread smoke if there's no excess
        float excess = density - maxAcceptablePressure;
        if (excess <= 0) return;

        #endregion

        // Determine if there are spaces available to spread smoke to
        int availableNeighbours = 0;
        for (int n = 0; n < neighbourOffsets.Length; n++)
        {
            // Get adjacent space
            Vector3Int neighbour = new Vector3Int(x, y, z) + neighbourOffsets[n];

            // If space is not part of the terrain grid, treat it as empty.
            // If a space is occupied by solid terrain, don't allow smoke in
            Vector3Int neighbourOnGrid = terrainData.ChunkToGridCoords(chunkPositionInGrid, neighbour);
            bool outside = MiscFunctions.IsIndexOutsideArray(terrainData.gridSize, neighbourOnGrid);
            if (!outside && terrainData.containsTerrain[neighbourOnGrid.x, neighbourOnGrid.y, neighbourOnGrid.z]) continue;

            // Add to list of valid neighbours
            availableNeighboursNonAlloc[availableNeighbours] = neighbour;
            availableNeighbours += 1;
        }

        // If there isn't anywhere for the smoke to escape to, nothing else to do
        if (availableNeighbours <= 0) return;

        // If there are neighbouring spaces to spread smoke to, divide amongst them and subtract that value from density
        float toSpread = density;
        //float toSpread = excess;

        // Calculate how much excess smoke should be spread this frame. Moving all of it would make the cloud expand to full size instantly
        // A higher volume of gas will expand faster due to higher pressure, and expand to fill all available space.
        // So the amount of gas spread should be proportional to the current pressure, and divided equally amongst all neighbours.
        float toSpreadThisStep = toSpread;

        // If grid is finer, moving to a single adjacent square means a smaller step, so more needs to be spread to each square per step.
        // Of course, we don't want to spread more than is actually there
        toSpreadThisStep = Mathf.Min(toSpreadThisStep * gridScale, toSpread);

        // Divide smoke between itself and neighbouring spaces
        // TO DO: should I use noise to make the spreading slightly uneven, resulting in more billowy clouds?
        float split = toSpreadThisStep / (availableNeighbours + 1);
        for (int i = 0; i < availableNeighbours; i++)
        {
            // Add portion of smoke to neighbouring space
            // If space is outside chunk, search for neighbouring chunks
            Vector3Int n = availableNeighboursNonAlloc[i];
            if (MiscFunctions.IsIndexOutsideArray(size, n))
            {
                // Find adjacent grid
                n = terrainData.ChunkToGridCoords(chunkPositionInGrid, n);
                terrainData.GridToChunkCoords(n, out Vector3Int chunkPosition, out Vector3Int positionInChunk);

                // Do nothing if chunk cannot be found (on edge of grid, let smoke disappear)
                if (MiscFunctions.IsIndexOutsideArray(parentGrid.chunkGridSize, chunkPosition)) continue;

                SmokeChunk neighbouringChunk = parentGrid.chunks[chunkPosition.x, chunkPosition.y, chunkPosition.z];
                // Find correct chunk and introduce smoke there instead
                neighbouringChunk.AddSmoke(positionInChunk, split);
            }
            else
            {
                grid[n.x, n.y, n.z] += split;
            }
        }
        density -= toSpreadThisStep;
        density += split;
    }
    
    


    void IterateThroughGrid(System.Action<int, int, int> action) => MiscFunctions.IterateThroughGrid(size, action);
}