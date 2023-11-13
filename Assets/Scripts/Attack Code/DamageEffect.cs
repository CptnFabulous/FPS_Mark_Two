using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A class containing data for dealing damage in a particular way
/// </summary>
public abstract class DamageEffect : MonoBehaviour
{
    public int baseDamage = 10;
    public int stun = 10;
    public float knockback = 15;
    public DamageType type = DamageType.Piercing;
    //public LayerMask hitDetection = ~0;

    public UnityEvent<Entity> additionalEffects;

    public abstract void DamageFromProjectile(Projectile projectile);
}
