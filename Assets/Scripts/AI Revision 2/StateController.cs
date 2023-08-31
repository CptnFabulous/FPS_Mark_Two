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
    public void SwitchToState(StateFunction newState)
    {
        if (newState.transform.IsChildOf(transform) == false) return;

        current.enabled = false;
        current = newState;
        current.enabled = true;
    }

    private void Start()
    {
        // Pre-emptively disable all attached states
        foreach (StateFunction f in GetComponentsInChildren<StateFunction>()) f.enabled = false;
        // Enable the starting state
        if (currentState != null) currentState.enabled = true;

    }
}

public abstract class StateFunction : MonoBehaviour
{
    StateController _base;

    public StateController controller => _base ??= GetComponentInParent<StateController>();

    protected void SwitchToState(StateFunction newState) => controller.SwitchToState(newState);
}