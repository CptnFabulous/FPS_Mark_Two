using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponMode : MonoBehaviour
{
    public float switchSpeed;


    public virtual IEnumerator Attack(WeaponHandler user)
    {
        yield return null;
    }
}
