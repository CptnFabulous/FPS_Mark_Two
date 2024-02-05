using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateController : MonoBehaviour
{
    [SerializeField] StateFunction current;

    public StateFunction currentState
    {
        get => current;
        set => SwitchToState(value);
    }
    public StateFunction previousState { get; private set; }

    public void SwitchToState(StateFunction newState)
    {
        // Do nothing if already in the current state
        if (newState == currentState) return;

        // Do nothing if state is not part of the state machine
        if (newState != null && newState.transform.IsChildOf(transform) == false)
        {
            Debug.LogError($"{this}: cannot switch to {newState} because it's not part of this state machine!");
            return;
        }

        current.enabled = false;
        previousState = current;
        current = newState;
        current.enabled = true;
    }
    private void Awake()
    {
        foreach (StateFunction f in GetComponentsInChildren<StateFunction>()) f.enabled = false;
    }
    private void OnEnable() => currentState.enabled = true;
    private void OnDisable() => currentState.enabled = false;
}

public abstract class StateFunction : MonoBehaviour
{
    StateController _base;

    public StateController controller => _base ??= GetComponentInParent<StateController>();

    protected void SwitchToState(StateFunction newState) => controller.SwitchToState(newState);
}