using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Hitbox : MonoBehaviour
{
    public float damageMultiplier = 1;
    public bool isCritical;
    public Health sourceHealth;

    [Header("Collision Damage")]
    float minimumCollisionForceToDamage = 12;
    float damagePerCollisionForceUnit = 5f;
    float stunPerCollisionForceUnit = 5f;

    Collider c;

    public Collider collider => c ??= GetComponent<Collider>();
    public Character attachedTo => (sourceHealth != null) ? sourceHealth.attachedTo : null;

    public void Damage(int damage, int stun, DamageType type, Entity attacker, bool critical = false)
    {
        if (sourceHealth == null) return;
        sourceHealth.Damage(damage, stun, critical, type, attacker);
    }

    public void Damage(int damage, float criticalMultiplier, int stun, DamageType type, Entity attacker)
    {
        if (isCritical)
        {
            damage = Mathf.RoundToInt(damage * criticalMultiplier);
            stun = Mathf.RoundToInt(stun * criticalMultiplier);
        }
        Damage(damage, stun, type, attacker, isCritical);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Figure out velocity
        float force = collision.relativeVelocity.magnitude;
        // If a rigidbody is present, multiply the mass accordingly
        Rigidbody rb = collision.rigidbody;
        if (rb != null) force *= rb.mass;

        // If the force isn't enough to register, cancel
        if (force <= minimumCollisionForceToDamage) return;
        force -= minimumCollisionForceToDamage;

        // Calculate damage and stun accordingly
        float damage = force * damagePerCollisionForceUnit;
        float stun = force * stunPerCollisionForceUnit;
        Entity thingThatDamagedThisHitbox = collision.gameObject.GetComponent<Entity>();
        Damage(Mathf.RoundToInt(damage), Mathf.RoundToInt(stun), DamageType.Impact, thingThatDamagedThisHitbox);
    }
}
