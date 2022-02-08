using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public List<State> allStates = new List<State>();
    public List<Transition> allTransitions = new List<Transition>();
    int currentStateIndex = 0;
    public State currentState
    {
        get
        {
            if (allStates.Count <= 0 || currentStateIndex < 0 || currentStateIndex > allStates.Count - 1)
            {
                return null;
            }
            return allStates[currentStateIndex];
        }
        set
        {
            currentStateIndex = allStates.IndexOf(value);
        }
    }

    void Start()
    {
        currentState?.Enter(this);
    }
    void Update()
    {
        if (currentState == null)
        {
            return;
        }

        for (int i = 0; i < allTransitions.Count; i++)
        {
            // If transition can happen from the current state (or from anywhere)
            bool transitionCanOccur = allTransitions[i].from == currentState || allTransitions[i].from == null;
            if (transitionCanOccur && allTransitions[i].conditions.Invoke() == true)
            {
                currentState?.Exit(this);
                currentState = allTransitions[i].to;
                currentState.Enter(this);
            }
        }

        currentState?.Update(this);
    }
    private void LateUpdate()
    {
        currentState?.LateUpdate(this);
    }
    private void FixedUpdate()
    {
        currentState?.FixedUpdate(this);
    }

    public void AddState(State newState, bool setAsDefault = false)
    {
        allStates.Add(newState);
        if (setAsDefault)
        {
            currentState = newState;
        }
    }
    public void AddTransition(State from, State to, Func<bool> conditions)
    {
        allTransitions.Add(new Transition(from, to, conditions));
    }

    #region Required classes
    [System.Serializable]
    public abstract class State
    {
        public string name;
        public virtual void Enter(StateMachine controller) { return; }
        public virtual void Update(StateMachine controller) { return; }
        public virtual void LateUpdate(StateMachine controller) { return; }
        public virtual void FixedUpdate(StateMachine controller) { return; }
        public virtual void Exit(StateMachine controller) { return; }
    }
    public struct Transition
    {
        public State from;
        public State to;
        public Func<bool> conditions;
        public Transition(State _from, State _to, Func<bool> _conditions)
        {
            from = _from;
            to = _to;
            conditions = _conditions;
        }
    }
    #endregion

    /// <summary>
    /// Runs multiple states simultaneously.
    /// </summary>
    public class MultiState : StateMachine.State
    {
        public MultiState(string newName)
        {
            name = newName;
        }

        public List<StateMachine.State> allStates = new List<StateMachine.State>();
        public override void Enter(StateMachine controller)
        {
            for (int i = 0; i < allStates.Count; i++)
            {
                allStates[i].Enter(controller);
            }
        }
        public override void Update(StateMachine controller)
        {
            for (int i = 0; i < allStates.Count; i++)
            {
                allStates[i].Update(controller);
            }
        }
        public override void LateUpdate(StateMachine controller)
        {
            for (int i = 0; i < allStates.Count; i++)
            {
                allStates[i].LateUpdate(controller);
            }
        }
        public override void FixedUpdate(StateMachine controller)
        {
            for (int i = 0; i < allStates.Count; i++)
            {
                allStates[i].FixedUpdate(controller);
            }
        }
        public override void Exit(StateMachine controller)
        {
            for (int i = 0; i < allStates.Count; i++)
            {
                allStates[i].Exit(controller);
            }
        }
    }
    /// <summary>
    /// Adds a secondary layer of states with their own transitions.
    /// </summary>
    public class SubStateMachine : StateMachine.State
    {
        public SubStateMachine(string newName)
        {
            name = newName;
        }
        
        public List<StateMachine.State> allStates = new List<StateMachine.State>();
        public List<StateMachine.Transition> allTransitions = new List<StateMachine.Transition>();
        int currentStateIndex = 0;
        public StateMachine.State currentState
        {
            get
            {
                if (allStates.Count <= 0 || currentStateIndex < 0 || currentStateIndex > allStates.Count - 1)
                {
                    return null;
                }
                return allStates[currentStateIndex];
            }
            set
            {
                currentStateIndex = allStates.IndexOf(value);
            }
        }

        public override void Enter(StateMachine controller)
        {
            currentState?.Enter(controller);
        }
        public override void Update(StateMachine controller)
        {
            if (currentState == null)
            {
                return;
            }

            for (int i = 0; i < allTransitions.Count; i++)
            {
                // If transition can happen from the current state (or from anywhere)
                bool transitionCanOccur = allTransitions[i].from == currentState || allTransitions[i].from == null;
                if (transitionCanOccur && allTransitions[i].conditions.Invoke() == true)
                {
                    currentState?.Exit(controller);
                    currentState = allTransitions[i].to;
                    currentState.Enter(controller);
                }
            }

            currentState?.Update(controller);
        }
        public override void LateUpdate(StateMachine controller)
        {
            currentState?.LateUpdate(controller);
        }
        public override void FixedUpdate(StateMachine controller)
        {
            currentState?.FixedUpdate(controller);
        }
        public override void Exit(StateMachine controller)
        {
            currentState?.Exit(controller);
        }

        public void AddState(StateMachine.State newState, bool setAsDefault = false)
        {
            allStates.Add(newState);
            if (setAsDefault)
            {
                currentState = newState;
            }
        }
        public void AddTransition(StateMachine.State from, StateMachine.State to, Func<bool> conditions)
        {
            allTransitions.Add(new StateMachine.Transition(from, to, conditions));
        }
    }

    [System.Serializable]
    public class TestState : State
    {
        public float timeLimit;
        float stateEnterTime;

        public override void Enter(StateMachine controller)
        {
            Debug.Log("Entering state " + name + " on frame " + Time.frameCount);
            stateEnterTime = Time.time;
        }

        public override void Exit(StateMachine controller)
        {
            Debug.Log("Exiting state " + name + " on frame " + Time.frameCount);
        }

        public override void FixedUpdate(StateMachine controller)
        {
            Debug.Log("Physics updating state " + name + " on frame " + Time.frameCount);
        }

        public override void LateUpdate(StateMachine controller)
        {
            Debug.Log("Late updating state " + name + " on frame " + Time.frameCount);
        }

        public override void Update(StateMachine controller)
        {
            Debug.Log("Updating state " + name + " on frame " + Time.frameCount);
        }

        public Func<bool> TimeLimitExceeded() => () => (Time.time - stateEnterTime) >= timeLimit;
        /*
        Func<bool> TimeLimitExceeded() => TimeLimitTest;
        bool TimeLimitTest()
        {
            return (Time.time - stateEnterTime) >= timeLimit;
        }
        */
    }
}
