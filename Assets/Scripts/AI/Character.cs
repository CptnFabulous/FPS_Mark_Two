using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : Entity
{
    public Health health;
    
    public override void Delete()
    {
        health.Damage(health.data.max * 999, DamageType.DeletionByGame, null);
    }
}
