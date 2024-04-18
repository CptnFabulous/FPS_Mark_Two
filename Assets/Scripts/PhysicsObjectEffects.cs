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
        // Calculate multiplier for effects
        float force = collision.relativeVelocity.magnitude - minImpactForce;
        float forceRange = maxImpactForce - minImpactForce;
        float multiplier = force / forceRange;

        // Cancel if threshold isn't met to bother displaying effects
        if (multiplier <= 0) return;

        // Play effect for each contact point
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint contactPoint = collision.GetContact(i);

            // Checks if one collider is not a parent of the other (check if this actually changes anything)
            Transform a = contactPoint.thisCollider.transform;
            Transform b = contactPoint.otherCollider.transform;
            if (a.parent == b || b.parent == a) continue;

            impactEffect.Play(collision.collider.gameObject, rootEntity, contactPoint.point, contactPoint.normal, multiplier);
        }
    }
}
