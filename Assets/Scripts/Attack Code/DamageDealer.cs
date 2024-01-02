using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DamageDealer
{
    [SerializeField] int damage;
    [SerializeField] float criticalMultiplier = 1;
    [SerializeField] DamageType type;
    [SerializeField] int stun;
    [SerializeField] float knockback;

    [SerializeField] UnityEngine.Events.UnityEvent onHit;

    public void AttackObject(GameObject target, Entity attacker, Vector3 point, Vector3 direction)
    {
        if (attacker.transform.IsChildOf(target.transform)) return;
        
        
        // Apply knockback to the closest rigidbody
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

        // Apply knockback
        Rigidbody rb = target.GetComponentInParent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForceAtPosition(point, direction.normalized * knockback);
        }

        // TO DO: Play damage effects on surface (e.g. sound, impacts, etc.)
        onHit.Invoke();
    }
}
