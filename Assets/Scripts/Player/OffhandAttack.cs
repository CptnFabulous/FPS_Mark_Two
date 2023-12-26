using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OffhandAttack : MonoBehaviour
{
    public WeaponMode attack;
    [SerializeField] string mapName; // "On foot"
    [SerializeField] string actionName; // "Melee Attack"

    private void Awake()
    {
        PlayerInput p = GetComponentInParent<PlayerInput>();
        InputActionMap map = p.actions.FindActionMap(mapName);
        InputAction action = map.FindAction(actionName);
        action.performed += OnAttack;
    }

    void OnAttack(InputAction.CallbackContext context)
    {
        attack.SetPrimaryInput(context.ReadValueAsButton());
    }
}
