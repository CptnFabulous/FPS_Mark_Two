using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObjectEffects : MonoBehaviour
{
    public ImpactEffect impactEffect;

    float minImpactForce = 0;
    float maxImpactForce = 20;

    Entity e;
    public Entity rootEntity => e ??= GetComponentInParent<Entity>();

    private void OnCollisionEnter(Collision collision)
    {
        float force = collision.relativeVelocity.magnitude - minImpactForce;
        float forceRange = maxImpactForce - minImpactForce;
        float multiplier = force / forceRange;

        foreach (ContactPoint contactPoint in collision.contacts)
        {
            impactEffect.Play(collision.collider.gameObject, rootEntity, contactPoint.point, contactPoint.normal, multiplier);
        }
    }
}
