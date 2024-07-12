using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnvironmentalHazard : MonoBehaviour
{
    public Entity entity;
    [SerializeField] DamageType damageType;

    Collider c;
    public Collider collider => c ??= GetComponent<Collider>();

    private void OnCollisionEnter(Collision collision) => DamageCheck(collision.collider);
    private void OnTriggerEnter(Collider other) => DamageCheck(other);
    private void DamageCheck(Collider other)
    {
        //Debug.Log($"{other}: checking hazard collision");

        // If the collider is part of an entity, said entity has fallen into the damage zone. Damage it!
        Entity e = EntityCache<Entity>.GetEntity(other.gameObject);
        if (e != null && e.health != null)
        {
            Health h = e.health;
            //Debug.Log($"{e} entered/collided with {this} and will now be killed");
            int damage = h.data.max * 10;
            h.Damage(damage, damage, true, damageType, entity, other.bounds.center - collider.bounds.center);
        }
    }
}
