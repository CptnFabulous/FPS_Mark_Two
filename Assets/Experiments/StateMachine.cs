using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    MachineState currentState;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currentState.Update(this);
    }

    private void LateUpdate()
    {
        currentState.LateUpdate(this);
    }

    private void FixedUpdate()
    {
        currentState.FixedUpdate(this);
    }
}
