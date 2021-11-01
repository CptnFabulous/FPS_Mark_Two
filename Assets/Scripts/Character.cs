using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : Entity
{
    /*
    public override void Delete()
    {
        Hitbox hitbox = GetComponentInChildren<Hitbox>();
        if (hitbox != null)
        {
            int damageForGuaranteedKill = Mathf.RoundToInt(hitbox.sourceHealth.data.current * 2);
            hitbox.Damage(damageForGuaranteedKill, null);
        }
        else
        {
            base.Delete();
        }
    }
    */
}
