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

    public void Damage(int amount, DamageType type, Entity attacker)
    {
        if (sourceHealth == null)
        {
            return;
        }
        sourceHealth.Damage(amount, type, attacker);
    }

    public void Damage(int amount, float criticalMultiplier, DamageType type, Entity attacker)
    {
        if (isCritical)
        {
            amount = Mathf.RoundToInt(amount * criticalMultiplier);
        }
        Damage(amount, type, attacker);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Figure out velocity and deal impact damage accordingly
        float force = collision.relativeVelocity.magnitude;
        Debug.Log("Force: " + force);
        if (force > minimumCollisionForceToDamage)
        {
            float damage = (force - minimumCollisionForceToDamage) * damagePerCollisionForceUnit;
            Debug.Log("Damage: " + damage);
            Entity thingThatDamagedThisHitbox = collision.gameObject.GetComponent<Entity>();
            Damage(Mathf.RoundToInt(damage), DamageType.Impact, thingThatDamagedThisHitbox);
        }
    }
}
