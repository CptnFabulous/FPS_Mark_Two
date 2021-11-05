using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Hitbox : MonoBehaviour
{
    public float damageMultiplier = 1;
    public bool isCritical;
    public Health sourceHealth;

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
}
