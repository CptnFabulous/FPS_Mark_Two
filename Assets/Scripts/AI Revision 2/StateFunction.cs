using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    /// <summary>
    /// Runs before the state is deactivated by the parent controller.
    /// </summary>
    public virtual IEnumerator AsyncExit() => null;
    /// <summary>
    /// Runs after this state is enabled by the parent controller.
    /// </summary>
    public virtual IEnumerator AsyncProcedure() => null;
}