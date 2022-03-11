using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Hitbox : MonoBehaviour
{
    public float damageMultiplier = 1;
    public bool isCritical;
    public Health sourceHealth;
    public Collider collider { get; private set; }

    [Header("Collision Damage")]
    float minimumCollisionForceToDamage = 12;
    float damagePerCollisionForceUnit = 5f;
    float stunPerCollisionForceUnit = 5f;

    private void Awake()
    {
        collider = GetComponent<Collider>();
    }

    public void Damage(int damage, int stun, DamageType type, Entity attacker, bool critical = false)
    {
        if (sourceHealth == null)
        {
            return;
        }
        sourceHealth.Damage(damage, stun, critical, type, attacker);
    }

    public void Damage(int damage, float criticalMultiplier, int stun, DamageType type, Entity attacker)
    {
        if (isCritical)
        {
            damage = Mathf.RoundToInt(damage * criticalMultiplier);
        }
        Damage(damage, stun, type, attacker, isCritical);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Figure out velocity and deal impact damage accordingly
        float force = collision.relativeVelocity.magnitude;
        if (force > minimumCollisionForceToDamage)
        {
            float damage = (force - minimumCollisionForceToDamage) * damagePerCollisionForceUnit;
            float stun = (force - minimumCollisionForceToDamage) * stunPerCollisionForceUnit;
            Entity thingThatDamagedThisHitbox = collision.gameObject.GetComponent<Entity>();
            Damage(Mathf.RoundToInt(damage), Mathf.RoundToInt(stun), DamageType.Impact, thingThatDamagedThisHitbox);
        }
    }
}
