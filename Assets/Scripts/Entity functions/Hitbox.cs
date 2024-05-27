using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Hitbox : MonoBehaviour
{
    public float damageMultiplier = 1;
    public bool isCritical;
    public Health sourceHealth;

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
        sourceHealth.DamageFromPhysicsCollision(collision, this);
    }
}
