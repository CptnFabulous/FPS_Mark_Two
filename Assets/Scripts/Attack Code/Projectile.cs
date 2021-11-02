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
        else // Move bullet
        {
            transform.Translate(velocity.normalized * DetectionLength, Space.World);
            velocity = Vector3.MoveTowards(velocity, Physics.gravity, weight * Time.deltaTime);
        }
        //Debug.DrawRay(transform.position, velocity, Color.green);
    }

    

    public void OnHit(RaycastHit thingHit)
    {
        //Debug.Log(thingHit.collider.name);
        onHit.Invoke(thingHit);
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


    public void StickObjectToSurface(RaycastHit surface, GameObject objectToStick, Vector3 rotationAxis)
    {

    }

}
