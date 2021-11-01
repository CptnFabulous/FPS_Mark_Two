using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Hitbox : MonoBehaviour
{
    public float damageMultiplier = 1;
    public bool isCritical;
    public Health sourceHealth;

    public void Damage(int amount, Entity attacker)
    {
        if (sourceHealth == null)
        {
            return;
        }
        sourceHealth.Damage(amount, attacker);
    }

    public void Damage(int amount, float criticalMultiplier, Entity attacker)
    {
        if (isCritical)
        {
            amount = Mathf.RoundToInt(amount * criticalMultiplier);
        }
        Damage(amount, attacker);
    }
}
