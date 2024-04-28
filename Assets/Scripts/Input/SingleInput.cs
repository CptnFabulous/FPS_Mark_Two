using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class SingleInput : MonoBehaviour
{
    [SerializeField] string mapName;
    [SerializeField] string actionName;
    public UnityEvent<InputAction.CallbackContext> onActionPerformed;

    private void Awake()
    {
        PlayerInput p = GetComponentInParent<PlayerInput>();
        InputActionMap map = p.actions.FindActionMap(mapName);
        InputAction action = map.FindAction(actionName);
        action.performed += onActionPerformed.Invoke;
    }
}