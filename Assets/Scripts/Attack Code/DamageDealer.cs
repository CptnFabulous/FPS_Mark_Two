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

    [SerializeField] UnityEvent onHit;

    public bool AttackObject(GameObject target, Entity attacker, Vector3 point, Vector3 direction)
    {
        //if (attacker.transform.IsChildOf(target.transform)) return false;

        // Check that it's not hitting an ally (do nothing if so)
        Entity targetChar = target.GetComponentInParent<Entity>();
        if (attacker.IsHostileTowards(targetChar) == false) return false;
        
        // Apply damage and stun to either the hitbox or the health script, if there is one
        Hitbox hb = target.GetComponentInParent<Hitbox>();
        if (hb != null)
        {
            hb.Damage(damage, criticalMultiplier, stun, type, attacker);
        }
        else
        {
            Health h = target.GetComponentInParent<Health>();
            if (h != null) h.Damage(damage, stun, false, type, attacker);
        }

        // Apply knockback to the closest rigidbody
        Rigidbody rb = target.GetComponentInParent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForceAtPosition(point, direction.normalized * knockback);
        }

        // Play damage effects on surface (e.g. sound, impacts, etc.)
        onHit.Invoke();

        return true;
    }
}
