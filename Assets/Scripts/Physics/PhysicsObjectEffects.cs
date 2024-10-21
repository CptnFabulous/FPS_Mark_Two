using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObjectEffects : MonoBehaviour
{
    [SerializeField] Entity _rootEntity;
    public ImpactEffect impactEffect;
    //public ImpactEffect scrapeEffect;


    float minImpactForce = 0;
    float maxImpactForce = 20;
    /*
    float scrapeThreshold = 0.2f;
    float minScrapeForce;
    float maxScrapeForce;
    */

    float effectCooldown = 0.05f; // Try 0.2f if effects start spamming. Maybe the amount should vary based on the class

    float lastTimeEffectPlayed = 0;

    public Entity rootEntity => _rootEntity ??= GetComponentInParent<Entity>();

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time - lastTimeEffectPlayed < effectCooldown) return;
        lastTimeEffectPlayed = Time.time;
        
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
    /*
    private void OnCollisionStay(Collision collision)
    {
        Vector3 direction = collision.relativeVelocity;
        Vector3 normal = collision.contacts[0].normal;
        for (int i = 1; i < collision.contactCount; i++)
        {
            normal += collision.contacts[i].normal;
        }

        // Calculate the dot (if the value is close to zero then the two objects are scraping against each other)
        // Since it doesn't matter whether the scrape is moving towards or away, just get the absolute value
        // Since 0 means a 90 degree angle (which is what we want, change the value to 1 - value, so zero is no scrape and 1 is full scrape
        float dot = Vector3.Dot(direction, normal);
        float scrapeMultiplier = 1 - Mathf.Abs(dot);
        if (scrapeMultiplier < scrapeThreshold) return;

        // Multiply scrapeValue by some kind of multiplier between the min and max scrape force
        // Play the impact effect at that intensity

    }
    */

    /*
    private void OnCollisionExit(Collision collision)
    {
        
    }
    */
}
