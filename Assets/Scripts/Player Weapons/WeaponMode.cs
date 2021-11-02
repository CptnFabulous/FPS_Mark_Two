using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class WeaponMode : MonoBehaviour
{
    public string description = "A distinct attack mode for a weapon.";
    public Sprite icon;
    public float switchSpeed;
    public UnityEvent onSwitch;


    public abstract void UpdateLoop(WeaponHandler user);

    public abstract bool InAction { get; }
}
