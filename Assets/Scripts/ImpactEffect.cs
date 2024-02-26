using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Impact Effect", menuName = "ScriptableObjects/Impact Effect", order = 0)]
public class ImpactEffect : ScriptableObject
{
    // List of terrain types
    // For each terrain type:
    // Particle effect
    // Impact noise (DiegeticSound)

    public DiegeticSound defaultSound;
    public ParticleSystem defaultImpactEffect;
    static int maxNumberOfSpawnedEffects = 100;

    public void Play(GameObject surfaceCollider, Entity sourceEntity, Vector3 point, Vector3 normal, float intensity = 1)
    {
        // Determine effects based on surface (currently doesn't have support for multiple sound types)
        ParticleSystem effect = defaultImpactEffect;
        DiegeticSound sound = defaultSound;

        // Instantiate impact effect at surface
        if (effect != null)
        {
            ParticleSystem effectToSpawn = ObjectPool.RequestObject(effect, true, maxNumberOfSpawnedEffects);
            StickObjectToSurface(effectToSpawn.transform, surfaceCollider.transform, point, normal, Vector3.forward);
            // TO DO: use intensity to determine size of particles
            effectToSpawn.Play();
        }

        // Play sound effect at point
        if (sound != null)
        {
            sound.Play(point, sourceEntity, intensity);
        }
    }

    public static void StickObjectToSurface(Transform objectToStick, RaycastHit surface, Vector3 rotationAxis, float distanceOffSurface = 0)
    {
        StickObjectToSurface(objectToStick, surface.transform, surface.point, surface.normal, rotationAxis, distanceOffSurface);
    }
    public static void StickObjectToSurface(Transform objectToStick, Transform surface, Vector3 point, Vector3 normal, Vector3 rotationAxis, float distanceOffSurface = 0)
    {
        objectToStick.position = point + (normal * distanceOffSurface);
        objectToStick.rotation = Quaternion.FromToRotation(rotationAxis, normal);
        objectToStick.parent = surface;
    }
}
