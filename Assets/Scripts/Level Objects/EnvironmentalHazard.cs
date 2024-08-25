using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnvironmentalHazard : MonoBehaviour
{
    public Entity entity;
    public bool enableFromPhysicsEvents = true;

    [Header("Damage")]
    //[SerializeField] DamageType damageType;

    public DamageDealer contactDamage;
    //public DamageDealer continuousDamage;
    //public float tickDelay = 0.2f;



    Dictionary<Entity, float> previouslyDamaged = new Dictionary<Entity, float>();
    float damageCooldown = 0.5f;

    Collider c;
    public Collider collider => c ??= GetComponent<Collider>();

    private void OnCollisionEnter(Collision collision)
    {
        if (enableFromPhysicsEvents == false) return;
        ContactPoint contactPoint = collision.contacts[0];
        DamageCheck(collision.collider, contactPoint.point, -collision.relativeVelocity, contactPoint.normal);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (enableFromPhysicsEvents == false) return;
        DamageCheck(c);
    }

    public void DamageCheck(Rigidbody rb)
    {
        Collider c = rb.GetComponentInChildren<Collider>();
        DamageCheck(c, rb.centerOfMass, rb.velocity, -rb.velocity);
    }
    public void DamageCheck(Collider other)
    {
        Vector3 entityPosition = other.bounds.center;
        Vector3 direction = collider.bounds.center - entityPosition;
        DamageCheck(other, entityPosition, direction, -direction);
    }
    public void DamageCheck(Collider other, Vector3 point, Vector3 direction, Vector3 normal)
    {
        //Debug.Log($"{other}: checking hazard collision");
        Entity e = EntityCache<Entity>.GetEntity(other.gameObject);


        Entity attacker = entity;

        /*
        // Check if the incoming object was recently hit 



        if (PhysicsCache.launchedBy.TryGetValue())
        Entity potentialAttacker = PhysicsCache.launchedBy[].Item1;
        float timeLaunched = PhysicsCache.launchedBy[].Item2;
        */

        // If we damaged the object too recently, ignore
        // (So that damage doesn't happen multiple times due to a single object hitting multiple hitboxes at once)
        
        // TO DO? Make it so if the hit object isn't part of a parent entity, just register that object instead (change the dictionary to use gameobjects instead of entities)
        
        if (previouslyDamaged.TryGetValue(e, out float hitTime) && (Time.time - hitTime) < damageCooldown) return;
        previouslyDamaged[e] = Time.time; // Update the last time hit for the next check

        contactDamage.AttackObject(other.gameObject, attacker, entity, point, direction, normal);
    }
}
