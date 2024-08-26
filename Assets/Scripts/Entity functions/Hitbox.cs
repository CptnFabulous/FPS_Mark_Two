using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Hitbox : MonoBehaviour
{
    //public float damageMultiplier = 1;
    [SerializeField] Entity _attachedTo;
    public bool isCritical;
    public DamageResistanceProfile resistances;

    Collider c;

    public Collider collider => c ??= GetComponent<Collider>();
    public Entity attachedTo => _attachedTo ??= GetComponentInParent<Entity>();
    public Health sourceHealth => attachedTo.health;

    public void Damage(int damage, int stun, DamageType type, Entity attacker, Vector3 direction, bool critical = false)
    {
        if (sourceHealth == null) return;

        //force *= hitbox.damageMultiplier;
        if (resistances != null)
        {
            float multiplier = resistances[type];

            //Debug.Log($"{attachedTo}: damage resistance profile present, multiplying by {multiplier}");
            if (multiplier == 0) return;

            damage = Mathf.RoundToInt(damage * multiplier);
            stun = Mathf.RoundToInt(stun * multiplier);
        }

        sourceHealth.Damage(damage, stun, critical, type, attacker, direction);
    }
    public void Damage(int damage, float criticalMultiplier, int stun, DamageType type, Entity attacker, Vector3 direction)
    {
        if (isCritical)
        {
            damage = Mathf.RoundToInt(damage * criticalMultiplier);
            stun = Mathf.RoundToInt(stun * criticalMultiplier);
        }
        Damage(damage, stun, type, attacker, direction, isCritical);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (sourceHealth == null) return;
        sourceHealth.DamageFromPhysicsCollision(collision, this);
    }
}
