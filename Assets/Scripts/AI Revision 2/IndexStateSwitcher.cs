using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class IndexStateSwitcher : MonoBehaviour
{
    [System.Serializable]
    public struct InputStatePair
    {
        public StateFunction state;
        public SingleInput input;
    }

    public StateController controller;
    public InputStatePair[] states;
    public StateFunction neutralState;
    public UnityEvent<int> onIndexChanged;

    [Header("Scrolling")]
    public SingleInput scrollInput;

    private void Awake()
    {
        for (int i = 0; i < states.Length; i++)
        {
            int index = i;
            states[index].input.onActionPerformed.AddListener((context) => ProcessInput(context, states[index].state));
        }

        if (scrollInput != null) scrollInput.onActionPerformed.AddListener(ProcessScrollInput);
    }
    void ProcessInput(InputAction.CallbackContext context, StateFunction state)
    {
        // If state is not active, switch to it.
        // If it's already active, switch to the 'null' state
        StateFunction toSwitchTo = controller.currentState != state ? state : neutralState;
        controller.SwitchToState(toSwitchTo);

        int nextStateIndex = MiscFunctions.IndexOfInArray(controller.states, controller.nextState);
        if (nextStateIndex < 0 || nextStateIndex >= controller.states.Length) return;
        onIndexChanged.Invoke(nextStateIndex);
    }
    void ProcessScrollInput(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        controller.stateIndex += Mathf.RoundToInt(value);

        int nextStateIndex = MiscFunctions.IndexOfInArray(controller.states, controller.nextState);
        if (nextStateIndex < 0 || nextStateIndex >= controller.states.Length) return;
        onIndexChanged.Invoke(nextStateIndex);
    }
}