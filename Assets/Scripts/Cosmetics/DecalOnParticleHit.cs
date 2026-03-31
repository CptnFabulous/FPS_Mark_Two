using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class DecalOnParticleHit : MonoBehaviour
{
    public ImpactEffect effect;
    public DiegeticSound sound;
    public float maxVelocity = 5f;

    ParticleSystem particleSystem;
    List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    private void Awake()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }
    private void OnParticleCollision(GameObject other)
    {
        int safeCollisionEventSize = ParticlePhysicsExtensions.GetSafeCollisionEventSize(particleSystem);
        int numberOfCollisions = particleSystem.GetCollisionEvents(other, collisionEvents);
        Debug.Log($"{name}: {safeCollisionEventSize}, {numberOfCollisions}");
        for (int i = 0; i < numberOfCollisions; i++)
        {
            ParticleCollisionEvent pce = collisionEvents[i];
            Vector3 point = pce.intersection;
            float multiplier = pce.velocity.magnitude / maxVelocity;
            if (effect != null)
            {
                effect.Play(other, null, point, pce.velocity, pce.normal, transform.up, multiplier);
            }
            if (sound != null)
            {
                Debug.Log($"Playing {sound.name} at {point}");
                sound.Play(point, null, multiplier);
            }
        }
    }
}
