using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnvironmentalHazard : Entity
{
    [SerializeField] DamageType damageType;

    private void OnCollisionEnter(Collision collision) => DamageCheck(collision.collider);
    private void OnTriggerEnter(Collider other) => DamageCheck(other);
    private void DamageCheck(Collider other)
    {
        // If an entity fell into the trigger zone, 
        Character e = other.GetComponentInParent<Character>();
        if (e != null)
        {
            Debug.Log($"{e} entered/collided with {this} and will now be killed");
            int damage = e.health.data.max * 10;
            e.health.Damage(damage, damage, true, damageType, this);
        }
    }
}
