using CptnFabulous.MiscUtility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public struct ParticleCollisionInfo
{
    public ParticleSystem emitter { get; }
    public ParticleCollisionEvent particleData { get; }
    public Entity sourceEntity { get; }
    public float strengthRatio { get; }

    public ParticleCollisionInfo(ParticleSystem emitter, ParticleCollisionEvent particleData, Entity sourceEntity, float strengthRatio)
    {
        this.emitter = emitter;
        this.particleData = particleData;
        this.sourceEntity = sourceEntity;
        this.strengthRatio = strengthRatio;
    }
}

[RequireComponent(typeof(ParticleSystem))]
public class EffectOnParticleHit : MonoBehaviour
{
    public Entity sourceEntity;
    public float maxVelocity = 5f;
    public UnityEvent<ParticleCollisionInfo> invokeOnImpact;

    ParticleSystem _emitter;
    List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    public ParticleSystem emitter => ComponentUtility.AutoCache(ref _emitter, gameObject);

    private void OnParticleCollision(GameObject other)
    {
        int numberOfCollisions = emitter.GetCollisionEvents(other, collisionEvents);
        //int safeCollisionEventSize = ParticlePhysicsExtensions.GetSafeCollisionEventSize(emitter);
        //Debug.Log($"{name}: {safeCollisionEventSize}, {numberOfCollisions}");
        for (int i = 0; i < numberOfCollisions; i++)
        {
            ParticleCollisionEvent pce = collisionEvents[i];

            // Calculate impact strength proportional to expected range
            float multiplier = pce.velocity.magnitude / maxVelocity;
            // Create a new struct and invoke events
            ParticleCollisionInfo info = new ParticleCollisionInfo(emitter, pce, sourceEntity, multiplier);
            invokeOnImpact.Invoke(info);
        }
    }
}
