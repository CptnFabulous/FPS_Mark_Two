using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PointDamage : MonoBehaviour
{
    public int damage = 10;
    public float criticalMultiplier = 3;
    public float knockback = 15;
    public DamageType type = DamageType.Piercing;
    public UnityEvent<RaycastHit> onDamaged;
    public UnityEvent<RaycastHit> onUndamaged;

    public void ProjectileDamage(Projectile projectile)
    {
        Damage(projectile.surfaceHit, projectile.transform.position, projectile.spawnedBy);
    }

    public void Damage(RaycastHit rh, Vector3 origin, Entity attacker)
    {
        // Check for friendly fire
        Character attacking = attacker as Character;

        Hitbox damageable = rh.collider.GetComponent<Hitbox>();
        Character attacked = rh.collider.GetComponentInParent<Character>();

        // Set canHit to true if both the attacker and target have specified factions, and they are hostile towards each other
        // Or if there aren't two characters to check the factions of
        bool canHit = (attacking == null || attacked == null) || attacking.affiliation.IsHostileTowards(attacked.affiliation);

        if (damageable != null && canHit) // If a hitbox is present
        {
            damageable.Damage(damage, criticalMultiplier, type, attacker);
            onDamaged.Invoke(rh);
        }
        else
        {
            onUndamaged.Invoke(rh);
        }

        if (rh.rigidbody != null && canHit)
        {
            Vector3 direction = (rh.point - origin).normalized;
            rh.rigidbody.AddForceAtPosition(direction * knockback, rh.point, ForceMode.Impulse);
        }
    }
}
