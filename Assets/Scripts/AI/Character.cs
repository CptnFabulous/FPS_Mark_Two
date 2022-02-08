using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : Entity
{
    public Faction affiliation;
    public Health health;

    public bool IsHostileTowards(Character other)
    {
        return affiliation.IsHostileTowards(other.affiliation);
    }

    public override void Delete()
    {
        health.Damage(health.data.max * 999, DamageType.DeletionByGame, null);
    }
}
