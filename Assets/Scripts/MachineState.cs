using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MachineState
{
    
    public abstract bool ExitCriteria { get; }
    public abstract void Enter(StateMachine controller);
    public abstract void Update(StateMachine controller);
    public abstract void LateUpdate(StateMachine controller);
    public abstract void FixedUpdate(StateMachine controller);
    public abstract void Exit(StateMachine controller);
}
