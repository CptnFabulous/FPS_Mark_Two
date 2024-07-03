using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StateController : StateFunction
{
    [SerializeField] StateFunction current;

    public UnityEngine.Events.UnityEvent<bool> onSetActive;


    public StateFunction currentState
    {
        get => current;
        set => SwitchToState(value);
    }
    public StateFunction previousState { get; private set; }
    public StateFunction[] states { get; private set; }

    public override void SwitchToState(StateFunction newState)
    {
        // Do nothing if the desired state is not part of the state machine hierarchy
        if (newState.root != root)
        {
            Debug.LogError($"{this}: cannot switch to {newState} because it's not part of this state machine!");
            return;
        }

        // If not a child of this controller, but still in the hierarchy, initiate from its parent controller instead
        if (newState.controller != this)
        {
            newState.controller.SwitchToState(newState);
            return;
        }

        Debug.Log($"{this}: switching from {current} to {newState}");
        if (newState != currentState)
        {
            current.enabled = false;
            previousState = current;
            current = newState;
            current.enabled = true;
        }

        // Trigger state switch in parent controller
        // (iterates up to ensure that all necessary parent controllers are active)
        if (controller != null) controller.SwitchToState(this);
    }

    private void Awake()
    {
        // Get all child states (only immediate children)
        states = GetComponentsInChildren<StateFunction>();
        states = states.Where(s => s.controller == this).ToArray();
        // Pre-emptively disables all states
        foreach (StateFunction f in states) f.enabled = false;
    }
    private void OnEnable() => SetActivity(true);
    private void OnDisable() => SetActivity(false);

    void SetActivity(bool active)
    {
        /*
        foreach (Behaviour c in subComponents)
        {
            c.enabled = active;
        }
        */
        currentState.enabled = active;
        //onSetActive.Invoke(active);
    }
}

public abstract class StateFunction : MonoBehaviour
{
    StateController _base;

    /// <summary>
    /// The controller that this state is managed by. Only registers if it's in a parent GameObject.
    /// </summary>
    public StateController controller => _base ??= transform.parent.GetComponentInParent<StateController>();
    /// <summary>
    /// The root controller in a hierarchy.
    /// <para></para>
    /// </summary>
    public StateFunction root => controller == null ? this : controller.root;
    // If a parent controller is present, ask it for its root.
    // Once a state can't find a parent controller, that state is the root.

    public virtual void SwitchToState(StateFunction newState) => controller.SwitchToState(newState);
}