using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StateController : StateFunction
{
    [SerializeField] Entity _rootEntity;
    [SerializeField] StateFunction current;
    public UnityEngine.Events.UnityEvent<bool> onSetActive;

    public Entity rootEntity => _rootEntity ??= controller.rootEntity;
    public StateFunction currentState => current;
    public StateFunction previousState { get; private set; }
    public StateFunction[] states { get; private set; }

    public StateFunction currentStateInChildren
    {
        get
        {
            // Check the active state. If it's a StateController, check there instead.
            if (currentState is StateController childController) return childController.currentStateInChildren;
            return currentState;
        }
    }
    public StateFunction currentStateInHierarchy => root.currentStateInChildren;

    public override void SwitchToState(StateFunction newState)
    {
        // Do nothing if the desired state is not part of the state machine hierarchy
        if (newState.root != root)
        {
            rootEntity.DebugLog($"{this}: cannot switch to {newState} because it's not part of this state machine!");
            return;
        }

        // If not a child of this controller, but still in the hierarchy, initiate from its parent controller instead
        if (newState.controller != this)
        {
            newState.controller.SwitchToState(newState);
            return;
        }

        if (newState != currentState)
        {
            rootEntity.DebugLog($"{root}: switching from {current} to {newState} on frame {Time.frameCount}");
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

        // If 'current' state is not actually a child state, replace it with one that is
        if (states.Contains(current) == false)
        {
            current = null;
            if (states.Length > 0) current = states[0];
        }
    }
    private void OnEnable() => SetActivity(true);
    private void OnDisable() => SetActivity(false);

    void SetActivity(bool active)
    {
        currentState.enabled = active;
        onSetActive.Invoke(active);
    }
}

public abstract class StateFunction : MonoBehaviour
{
    StateController _base;

    /// <summary>
    /// The controller that this state is managed by. Only registers if it's in a parent GameObject.
    /// </summary>
    public StateController controller
    {
        get
        {
            if (_base == null && transform.parent != null)
            {
                _base = transform.parent.GetComponentInParent<StateController>();
            }
            return _base;
        }
    }
    /// <summary>
    /// The root controller in a hierarchy.
    /// <para></para>
    /// </summary>
    public StateController root
    {
        get
        {
            // If a parent controller exists, return its root instead
            if (controller != null) return controller.root;
            // If not, check if this state is itself a controller. If so, it's the root.
            if (this is StateController c) return c;
            // If not, the state is not part of a hierarchy and there is no valid root.
            return null;
        }
    }
    // If a parent controller is present, ask it for its root.
    // Once a state can't find a parent controller, that state is the root.

    public virtual void SwitchToState(StateFunction newState) => controller.SwitchToState(newState);

    //public virtual IEnumerator AsyncProcedure() => null;
}