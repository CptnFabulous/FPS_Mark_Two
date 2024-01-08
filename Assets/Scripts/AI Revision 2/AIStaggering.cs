using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.LowLevel;

public class AIStaggering : AIStateFunction
{
    [SerializeField] float staggerTime = 3;

    float timeEntered;
    StateFunction previousState;
    NavMeshPath existingPath;

    protected override void OnEnable()
    {
        base.OnEnable();
        
        previousState = controller.previousState;
        timeEntered = Time.time;
        
        existingPath = navMeshAgent.path;
        navMeshAgent.ResetPath();
    }
    private void OnDisable()
    {
        navMeshAgent.path = existingPath;
        existingPath = null;
    }

    // Update is called once per frame
    void Update()
    {
        // If the enemy has remained stunned for long enough, revert to normal functioning
        float timeElapsed = Time.time - timeEntered;
        if (timeElapsed > staggerTime)
        {
            SwitchToState(previousState);
        }
    }
}
