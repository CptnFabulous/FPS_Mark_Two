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

        currentState.Update(this);
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
        public abstract void Enter(StateMachine controller);
        public abstract void Update(StateMachine controller);  
        public abstract void LateUpdate(StateMachine controller);
        public abstract void FixedUpdate(StateMachine controller);
        public abstract void Exit(StateMachine controller);
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
