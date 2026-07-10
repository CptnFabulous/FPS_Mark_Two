using CptnFabulous.MiscUtility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Hitbox : MonoBehaviour
{
    [SerializeField] Entity _attachedTo;
    public bool isCritical;
    public DamageResistanceProfile resistances;

    Collider c;
    Rigidbody _rb;

    public Collider collider => ComponentUtility.AutoCache(ref c, gameObject);
    public Entity attachedTo => ComponentUtility.AutoCache(ref _attachedTo, gameObject, ComponentGetType.InParent);
    public Health sourceHealth => attachedTo.health;
    public Rigidbody rigidbody => ComponentUtility.AutoCache(ref _rb, gameObject, ComponentGetType.InParent);

    public void Damage(int damage, int stun, DamageType type, Entity attacker, Entity weaponUsed, Vector3 direction, bool critical = false)
    {
        if (sourceHealth == null) return;

        float multiplier = resistances[type];

        //Debug.Log($"{attachedTo}: damage resistance profile present, multiplying by {multiplier}");
        if (multiplier == 0) return;

        damage = Mathf.RoundToInt(damage * multiplier);
        stun = Mathf.RoundToInt(stun * multiplier);

        sourceHealth.Damage(damage, stun, critical, type, attacker, weaponUsed, direction);
    }
    public void Damage(int damage, float criticalMultiplier, int stun, DamageType type, Entity attacker, Entity weaponUsed, Vector3 direction)
    {
        if (isCritical)
        {
            damage = Mathf.RoundToInt(damage * criticalMultiplier);
            stun = Mathf.RoundToInt(stun * criticalMultiplier);
        }
        Damage(damage, stun, type, attacker, weaponUsed, direction, isCritical);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (sourceHealth == null) return;
        sourceHealth.DamageFromPhysicsCollision(collision, this);
    }
}
