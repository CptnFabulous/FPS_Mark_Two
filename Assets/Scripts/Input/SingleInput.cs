using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;

public class SingleInput : MonoBehaviour
{
    [SerializeField] string mapName;
    [SerializeField] string actionName;
    [SerializeField, Tooltip("Optional, to directly assign a visual prompt")] InputPrompt inputPrompt;

    public UnityEvent<InputAction.CallbackContext> onActionPerformed;

    public PlayerInput player { get; private set; }
    public InputAction action { get; private set; }
    public InputActionMap map { get; private set; }

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
        action.performed += onActionPerformed.Invoke;
    }
    private void OnDisable()
    {
        action.performed -= onActionPerformed.Invoke;
    }
}