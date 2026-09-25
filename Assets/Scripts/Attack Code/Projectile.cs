using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public enum ProjecileHitResult
    {
        Embed,
        Bounce,
        Penetrate
    }

    public struct ProjectileData
    {
        public float mass; // Presently unused
        public float drag;
        public float radius;
        public LayerMask hitDetection;
        public System.Action onTravel;
        public System.Func<RaycastHit, ProjecileHitResult> onHit;
    }

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

    ProjectileData projectileData;

    void Start()
    {
        velocity = transform.forward * startingVelocity;

        projectileData.drag = 0;
        projectileData.radius = diameter / 2;
        projectileData.hitDetection = detection.mask;
        projectileData.onHit = (rh) =>
        {
            OnHit(rh);
            return ProjecileHitResult.Embed;
        };
    }
    void Update()
    {
        transform.LookAt(transform.position + velocity);

        Vector3 position = transform.position;
        CalculateTrajectoryDelta(ref position, ref velocity, Time.deltaTime * velocity.magnitude, projectileData);
        transform.position = position;
    }
    public void OnHit(RaycastHit thingHit)
    {
        surfaceHit = thingHit;
        damageStats.AttackObject(thingHit.collider.gameObject, spawnedBy, entity, thingHit.point, velocity, thingHit.normal);
    }

    #region Additional functions
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

    public static void CalculateTrajectoryDelta(ref Vector3 position, ref Vector3 velocity, float deltaDistance, ProjectileData data)
    {
        // Launch a raycast to get the hit data, and calculate how far the projectile actually travels
        bool surfaceHit = Physics.SphereCast(position, data.radius, velocity, out RaycastHit rh, deltaDistance, data.hitDetection);
        float distanceTravelled = surfaceHit ? rh.distance : deltaDistance;
        position += velocity.normalized * distanceTravelled;

        // Calculate how long it took, so we can perform over-time effects (to predict what the velocity should be by the time it reaches its hit point
        float deltaTime = distanceTravelled / velocity.magnitude;

        // Account for drag (replicate Unity's rigidbody system)
        // TO DO: maybe have a setting to switch between realistic physics, how Unity does it?
        velocity = velocity * (1 - deltaTime * data.drag);

        // Alter velocity based on external factors
        Vector3 velocityChange = Vector3.zero;
        velocityChange += Physics.gravity * deltaTime; // Gravity
        // To do: wind

        // Apply to overall velocity value
        velocity += velocityChange;

        float remainingDistance = deltaDistance - distanceTravelled;

        if (data.onTravel != null) data.onTravel.Invoke();

        if (!surfaceHit) return;

        //Debug.Log($"Hit {rh.collider}");

        // Figure out what to do if a surface was hit.
        ProjecileHitResult result = data.onHit.Invoke(rh);

        // Should the bullet bounce, embed or penetrate?

        // If embed, stop all velocity
        // If bounce, reflect velocity off normal
        // If penetrate, do nothing?

        // If bounce or penetrate, invoke another cast of this function for the remaining distance

        switch (result)
        {
            case ProjecileHitResult.Embed:

                velocity = Vector3.zero;

                break;

            case ProjecileHitResult.Bounce:

                // Alter velocity based on angle of hit
                float dot = Vector3.Dot(velocity.normalized, -rh.normal.normalized);

                // Straight-on = 1, skating = 0
                float angleMultiplier = (1 - dot);
                velocity *= angleMultiplier;

                // Reflect velocity off surface
                velocity = Vector3.Reflect(velocity, rh.normal);

                // TO DO: reduce velocity based on bounce coefficient?

                CalculateTrajectoryDelta(ref position, ref velocity, remainingDistance, data);

                break;

            case ProjecileHitResult.Penetrate:

                CalculateTrajectoryDelta(ref position, ref velocity, remainingDistance, data);

                break;
        }
    }
}