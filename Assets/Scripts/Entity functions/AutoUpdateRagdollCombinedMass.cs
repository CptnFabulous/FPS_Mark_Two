using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use this to average out a ragdoll's mass, if it's not attached to a PhysicsAffectedAI component.
/// </summary>
[RequireComponent(typeof(Ragdoll))]
public class AutoUpdateRagdollCombinedMass : MonoBehaviour
{
    [SerializeField] float combinedMass = 1;

    private void Start() => GetComponent<Ragdoll>().combinedMass = combinedMass;
}
