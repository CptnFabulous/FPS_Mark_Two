using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : Entity
{
    [Header("Character data")]
    public Faction affiliation;
    public RegeneratingResource stamina;

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



    public WeaponHandler weaponHandler => (this as Player) != null ? (this as Player).weapons : null;
    public AmmunitionInventory ammo => (weaponHandler != null) ? weaponHandler.ammo : null;


    public abstract Character target { get; }

    protected override void Awake()
    {
        base.Awake();
        health.onDeath.AddListener((_) => Die());
    }

    protected virtual void Die()
    {
        Debug.Log($"{this} is now dying");
    }
}
