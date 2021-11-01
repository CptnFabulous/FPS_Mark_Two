using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PointDamage : MonoBehaviour
{
    public int damage = 10;
    public float criticalMultiplier = 3;
    public float knockback = 15;
    public UnityEvent<RaycastHit> onDamaged;
    public UnityEvent<RaycastHit> onUndamaged;

    public void ProjectileDamage(Projectile projectile)
    {
        Damage(projectile.surfaceHit, projectile.transform.position, projectile.spawnedBy);
    }

    public void Damage(RaycastHit rh, Vector3 origin, Entity attacker)
    {
        bool notAlly = true; // Check for friendly fire (not implemented yet)
        
        Hitbox damageable = rh.collider.GetComponent<Hitbox>();
        if (damageable != null && notAlly) // If a hitbox is present
        {
            Debug.Log("Damaging " + damageable.name + " on frame " + Time.frameCount);
            damageable.Damage(damage, criticalMultiplier, attacker);
            onDamaged.Invoke(rh);
        }
        else
        {
            Debug.Log("Did not damage " + rh.collider.name + " on frame " + Time.frameCount);
            onUndamaged.Invoke(rh);
        }

        if (rh.rigidbody != null && notAlly)
        {
            Vector3 direction = (rh.point - origin).normalized;
            rh.rigidbody.AddForceAtPosition(direction * knockback, rh.point, ForceMode.Impulse);
        }
    }
}
