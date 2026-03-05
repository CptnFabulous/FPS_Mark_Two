using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Entity entity;
    public Entity spawnedBy;

    [Header("Ballistics")]
    public float weight = 1;
    public float diameter = 0.05f;
    public float startingVelocity = 100;
    public DetectionProfile detection;

    [Header("Impact")]
    public DamageDealer damageStats;

    public RaycastHit surfaceHit;
    Vector3 velocity;

    void Start()
    {
        velocity = transform.forward * startingVelocity;
    }
    void Update()
    {
        transform.LookAt(transform.position + velocity);

        Vector3 position = transform.position;

        bool thingHit = CalculateTrajectoryDelta(ref position, ref velocity, weight, diameter, Time.deltaTime, detection.mask, out surfaceHit);
        if (thingHit) OnHit(surfaceHit);

        transform.position = position;
    }
    public void OnHit(RaycastHit thingHit)
    {
        surfaceHit = thingHit;
        damageStats.AttackObject(thingHit.collider.gameObject, spawnedBy, entity, thingHit.point, velocity, thingHit.normal);
    }

    #region Additional functions
    public void Ricochet(ref Vector3 position, ref Vector3 velocity, float lengthToTravel, RaycastHit surfaceHit, float velocityDecayMultiplier = 0.75f)
    {
        position = surfaceHit.point;

        // Change velocity
        velocity = Vector3.Reflect(velocity, surfaceHit.normal);
        velocity *= velocityDecayMultiplier;
        // Check how close the hit point is, and how far the projectile needs to travel after ricocheting
        float outDistance = lengthToTravel - surfaceHit.distance;
        // Update position based on position after ricocheting
        position = surfaceHit.point + outDistance * velocity.normalized;
    }
    public void SpawnObjectAtImpactPoint(GameObject prefab)
    {
        Instantiate(prefab, surfaceHit.point, Quaternion.identity);
    }
    public void SpawnObjectForwardOffSurface(GameObject prefab)
    {
        GameObject newObject = Instantiate(prefab);
        StickObjectToSurface(newObject.transform, surfaceHit, Vector3.forward);
    }
    public void EmbedProjectileInSurface(bool rotateToStick)
    {
        if (rotateToStick)
        {
            StickObjectToSurface(transform, surfaceHit, Vector3.forward);
        }
        else
        {
            transform.parent = surfaceHit.transform;
        }
    }
    public static void StickObjectToSurface(Transform objectToStick, RaycastHit surface, Vector3 rotationAxis, float distanceOffSurface = 0)
    {
        objectToStick.position = surface.point + (surface.normal * distanceOffSurface);
        objectToStick.rotation = Quaternion.FromToRotation(rotationAxis, surface.normal);
        objectToStick.parent = surface.transform;
    }
    #endregion

    public static bool CalculateTrajectoryDelta(ref Vector3 position, ref Vector3 velocity, float mass, float radius, float deltaTime, LayerMask hitDetection, out RaycastHit rh)
    {
        // TO DO: have different variants of this for regular raycasts, boxcasts, capsulecasts, etc., to count for different object shapes

        float lengthToTravel = velocity.magnitude * deltaTime;
        bool surfaceHit = Physics.SphereCast(position, radius, velocity, out rh, lengthToTravel, hitDetection);
        if (surfaceHit)
        {
            // TO DO: update position and velocity differently in response to surface hit
            // Determine whether to penetrate, ricochet or embed

            // As of yet, ignore that stuff and just stop movement as soon as we hit something
            return true;
        }
        else
        {
            // Move bullet linearly as expected
            position += velocity.normalized * lengthToTravel;
        }

        // Alter velocity based on external factors
        Vector3 velocityChange = Vector3.zero;
        velocityChange += Physics.gravity * deltaTime; // Gravity
        // To do: wind
        // To do: modify velocity change based on drag

        velocity += velocityChange;

        return surfaceHit;
    }
}