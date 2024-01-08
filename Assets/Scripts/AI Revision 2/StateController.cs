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
        if (newState != null && newState.transform.IsChildOf(transform) == false) return;

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