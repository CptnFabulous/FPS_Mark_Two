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

    public InputAction action { get; private set; }
    public InputActionMap map { get; private set; }

    private void Awake()
    {
        PlayerInput p = GetComponentInParent<PlayerInput>();
        map = p.actions.FindActionMap(mapName);
        action = map.FindAction(actionName);
        action.performed += onActionPerformed.Invoke;
    }
}