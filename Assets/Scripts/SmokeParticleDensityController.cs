using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeParticleDensityController : MonoBehaviour
{
    class ParticleGridSpace
    {
        // The data for all the particles in the grid space - their origin cloud and index.
        // The data is stored in two separate arrays, as having to rapidly make instances of a custom struct was awful for performance.
        // I imagine it'd be the same with tuples as well.
        public Vector3Int gridPosition { get; private set; }
        public Vector3 worldPosition { get; private set; }

        public SmokeCloud[] clouds;// = new SmokeCloud[4096];
        public int[] indices;// = new int[4096];
        public int numberOfParticles;

        public ParticleGridSpace(Vector3Int gridPosition, Vector3 worldPosition, int maxSize)
        {
            this.gridPosition = gridPosition;
            this.worldPosition = worldPosition;
            clouds = new SmokeCloud[maxSize];
            indices = new int[maxSize];
        }
    }

    [SerializeField] float minimumAcceptableDistance = 1.5f;
    //public float resolveVectorMagnitude = 1f;
    public float resolveVelocityMagnitude = 10f;
    public float deceleration = 10f;

    Dictionary<Vector3Int, ParticleGridSpace> dictionary = new Dictionary<Vector3Int, ParticleGridSpace>();
    //FixedSizeDictionary<Vector3Int, ParticleGridSpace> gridSpaceDictionary = new FixedSizeDictionary<Vector3Int, ParticleGridSpace>(65536);

    //static readonly Vector3Int neighbourSpaceVolumeToCheck = new Vector3Int(3, 3, 3);

    public static readonly Vector3Int[] neighbourOffsets = new Vector3Int[]
    {
        new Vector3Int(0, 0, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(1, 1, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(1, 0, 1),
        new Vector3Int(0, 1, 1),
        new Vector3Int(1, 1, 1),
    };

    void FixedUpdate()
    {
        // Clear grid space data keys, and the values inside each ParticleGridSpace class, but not the classes themselves (since they can be reused)
        /*
        for (int i = 0; i < gridSpaceDictionary.Count; i++) gridSpaceDictionary.valueArray[i].numberOfParticles = 0;
        gridSpaceDictionary.Clear(true);
        */

        // Clear particle counts
        foreach (ParticleGridSpace gridSpace in dictionary.Values) gridSpace.numberOfParticles = 0;
        // Clear particle resolvers
        IterateThroughParticles(ResetResolverVectors);

        // Ensure current particle data is up-to-date.
        foreach (SmokeCloud cloud in SmokeCloud.activeClouds) cloud.particleEmitter.GetParticles(cloud.particleArray);

        // Check which particles are in what grid spaces
        IterateThroughParticles(CalculateParticleGridPosition);
        // Calculate resolver direction for each particle
        IterateThroughParticles(CalculateResolverDirection);
        // Apply resolver offset for each particle, now that calculations are performed
        IterateThroughParticles(ApplyResolverOffset);
        
        // Apply new particle data
        foreach (SmokeCloud cloud in SmokeCloud.activeClouds)
        {
            cloud.particleEmitter.SetParticles(cloud.particleArray, cloud.numberOfParticles);
        }

        /*
        int totalParticleCount = 0;
        IterateThroughParticles((_, _) => totalParticleCount++);
        Debug.Log(totalParticleCount);
        */
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        foreach (ParticleGridSpace gridSpace in dictionary.Values)
        {
            if (gridSpace.numberOfParticles <= 0) continue;
            Gizmos.DrawWireCube(gridSpace.worldPosition, (minimumAcceptableDistance * 2) * Vector3.one);
        }

        Gizmos.color = Color.black;
        IterateThroughParticles((cloud, index) =>
        {
            if (index >= cloud.particleEmitter.particleCount) return;

            // Show resolver vectors for each particle
            Vector3 resolverVector = cloud.particleOffsetResolvers[index];
            if (resolverVector.sqrMagnitude <= 0) return;

            ParticleSystem.Particle realParticle = cloud.particleArray[index];
            Gizmos.DrawRay(realParticle.position, resolverVector);
        });
    }

    void ResetResolverVectors(SmokeCloud cloud, int index)
    {
        cloud.particleOffsetResolvers[index] = Vector3.zero;
    }
    void CalculateParticleGridPosition(SmokeCloud cloud, int index)
    {
        // Calculate the closest grid space for the particle's position.
        ParticleSystem.Particle realParticle = cloud.particleArray[index];
        Vector3Int gridPosition = WorldToGridPosition(realParticle.position, out _);

        // Check if an entry is present. If not, create one and create a new ParticleGridSpace to assign to it.
        /*
        if (gridSpaceDictionary.TryAddEntry(gridPosition, out int dictionaryIndex))
        {
            gridSpaceDictionary.SetValue(dictionaryIndex, new ParticleGridSpace());
        }
        ParticleGridSpace gridSpace = gridSpaceDictionary.valueArray[dictionaryIndex];
        */
        bool spaceExists = dictionary.TryGetValue(gridPosition, out ParticleGridSpace gridSpace);
        if (!spaceExists)
        {
            gridSpace = new ParticleGridSpace(gridPosition, GridToWorldPosition(gridPosition), 4096);
            dictionary.Add(gridPosition, gridSpace);
        }
        
        // Add this particle to that grid space (and increment the array count to match)
        gridSpace.clouds[gridSpace.numberOfParticles] = cloud;
        gridSpace.indices[gridSpace.numberOfParticles] = index;
        gridSpace.numberOfParticles++;
    }
    void CalculateResolverDirection(SmokeCloud cloud, int index)
    {
        float squaredMinimumDistance = minimumAcceptableDistance * minimumAcceptableDistance;

        // Get the grid space of the current particle.
        ParticleSystem.Particle realParticle = cloud.particleArray[index];
        Vector3Int particleGridPosition = WorldToGridPosition(realParticle.position, out Vector3 nonRoundedGridPosition);

        // Figure out the direction of the neighbouring grid spaces that the particle radius may intrude into.
        // Due to the size of the grid space, we can ensure it'll only intrude in a single direction on each axis.
        Vector3Int offsetDirection = new Vector3Int();
        for (int i = 0; i < 2; i++)
        {
            // For each axis, get the direction of the particle relative to the grid space centre
            // Then 'normalise' (so it neatly shifts over to the next grid space)
            offsetDirection[i] = System.MathF.Sign(nonRoundedGridPosition[i] - particleGridPosition[i]);
        }

        // Iterate through each possible neighbour
        // In this case, each time one of the baase offsets is 'one', it's replaced with the actual offset direction for that axis.
        for (int i = 0; i < 8; i++)
        {
            Vector3Int neighbour = particleGridPosition + (neighbourOffsets[i] * offsetDirection);

            // If a grid space is found, check it. Otherwise move to the next coordinate.
            if (dictionary.TryGetValue(neighbour, out ParticleGridSpace space) == false) continue;
            CheckWithinGridSpace(space);
        }



        /*
        // Iterate through grid spaces to find the ones that are close enough to this particle.
        for (int dictionaryIndex = 0; dictionaryIndex < gridSpaceDictionary.Count; dictionaryIndex++)
        {
            #region Check if this grid position is close enough to the current particle to worry about
            Vector3Int neighbouringPosition = gridSpaceDictionary.keyArray[dictionaryIndex];

            // Check if this grid space is more than one unit too far in any direction.
            bool tooFarAway = false;
            for (int a = 0; a < 2; a++)
            {
                int difference = neighbouringPosition[a] - particleGridPosition[a];
                if (Mathf.Abs(difference) > 1)
                {
                    tooFarAway = true;
                    break;
                }
            }
            // If not, none of the particles in it will be close enough to worry about. Proceed to the next one.
            if (tooFarAway) continue;
            #endregion

            ParticleGridSpace space = gridSpaceDictionary.valueArray[dictionaryIndex];
            CheckWithinGridSpace(space);
        }
        */




        /*
        Vector3Int gridPosBase = particleGridPosition - Vector3Int.one;
        MiscFunctions.IterateThroughGrid(neighbourSpaceVolumeToCheck, CheckAdjacentGridSpace);

        // The function for checking each space.
        void CheckAdjacentGridSpace(Vector3Int neighbourOffset)
        {
            // Check that particles are present in that grid space
            Vector3Int neighbour = gridPosBase + neighbourOffset;
            if (dictionary.TryGetValue(neighbour, out ParticleGridSpace space) == false) return;

            CheckWithinGridSpace(space);
        }
        */

        void CheckWithinGridSpace(ParticleGridSpace space)
        {
            // Check all particles in this grid space
            for (int i = 0; i < space.numberOfParticles; i++)
            {
                // Don't bother with the check if it's the same particle.
                SmokeCloud cloudOfAdjacentParticle = space.clouds[i];
                int indexOfAdjacentParticle = space.indices[i];
                if (indexOfAdjacentParticle == index && cloudOfAdjacentParticle == cloud) continue;

                ParticleSystem.Particle realAdjacentParticle = cloudOfAdjacentParticle.particleArray[indexOfAdjacentParticle];

                // Check distance to adjacent particle (squared values are used for improved performance)
                Vector3 direction = realAdjacentParticle.position - realParticle.position;
                if (direction.sqrMagnitude >= squaredMinimumDistance) continue;

                // If adjacent particle is too close, add a small resolver vector to its corresponding offset.
                // The direction added should be normalised, as a greater distance would mean a greater push, which is opposite of what makes sense with density.
                // TO DO: figure out if resolve offset speed should change based on distance from each particle, and how many other particles it's bunched up against.
                cloudOfAdjacentParticle.particleOffsetResolvers[indexOfAdjacentParticle] += direction.normalized;
            }
        }
    }
    void ApplyResolverOffset(SmokeCloud cloud, int index)
    {
        Vector3 resolverVector = cloud.particleOffsetResolvers[index];
        
        // Method 1: position is directly tweaked frame by frame.
        //cloud.particleArray[index].position += Time.fixedDeltaTime * resolveVectorMagnitude * resolverVector;
        //cloud.particleArray[index].position += resolverVector * Time.fixedDeltaTime;
        
        // Method 2: particle velocity is directly altered.
        cloud.particleArray[index].velocity = Vector3.MoveTowards(cloud.particleArray[index].velocity, resolveVelocityMagnitude * resolverVector, deceleration * Time.fixedDeltaTime);
        //cloud.particleArray[index].velocity = Vector3.MoveTowards(cloud.particleArray[index].velocity, resolverVector, deceleration * Time.fixedDeltaTime);
    }

    Vector3Int WorldToGridPosition(Vector3 worldPosition, out Vector3 nonRounded)
    {
        // The size of each grid space should be the total diameter of the checking volume for a particle.
        // This keeps each grid space as small as possible while ensuring that if a particle is in X grid space, all particles within the minimum distance are in either its space, or adjacent in one axis direction only.
        nonRounded = worldPosition / (minimumAcceptableDistance * 2);
        return Vector3Int.RoundToInt(nonRounded);
    }
    Vector3 GridToWorldPosition(Vector3Int gridPosition)
    {
        Vector3 result = gridPosition;
        return (minimumAcceptableDistance * 2) * result;
    }
    static void IterateThroughParticles(System.Action<SmokeCloud, int> action)
    {
        foreach (SmokeCloud cloud in SmokeCloud.activeClouds)
        {
            // Iterate through just the number of active particles.
            for (int i = 0; i < cloud.numberOfParticles; i++) action.Invoke(cloud, i);
        }
    }
}