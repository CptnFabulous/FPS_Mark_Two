using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    Vector3Int _chunkPositionInGrid;
    float[,,] grid;
    float[,,] _oldGrid;
    //Vector3[,,] velocityGrid;
    //Vector3[,,] oldVelocityGrid;
    VoxelMesh cloudMesh;

    public Vector3Int size => TerrainGrid.current.chunkSize;
    public Vector3Int chunkPositionInGrid => _chunkPositionInGrid;
    public bool smokeIsPresent { get; private set; } = false;

    public TerrainGrid terrainData => TerrainGrid.current;
    float maxDensityPerSpace => parentGrid.maxDensityPerSpace;
    float decaySpeed => parentGrid.decaySpeedPerSpace;
    //float velocityDecaySpeed => ;
    float deltaTime => parentGrid.timestep;
    int gridScale => parentGrid.gridScale;

    private void Awake()
    {
        parentGrid = GetComponentInParent<SimulatedSmokeGrid>();
        grid = new float[size.x, size.y, size.z];
        _oldGrid = new float[size.x, size.y, size.z];
        _chunkPositionInGrid = MiscFunctions.IndexOfIn3DArray(parentGrid.chunks, this);

        cloudMesh = GetComponent<VoxelMesh>();
    }
    void OnDrawGizmos()
    {
        if (!parentGrid.showDebugData) return;

        if (!smokeIsPresent) return;

        Gizmos.color = Color.blue;
        Vector3 boundsScale = size / gridScale;
        Gizmos.DrawWireCube(transform.position + boundsScale * 0.5f, boundsScale);

        IterateThroughGrid((x, y, z) =>
        {
            float density = grid[x, y, z];
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
        // TO DO: When moving into new grid space, lerp between old and new velocity based on the ratio of old to new smoke
        /*
        // Lerp between old and new velocity based on the ratio of old to new smoke
        float atNeighbour = grid[position.x, position.y, position.z];
        // Closer to zero means existing velocity is imposed
        // Closer to one means new smoke is stopped by neighbouring smoke
        float ratio = atNeighbour / (amount + atNeighbour);
        */

        grid[position.x, position.y, position.z] += amount;
        smokeIsPresent = true;
    }

    /// <summary>
    /// Move current data into old grid, and clear main grid to be populated.
    /// Each time an operation is performed that affects multiple spaces/chunks at once, this function needs to be run on every chunk.
    /// Otherwise the 'old' values are changed partway through updating, resulting in inconsistencies.
    /// </summary>
    public void PrepareForStep()
    {
        if (smokeIsPresent == false) return;
        
        // Move current data into old grid, and clear main grid to be populated
        IterateThroughGrid((x, y, z) =>
        {
            _oldGrid[x, y, z] = grid[x, y, z];
            grid[x, y, z] = 0;
        });
    }
    /*
    public void VelocityStep()
    {
        if (smokeIsPresent == false) return;

        // IDEA: the smoke velocity is mainly needed for spraying clouds, such as from a steam valve or fire extinguisher.
        // What if I just spray out regular Unity particles, but then when each particle comes to a stop or hits a wall it adds smoke to the grid?
        // I may still need the velocity step for stuff like wind and animations, but it may still simplify things.


        // Move smoke into adjacent spaces based on current velocity value
        IterateThroughGrid((x, y, z) =>
        {
            // Ignore if smoke has no velocity
            Vector3 velocity = oldVelocityGrid[x, y, z];
            if (velocity.sqrMagnitude <= 0) return;


            // Based on velocity, how fast the smoke is capable of moving, and how much smoke was moved previously, figure out how much of the current smoke needs to be shifted to an adjacent square.
            // If this runs every step, without some kind of limiter it would mean the smoke always moves one space per step, i.e. the maximum possible speed
            // So I think I need to use the density value to ensure smoke has time to build up before moving to the next space
            
            // But if I force the smoke to reach a certain density before moving any of itself to an adjacent space, that means it'll instantly stop moving any more once it goes below that density.
            // Alternatively, could I have an async 'move smoke from space X' function.
            // That only triggers once it reaches a certain density, but keeps going until the desired amount of smoke has been subtracted and put into the correct spaces?
            
            // ALTERNATIVELY: have the velocity and spread be tied together! Keep the velocity as a direction, and have that determine what direction the smoke goes in.
            // E.g. use the ratio of each axis to determine what squares it's most willing to go into.
            // The overall velocity in each direction can be compared against some kind of speed value (in the opposite direction) to determine the weighting against going in one specific direction vs spreading in all directions.
            // ALTERNATIVELY ALTERNATIVELY: wait until a certain density is reached, ONLY IF the desired spaces to move to don't already contain smoke. Otherwise just move everything.

            // But I think the top priority is making the smoke spawn from particle impacts, which will be much easier and serve most use-cases.
            

            float speed = velocity.magnitude;

            float maximumSpeed = 1 / parentGrid.timestep / gridScale;

            float smokeToMove = 

            // TO DO: calculate what adjacent spaces are in the direction the smoke needs to move in

            // Based on the plus and minus of each axis, figure out which directly adjacent spaces it would move into
            // Then calculate which diagonals would also be affected, and by which axes
            // Calculate what proportions of 
        });

        // Continuously decay smoke velocity due to resistance from surrounding air
        IterateThroughGrid((x, y, z) =>
        {
            velocityGrid[x, y, z] = Vector3.MoveTowards(velocityGrid[x, y, z], Vector3.zero, velocityDecaySpeed * deltaTime);
        });
    }
    */
    public void SpreadStep()
    {
        if (smokeIsPresent == false) return;

        // Spread smoke
        IterateThroughGrid((x, y, z) =>
        {
            CheckToSpreadSmoke(x, y, z, out float newPressure);
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
    void CheckToSpreadSmoke(int x, int y, int z, out float density)
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
            Vector3Int n = availableNeighboursNonAlloc[i];
            MoveSmokeToNeighbour(split, n);
        }
        density -= toSpreadThisStep;
        density += split;
    }
    void MoveSmokeToNeighbour(float toMove, Vector3Int neighbour)
    {
        // If space is outside chunk, search for neighbouring chunks
        if (MiscFunctions.IsIndexOutsideArray(size, neighbour))
        {
            // Find adjacent grid
            Vector3Int gridCoords = terrainData.ChunkToGridCoords(chunkPositionInGrid, neighbour);
            terrainData.GridToChunkCoords(gridCoords, out Vector3Int chunkPosition, out Vector3Int positionInChunk);

            // Do nothing if chunk cannot be found (on edge of grid, let smoke disappear)
            if (MiscFunctions.IsIndexOutsideArray(parentGrid.chunkGridSize, chunkPosition)) return;

            // Find correct chunk and introduce smoke there instead
            SmokeChunk neighbouringChunk = parentGrid.chunks[chunkPosition.x, chunkPosition.y, chunkPosition.z];
            neighbouringChunk.AddSmoke(positionInChunk, toMove);
        }
        else
        {
            AddSmoke(neighbour, toMove);
        }
    }
    void IterateThroughGrid(System.Action<int, int, int> action) => MiscFunctions.IterateThroughGrid(size, action);














    public void UpdateMesh()
    {
        // Do wacky marching cubes stuff
        if (smokeIsPresent)
        {
            cloudMesh.Calculate(grid, size, gridScale);
        }
        else
        {
            cloudMesh.Clear();
        }
    }
}






