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
    [SerializeField, Tooltip("Optional, to directly assign a visual prompt")] GUIButtonPrompt guiPrompt;

    public UnityEvent<InputAction.CallbackContext> onActionPerformed;

    public PlayerInput player { get; private set; }
    public InputAction action { get; private set; }
    public InputActionMap map { get; private set; }

    private void Awake()
    {
        if (string.IsNullOrEmpty(mapName)) return;
        if (string.IsNullOrEmpty(actionName)) return;

        //Debug.Log(mapName);
        player = GetComponentInParent<PlayerInput>();


        //Debug.Log(player);
        map = player.actions.FindActionMap(mapName);
        //Debug.Log(map);
        action = map.FindAction(actionName);
        //Debug.Log(action);
        action.performed += onActionPerformed.Invoke;

        if (guiPrompt != null)
        {
            guiPrompt.AssignAction(action, player);
        }
    }
}