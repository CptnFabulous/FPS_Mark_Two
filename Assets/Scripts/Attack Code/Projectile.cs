using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Projectile : MonoBehaviour
{
    public Entity entity;
    public Entity spawnedBy;

    [Header("Ballistics")]
    public float weight = 1;
    public float diameter = 0.05f;
    public float startingVelocity = 100;
    public LayerMask detection = ~0;

    [Header("Impact")]
    public DamageDealer damageStats;

    public RaycastHit surfaceHit;
    Vector3 velocity;
    float DetectionLength => velocity.magnitude * Time.deltaTime;

    void Start()
    {
        velocity = transform.forward * startingVelocity;
    }
    void Update()
    {
        transform.LookAt(transform.position + velocity);

        if (Physics.SphereCast(transform.position, diameter, velocity, out surfaceHit, DetectionLength, detection))
        {
            OnHit(surfaceHit);
        }
        
        // Move bullet
        transform.Translate(velocity.normalized * DetectionLength, Space.World);
        velocity = Vector3.MoveTowards(velocity, Physics.gravity, weight * Time.deltaTime);
    }
    public void OnHit(RaycastHit thingHit)
    {
        surfaceHit = thingHit;
        damageStats.AttackObject(thingHit.collider.gameObject, spawnedBy, entity, thingHit.point, velocity, thingHit.normal);
    }

    #region Additional functions
    public void Ricochet(float velocityDecayMultiplier = 0.75f)
    {
        Vector3 newDirection = Vector3.Reflect(velocity, surfaceHit.normal).normalized;
        float outDistance = DetectionLength - surfaceHit.distance;
        transform.position = surfaceHit.point + newDirection * outDistance;
        velocity = newDirection * velocity.magnitude * velocityDecayMultiplier;
        velocity = Vector3.MoveTowards(velocity, Physics.gravity, weight * Time.deltaTime);
    }
    public void CheckIfStopped(float velocityThreshold)
    {
        if (velocity.magnitude < velocityThreshold)
        {
            enabled = false;
        }
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

}