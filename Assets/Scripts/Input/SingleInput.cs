using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class SingleInput : MonoBehaviour
{
    [SerializeField] string mapName;
    [SerializeField] string actionName;
    [SerializeField, Tooltip("Optional, to directly assign a visual prompt")] InputPrompt inputPrompt;

    public UnityEvent<InputAction.CallbackContext> onActionStarted;
    public UnityEvent<InputAction.CallbackContext> onActionPerformed;
    public UnityEvent<InputAction.CallbackContext> onActionCancelled;

    public PlayerInput player { get; private set; }
    public InputAction action { get; private set; }
    public InputActionMap map { get; private set; }

    public bool usingGamepad => player.currentControlScheme.Contains("Gamepad");

    private void Awake()
    {
        if (string.IsNullOrEmpty(mapName)) return;
        if (string.IsNullOrEmpty(actionName)) return;

        player = GetComponentInParent<PlayerInput>();
        map = player.actions.FindActionMap(mapName);
        action = map.FindAction(actionName);

        if (inputPrompt != null)
        {
            inputPrompt.AssignAction(action, player);
        }
    }
    private void OnEnable()
    {
        action.started += onActionStarted.Invoke;
        action.performed += onActionPerformed.Invoke;
        action.canceled += onActionCancelled.Invoke;
    }
    private void OnDisable()
    {
        action.started -= onActionStarted.Invoke;
        action.performed -= onActionPerformed.Invoke;
        action.canceled -= onActionCancelled.Invoke;
    }
}