using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Projectile : MonoBehaviour
{
    public Entity spawnedBy;
    public float weight = 1;
    public float diameter = 0.05f;
    public float startingVelocity = 100;
    public LayerMask detection = ~0;
    public UnityEvent<RaycastHit> onHit;

    public RaycastHit surfaceHit;
    Vector3 velocity;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        velocity = transform.forward * startingVelocity;
    }

    float DetectionLength
    {
        get
        {
            return velocity.magnitude * Time.deltaTime;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.DrawRay(transform.position, velocity, Color.red);
        transform.LookAt(transform.position + velocity);
        if (Physics.SphereCast(transform.position, diameter, velocity, out surfaceHit, DetectionLength, detection))
        {
            //Debug.Log(surfaceHit.collider);
            OnHit(surfaceHit);
        }
        
        // Move bullet
        transform.Translate(velocity.normalized * DetectionLength, Space.World);
        velocity = Vector3.MoveTowards(velocity, Physics.gravity, weight * Time.deltaTime);
        transform.LookAt(transform.position + velocity);

        /*
        More realistic bullet physics ideas

        Static 'Environment' class containing variables for air resistance and wind
        Function: float Environment.Drag(float airResistance), that uses appropriate calculations to determine actual resistance

        velocity += Time.deltaTime * Physics.gravity;
        velocity += Time.deltaTime * -Environment.Drag(airResistance) * velocity.normalized;
        velocity += Time.deltaTime * Environment.wind;

        Simplified thing RooBubba showed me on discord
        float3 desiredVector = desiredMovementComponent.DesiredMovementVector / (1f + frictionFactor * DeltaTime);
        Translated to Unity code
        velocity = velocity / (1f + frictionFactor * Time.deltaTIme);
        */
        //Debug.DrawRay(transform.position, velocity, Color.green);
    }

    

    public void OnHit(RaycastHit thingHit)
    {
        //Debug.Log(thingHit.collider.name);
        surfaceHit = thingHit;
        onHit.Invoke(surfaceHit);
    }



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


}
