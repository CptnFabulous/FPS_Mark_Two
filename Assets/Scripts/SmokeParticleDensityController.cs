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
        public SmokeCloud[] clouds = new SmokeCloud[4096];
        public int[] indices = new int[4096];
        public int numberOfParticles;
    }

    public float minimumAcceptableDistance = 0.25f;
    public float resolveVectorMagnitude = 0.1f;

    Dictionary<Vector3Int, ParticleGridSpace> gridSpaceDictionary = new Dictionary<Vector3Int, ParticleGridSpace>();

    void FixedUpdate()
    {
        // Reset particle and resolver data, so it can be calculated properly this frame
        foreach (ParticleGridSpace space in gridSpaceDictionary.Values) space.numberOfParticles = 0;
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
    }
    private void OnDrawGizmosSelected()
    {
        IterateThroughParticles((cloud, index) =>
        {
            if (index >= cloud.particleEmitter.particleCount) return;

            ParticleSystem.Particle realParticle = cloud.particleArray[index];

            /*
            // Draw line from origin to particle
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(cloud.transform.position, realParticle.position);
            */

            /// Show resolver vectors for each particle
            Vector3 resolverVector = cloud.particleOffsetResolvers[index];
            if (resolverVector.sqrMagnitude <= 0) return;

            Gizmos.color = Color.black;
            Gizmos.DrawRay(realParticle.position, resolverVector/* / Time.deltaTime*/);
        });
    }



    void ResetResolverVectors(SmokeCloud cloud, int index)
    {
        cloud.particleOffsetResolvers[index] = Vector3.zero;
    }
    void CalculateParticleGridPosition(SmokeCloud cloud, int index)
    {
        ParticleSystem.Particle realParticle = cloud.particleArray[index];

        // Calculate the closest grid space using math.
        Vector3Int gridPosition = ParticleGridPosition(realParticle.position);

        // If a space doesn't exist, create one.

        //bool spaceExists = gridSpaceDictionary.TryGetValue(gridPosition, out ParticleGridSpace gridSpace);
        if (gridSpaceDictionary.ContainsKey(gridPosition) == false)
        {
            gridSpaceDictionary.Add(gridPosition, new ParticleGridSpace());
        }

        // Add this particle to that grid space (and increment the array count to match)
        ParticleGridSpace gridSpace = gridSpaceDictionary[gridPosition];
        gridSpace.clouds[gridSpace.numberOfParticles] = cloud;
        gridSpace.indices[gridSpace.numberOfParticles] = index;
        gridSpace.numberOfParticles++;
    }
    void CalculateResolverDirection(SmokeCloud cloud, int index)
    {
        float squaredMinimumDistance = minimumAcceptableDistance * minimumAcceptableDistance;

        // For each particle, check for other particles in current and adjacent grid spaces (so values obviously outside the range are ignored)
        ParticleSystem.Particle realParticle = cloud.particleArray[index];
        Vector3Int gridPosBase = ParticleGridPosition(realParticle.position) - Vector3Int.one;
        MiscFunctions.IterateThroughGrid(new Vector3Int(3, 3, 3), CheckAdjacentGridSpace);

        // The function for checking each space.
        void CheckAdjacentGridSpace(Vector3Int neighbourOffset)
        {
            // Check that particles are present in that grid space
            Vector3Int neighbour = gridPosBase + neighbourOffset;
            if (gridSpaceDictionary.TryGetValue(neighbour, out ParticleGridSpace space) == false) return;

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
                cloudOfAdjacentParticle.particleOffsetResolvers[indexOfAdjacentParticle] += resolveVectorMagnitude * direction.normalized;
            }
        }
    }
    void ApplyResolverOffset(SmokeCloud cloud, int index)
    {
        Vector3 resolverVector = cloud.particleOffsetResolvers[index];
        cloud.particleArray[index].position += resolverVector * Time.fixedDeltaTime;
    }


    Vector3Int ParticleGridPosition(Vector3 worldPosition)
    {
        // The size of each grid space should be 2/3 of the minimum acceptable distance before needing to push particles away from each other.
        // This keeps each grid space as small as possible while ensuring that if a particle is in X grid space, all particles within the minimum distance are in either its space or a neighbouring one.
        float gridSpaceLength = minimumAcceptableDistance / 3 * 2;
        return Vector3Int.RoundToInt(worldPosition / gridSpaceLength);
    }
    static void IterateThroughParticles(System.Action<SmokeCloud, int> action)
    {
        foreach (SmokeCloud cloud in SmokeCloud.activeClouds)
        {
            // Iterate through just the number of active particles.
            for (int i = 0; i < cloud.numberOfParticles; i++)
            {
                action.Invoke(cloud, i);
            }
        }
    }
}
