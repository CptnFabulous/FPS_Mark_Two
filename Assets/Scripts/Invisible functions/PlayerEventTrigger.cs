using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerEventTrigger : MonoBehaviour
{
    public UnityEvent<Player> onEnter;

    new Collider collider;
    new Rigidbody rigidbody;

    private void Awake()
    {
        collider = GetComponent<Collider>();
        collider.isTrigger = true;
        rigidbody = GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            rigidbody = gameObject.AddComponent<Rigidbody>();
        }
        rigidbody.isKinematic = true;
    }


    private void OnTriggerEnter(Collider other)
    {
        Player p = other.GetComponentInParent<Player>();
        if (p != null)
        {
            Debug.Log(p);
            onEnter.Invoke(p);
        }
    }
}
