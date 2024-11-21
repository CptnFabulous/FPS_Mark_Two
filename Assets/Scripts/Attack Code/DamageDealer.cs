using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DamageDealer
{
    public int damage;
    public int stun;
    public float knockback;
    public DamageType type;
    public float criticalMultiplier = 1;

    [Header("Additional effects")]
    [SerializeField] ImpactEffect impactEffect;
    public UnityEvent onHit;

    public bool AttackObject(GameObject target, Entity attacker, Entity attackingWith, Vector3 point, Vector3 direction, Vector3 normal, float multiplier = 1)
    {
        //if (attacker.transform.IsChildOf(target.transform)) return false;

        // Check that it's not hitting an ally (do nothing if so)
        Entity targetChar = EntityCache<Entity>.GetEntity(target);
        if (attacker.IsHostileTowards(targetChar) == false) return false;

        // Multiply values 
        int d = Mathf.RoundToInt(damage * multiplier);
        int s = Mathf.RoundToInt(stun * multiplier);

        // Apply knockback to the closest rigidbody
        Rigidbody rb = MiscFunctions.GetComponentInParentWhere<Rigidbody>(target.transform, (rb) => rb.isKinematic == false);
        if (rb != null)
        {
            rb.AddForceAtPosition(knockback * multiplier * direction.normalized, point, ForceMode.Impulse);

            // Tell the attacker's health script that it just applied velocity to a physics object, to prevent self damage from physics glitches
            if (attacker.health != null)
            {
                attacker.health.timesPhysicsObjectsWereLaunchedByThisEntity[PhysicsCache.GetRootRigidbody(rb).gameObject] = Time.time;
            }
        }

        // Apply damage and stun to either the hitbox or the health script, if there is one
        Hitbox hb = target.GetComponentInParent<Hitbox>();
        if (hb != null)
        {
            hb.Damage(d, criticalMultiplier, s, type, attacker, attackingWith, direction);
        }
        else
        {
            Health h = target.GetComponentInParent<Health>();
            if (h != null)
            {
                h.Damage(d, s, false, type, attacker, attackingWith, direction);
            }
        }

        // Play damage effects on surface (e.g. sound, impacts, etc.)
        impactEffect?.Play(target, attackingWith, point, normal, multiplier);
        onHit.Invoke();

        return true;
    }
}
