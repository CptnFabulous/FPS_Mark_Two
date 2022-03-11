using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    Rigidbody[] allJointRigidbodies;

    private void Awake()
    {
        allJointRigidbodies = GetComponentsInChildren<Rigidbody>();
        SetActive(enabled);
    }

    private void OnEnable()
    {
        SetActive(true);
    }
    private void OnDisable()
    {
        SetActive(false);
    }

    void SetActive(bool active)
    {
        for (int i = 0; i < allJointRigidbodies.Length; i++)
        {
            allJointRigidbodies[i].isKinematic = !active;
        }
    }
}
