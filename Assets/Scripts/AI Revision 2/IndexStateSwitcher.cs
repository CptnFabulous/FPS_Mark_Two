using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class IndexStateSwitcher : MonoBehaviour
{
    public StateController controller;
    public SingleInput scrollInput;
    public UnityEvent<int> onIndexChanged;

    private void Awake()
    {
        scrollInput.onActionPerformed.AddListener(ProcessInput);
    }
    void ProcessInput(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        controller.stateIndex += Mathf.RoundToInt(value);

        int nextStateIndex = MiscFunctions.IndexOfInArray(controller.states, controller.nextState);
        if (nextStateIndex < 0 || nextStateIndex >= controller.states.Length) return;
        onIndexChanged.Invoke(nextStateIndex);
    }
}