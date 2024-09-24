using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolArea : AIStateFunction
{
    public InvestigateLocations investigateState;

    protected override void OnEnable()
    {
        base.OnEnable();
        investigateState.onFail = this;

        navMeshAgent.isStopped = true;
    }
    private void OnDisable()
    {
        navMeshAgent.isStopped = false;
    }
    private void Update()
    {
        // TO DO: get rid of the navmeshagent stopping bits and have the AI move around an area based on a defined path.
    }
}
