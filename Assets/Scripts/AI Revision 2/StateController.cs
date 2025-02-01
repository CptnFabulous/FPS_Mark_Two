using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StateController : StateFunction
{
    [SerializeField] Entity _rootEntity;
    [SerializeField] StateFunction current;
    public UnityEngine.Events.UnityEvent<bool> onSetActive;

    Coroutine currentCoroutine;

    public Entity rootEntity => _rootEntity ??= controller.rootEntity;
    public StateFunction currentState => current;
    public int stateIndex
    {
        get => MiscFunctions.IndexOfInArray(states, currentState);
        set
        {
            value = Mathf.Clamp(value, 0, states.Length - 1);
            SwitchToState(states[value]);
        }
    }
    public StateFunction previousState { get; private set; }
    public StateFunction nextState { get; set; } = null;
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
    private void OnEnable()
    {
        // Restart the current state and its coroutines
        SwitchToState(current);
        // Trigger events that need to occur to match the controller's active state
        onSetActive.Invoke(true);
    }
    private void OnDisable()
    {
        // Hard-stop the current state and all its IEnumerator-based functionality
        StopCurrentCoroutine();
        current.enabled = false;
        // Trigger events that need to occur to match the controller's active state
        onSetActive.Invoke(false);
    }
    /*
    public override IEnumerator AsyncEnter()
    {
        yield return EnterCurrentStateIfPresentButDisabled();
        onSetActive.Invoke(true);
    }
    */
    public override IEnumerator AsyncProcedure()
    {
        yield return EnterCurrentStateIfPresentButDisabled();
    }
    public override IEnumerator AsyncExit()
    {
        yield return ExitCurrentStateIfPresentAndEnabled();
    }

    public override void SwitchToState(StateFunction newState)
    {
        StartCoroutine(TrySwitchState(newState));
    }
    /// <summary>
    /// Attempts to switch to a new state, but checks if it's necessary first so it doesn't override any current actions.
    /// </summary>
    IEnumerator TrySwitchState(StateFunction newState)
    {
        // Do nothing if the desired state is not part of the state machine hierarchy
        if (newState.root != root)
        {
            rootEntity.DebugLog($"{this}: cannot switch to {newState} because it's not part of this state machine!");
            yield break;
        }

        // If not a child of this controller, but still in the hierarchy, initiate from its parent controller instead
        if (newState.controller != this)
        {
            newState.controller.SwitchToState(newState);
            yield break;
        }

        // Check if new state is the same as the current one
        // If so, check if it's currently enabled
        // If state is the same and already enabled, do nothing
        if (newState == currentState && newState.enabled)
        {
            //rootEntity.DebugLog($"{this}: already switched to {newState}");
            yield break;
        }
        // If state is different, switch normally
        // If state is the same but currently disabled, enable it

        // Don't trigger another switch if already started switching to the desired state
        if (nextState == newState) yield break;
        nextState = newState;

        // Stop coroutine if one is currently active
        StopCurrentCoroutine();
        // Start new coroutine (and wait on it to finish)
        currentCoroutine = StartCoroutine(SwitchToStateAsync(newState));
        yield return currentCoroutine;
    }
    /// <summary>
    /// Switches to a new state, assuming all the 'should the switch occur' checks were done in <see cref="TrySwitchState(StateFunction)"/>.
    /// </summary>
    IEnumerator SwitchToStateAsync(StateFunction newState)
    {
        // Hang on - if a controller is deactivated from elsewhere, that should automatically trigger the AsyncExit() on its active state, if there is one.
        // So I don't need to remotely run a state's exit coroutine from outside its parent controller, since disabling a parent controller will automatically disable its active child, and that'll happen recursively.

        // Check that the current state actually needs to be changed
        if (newState != currentState)
        {
            rootEntity.DebugLog($"{root}: switching from {current} to {newState}");
            // Finish and disable the currently active state
            yield return ExitCurrentStateIfPresentAndEnabled();
            // Assign current and previous states
            previousState = current;
            current = newState;
        }

        nextState = null;

        // Ensure parent controller is active and switched to this controller (in case there's a hierarchy)
        // If this state is already the active one, it shouldn't do anything.
        // If not, it'll disable the other states to make way for this one.
        // It'll go upwards recursively until every necessary parent controller is active and switched correctly
        // The parent state shouldn't be able to accidentally disable the current one by iterating downwards, as the coroutine will realise 'ExitCurrentState' coroutine will realise is set to not do anything if the desired state is already active
        if (controller != null)
        {
            rootEntity.DebugLog($"{root}: switching over parent controllers in hierarchy");
            yield return controller.TrySwitchState(this);
        }

        yield return EnterCurrentStateIfPresentButDisabled();
    }

    IEnumerator EnterCurrentStateIfPresentButDisabled()
    {
        //rootEntity.DebugLog($"{this}: entering current state {current} (if necessary)");

        // Ensure there's actually a state to enter into
        if (current == null) yield break;
        // Don't redo any of the entry functions if the state is already active
        if (current.enabled) yield break;

        //rootEntity.DebugLog($"{this}: entering current state {current}");
        /*
        // Wait for new state to run its enter function
        IEnumerator enter = current.AsyncEnter();
        if (enter != null) yield return enter;
        */
        // Officially enable new state
        current.enabled = true;

        // Run the new state's procedure function, after the state is officially enabled
        IEnumerator procedure = current.AsyncProcedure();
        if (procedure != null) yield return procedure;
    }
    IEnumerator ExitCurrentStateIfPresentAndEnabled()
    {
        //rootEntity.DebugLog($"{this}: exiting current state {current} (if necessary)");

        // End current state in hierarchy (assuming it exists and is already active)
        if (current == null) yield break;
        // Don't redo any of the exit functions if the state is already disabled
        if (current.enabled == false) yield break;

        //rootEntity.DebugLog($"{this}: exiting current state {current}");

        // Wait for current state to exit out of whatever it's doing
        IEnumerator exit = current.AsyncExit();
        if (exit != null) yield return exit;

        // Then disable it
        current.enabled = false;
    }

    void StopCurrentCoroutine()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
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

    /*
    /// <summary>
    /// Runs when this state is being switched to, before it's officially enabled.
    /// </summary>
    public virtual IEnumerator AsyncEnter() => null;
    */
    /// <summary>
    /// Runs before the state is deactivated by the parent controller.
    /// </summary>
    public virtual IEnumerator AsyncExit() => null;
    /// <summary>
    /// Runs after this state is enabled by the parent controller.
    /// </summary>
    public virtual IEnumerator AsyncProcedure() => null;
}