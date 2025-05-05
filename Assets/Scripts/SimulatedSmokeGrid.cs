using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SimulatedSmokeGrid : MonoBehaviour
{
    public float timestep = 0.01f;
    public float maxDensityPerWorldUnit = 1;
    public float spreadSpeed = 1;
    public float decaySpeed = 0.2f;

    float[,,] grid;
    float[,,] oldGrid;
    float lastTimeSimulated = 0;
    
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

    public TerrainGrid terrainData => TerrainGrid.current;
    Vector3Int gridSize => terrainData.textureResolution;
    float maxDensityPerSpace => maxDensityPerWorldUnit; // TO DO: make sure this value adjusts to reflect the resolution
    float deltaTime => timestep;
    int gridScale => terrainData.resolutionScale;

    private void OnDrawGizmos()
    {
        if (enabled == false) return;
        if (grid == null || oldGrid == null) return;

        IterateThroughGrid((x, y, z) =>
        {
            float density = grid[x, y, z];
            float oldDensity = oldGrid[x, y, z];
            float fillRatio = density / maxDensityPerSpace;
            Color c = fillRatio > 1 ? Color.red : Color.gray;//Color.Lerp(Color.gray, Color.green, fillRatio);

            Vector3 position = terrainData.WorldPositionFromTextureCoordinates(new Vector3Int(x, y, z));
            Vector3 scale = Vector3.one / gridScale;
            //scale *= Mathf.Clamp01(fillRatio);

            c.a = Mathf.Clamp01(fillRatio);
            //c.a = 0.5f;

            Gizmos.color = c;
            Gizmos.DrawCube(position, scale);
        });
    }
    void Start()
    {
        grid = new float[gridSize.x, gridSize.y, gridSize.z];
        oldGrid = new float[gridSize.x, gridSize.y, gridSize.z];
    }
    private void OnEnable()
    {
        lastTimeSimulated = Time.time;
    }
    void Update()
    {
        float timeSinceLastSimulation = Time.time - lastTimeSimulated;
        int missedSteps = Mathf.FloorToInt(timeSinceLastSimulation / timestep);
        for (int i = 0; i < missedSteps; i++)
        {
            SimulationStep();
            lastTimeSimulated += timestep;
        }
    }

    void SimulationStep()
    {
        // Move current data into old grid, and clear main grid to be populated
        IterateThroughGrid((x, y, z) =>
        {
            oldGrid[x, y, z] = grid[x, y, z];
            grid[x, y, z] = 0;
        });

        // Spread smoke
        IterateThroughGrid((x, y, z) =>
        {
            CheckToDissipatePressure(x, y, z, out float newPressure);
            // Value is added rather than replaced, so that smoke donated from other spaces isn't overwritten.
            grid[x, y, z] += newPressure;
        });

        // Smoke density of all spaces decays over time, so if new smoke isn't introduced everything dissipates
        bool allZero = true;
        IterateThroughGrid((x, y, z) =>
        {
            grid[x, y, z] = Mathf.MoveTowards(grid[x, y, z], 0, decaySpeed * deltaTime);
            if (grid[x, y, z] > 0) allZero = false;
        });

        // Put script to sleep if there's no smoke to simulate.
        // When more smoke is introduced, that'll automatically wake it up
        if (allZero) enabled = false;
    }
    /// <summary>
    /// Spreads smoke from spaces with an overly high pressure, then returns the corrected value for that space.
    /// </summary>
    void CheckToDissipatePressure(int x, int y, int z, out float density)
    {
        // Do nothing if there's no smoke
        density = oldGrid[x, y, z];
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
            bool outside = MiscFunctions.IsIndexOutsideArray(oldGrid, neighbour.x, neighbour.y, neighbour.z);
            if (!outside && terrainData.containsTerrain[neighbour.x, neighbour.y, neighbour.z]) continue;
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
            // Add portion of smoke to that space
            // If it's outside the grid, don't actually do anything (smoke is leaving game space)
            Vector3Int n = availableNeighboursNonAlloc[i];
            if (MiscFunctions.IsIndexOutsideArray(oldGrid, n.x, n.y, n.z))
            {
                // TO DO: find adjacent grid
                continue;
            }
            grid[n.x, n.y, n.z] += split;
        }
        density -= toSpreadThisStep;
        density += split;
    }

    void IterateThroughGrid(System.Action<int, int, int> action)
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                for (int z = 0; z < gridSize.z; z++)
                {
                    action.Invoke(x, y, z);
                }
            }
        }
    }

    public void IntroduceSmoke(Vector3 worldPosition, float amount)
    {
        enabled = true;

        // Find the appropriate grid space and increase its smoke density by a certain amount
        Vector3Int gridPos = terrainData.TextureCoordinatesFromWorldPosition(worldPosition);
        if (MiscFunctions.IsIndexOutsideArray(grid, gridPos.x, gridPos.y, gridPos.z)) return;
        if (terrainData.containsTerrain[gridPos.x, gridPos.y, gridPos.z]) return;
        grid[gridPos.x, gridPos.y, gridPos.z] += amount;
        //Debug.Log($"Inserting {amount} smoke into {gridPos}, new density = {grid[gridPos.x, gridPos.y, gridPos.z]}");
    }

    [ContextMenu("Clear smoke")]
    public void Clear() => IterateThroughGrid((x, y, z) => grid[x, y, z] = 0);
}
