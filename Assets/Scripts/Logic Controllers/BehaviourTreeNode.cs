using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BehaviourTreeNode : MonoBehaviour
{
    
    // TO DO: should I modify behaviour tree nodes to not retain any information?
    // What if I want to run a loop recursively?
    // E.g. if a task fails, try a troubleshooting task and then retry the loop from the top?
    
    // Maybe I should remove currentHost and make BehaviourTreeNode into an interface.
    // Then BehaviourTree can be a MonoBehaviour that inherits from it.
    // That way I can even make nodes from ScriptableObjects.
    
    
    
    
    
    
    
    protected BehaviourTreeBase currentHost;

    /// <summary>
    /// Recurses up through the behaviour tree to find the original sequence of behaviours.
    /// </summary>
    public BehaviourTreeBase trunk
    {
        get
        {
            if (currentHost == null) return currentHost;
            if (currentHost.trunk == null) return currentHost;
            return currentHost.trunk;
        }
    }

    /// <summary>
    /// Starts running this state and performs associated start-up tasks.
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    public IEnumerator StartTask(BehaviourTreeBase host)
    {
        // Assigns the current host
        // So we can keep track of the hierarchy
        // And allow directly cancelling the current task
        currentHost = host;
        // Enables this node
        enabled = true;
        yield return new WaitForEndOfFrame();
        // Once enough time has passed that we know the script is enabled, start running the task.
        yield return PerformTask();
    }
    protected abstract IEnumerator PerformTask();
    public abstract IEnumerator OnCancelTask();
    /// <summary>
    /// Did this task succeed in the end? Ignored if it was ended prematurely.
    /// </summary>
    /// <returns></returns>
    public abstract bool Result();

    protected void EndPrematurely(bool success) => currentHost.EndPrematurely(success);
}