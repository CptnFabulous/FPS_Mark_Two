using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStatusIcon : MonoBehaviour
{
    public enum AIStatus
    {
        Idle,
        Patrolling,
        Suspicious,
        Hostile,
        Confused,
    }
    
    public void TriggerAnimation(AIStatus newStatus)
    {

    }
}
