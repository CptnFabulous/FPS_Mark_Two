using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;

public class WeaponInfo : MonoBehaviour
{
    [SerializeField] WeaponModeInfo mainHand;
    [SerializeField] WeaponModeInfo offHand;

    WeaponHandler weapons => w ??= GetComponentInParent<WeaponHandler>();
    WeaponHandler w;

    private void LateUpdate()
    {
        mainHand.mode = (weapons.CurrentWeapon != null) ? weapons.CurrentWeapon.CurrentMode : null;
        offHand.mode = weapons.offhand;
    }
}
