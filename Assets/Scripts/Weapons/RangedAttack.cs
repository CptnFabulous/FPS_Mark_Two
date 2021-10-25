using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedAttack : WeaponMode
{
    public GunGeneralStats stats;
    public GunFireController controls;
    public GunMagazine magazine;
    public GunOptics optics;




    public override IEnumerator Attack(WeaponHandler user)
    {
        return controls.Fire(user, stats);
    }



}
