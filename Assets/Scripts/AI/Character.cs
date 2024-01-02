using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : Entity
{
    [Header("Character data")]
    public Faction affiliation;
    public Health health;
    public Ragdoll characterModel;

    public override Bounds bounds => health.HitboxBounds;
    public Vector3 RelativeCentreOfMass(Vector3 hypotheticalTransformPosition)
    {
        Vector3 offset = CentreOfMass - transform.position;
        return hypotheticalTransformPosition + offset;
    }
    public abstract Transform LookTransform { get; }
    public abstract ICharacterLookController lookController { get; }
    public abstract Vector3 aimDirection { get; }
    public Vector3 RelativeLookOrigin(Vector3 hypotheticalTransformPosition)
    {
        Vector3 offset = LookTransform.position - transform.position;
        return hypotheticalTransformPosition + offset;
    }
    //public abstract Vector3 LookDirection { get; }
    public abstract LayerMask lookMask { get; }
    public abstract LayerMask attackMask { get; }

    public abstract Vector3 MovementDirection { get; }
    public Vector3 LocalMovementDirection => transform.InverseTransformDirection(MovementDirection);

    public override IList<Collider> colliders => health.HitboxColliders;


    public WeaponHandler weaponHandler => (this as Player) != null ? (this as Player).weapons : null;
    public AmmunitionInventory ammo => (weaponHandler != null) ? weaponHandler.ammo : null;


    public abstract Character target { get; }
    public bool IsHostileTowards(Character other)
    {
        return affiliation.IsHostileTowards(other.affiliation);
    }

    public override void Delete()
    {
        health.Damage(health.data.max * 999, 0, false, DamageType.DeletionByGame, null);
    }

    public virtual void Die()
    {
        if (characterModel != null)
        {
            characterModel.transform.SetParent(null);
            characterModel.enabled = true;
        }
    }
}
