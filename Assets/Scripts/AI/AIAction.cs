using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class AIAction : StateMachine.State
{
    public AI AI { get; private set; }
    public Combatant CombatAI
    {
        get
        {
            return AI as Combatant;
        }
    }
    public Character Character
    {
        get
        {
            return AI.character;
        }
    }
    public NavMeshAgent NavMeshAgent
    {
        get
        {
            return AI.agent;
        }
    }
    public AIAim AimData
    {
        get
        {
            return AI.aiming;
        }
    }
    public override void Enter(StateMachine controller)
    {
        AI = controller.GetComponent<AI>();
    }
}