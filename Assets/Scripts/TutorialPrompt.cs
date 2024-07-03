using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class TutorialPrompt : MonoBehaviour
{
    public GUIButtonPrompt input;
    public TMP_Text actionNameDisplay;

    string namePrefix = "Tutorial prompt: ";

    [SerializeField] string mapName;
    [SerializeField] string actionName;

    public InputAction action { get; private set; }
    public InputActionMap map { get; private set; }

    PlayerInput targetedPlayer;

    private void OnValidate() => AssignInput(false);
    private void Awake() => AssignInput(true);

    void AssignInput(bool runtime)
    {
        targetedPlayer = FindObjectOfType<PlayerInput>();
        if (targetedPlayer == null) return;

        map = targetedPlayer.actions.FindActionMap(mapName);
        if (map == null) return;

        action = map.FindAction(actionName);
        if (action == null) return;

        name = namePrefix + action.name;
        if (runtime) input.AssignAction(action, targetedPlayer);

        if (actionNameDisplay != null) actionNameDisplay.text = action.name;
    }
}
