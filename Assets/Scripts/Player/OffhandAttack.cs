using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OffhandAttack : MonoBehaviour
{
    public WeaponMode attack;
    public SingleInput input;

    private void Awake()
    {
        input.onActionPerformed.AddListener(OnAttack);
    }

    void OnAttack(InputAction.CallbackContext context)
    {
        attack.SetPrimaryInput(context.ReadValueAsButton());
    }
}
