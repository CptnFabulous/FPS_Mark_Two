using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GibbableObject : MonoBehaviour
{
    public GameObject parentGameObject;
    //public Rigidbody parentRigidbody;
    public List<Rigidbody> gibs;

    public void ActivateGibs()
    {
        // TO DO: figure out how to apply velocity from final attack to child objects

        /*
        if (parentRigidbody != null)
        {
            Debug.Log(parentRigidbody.velocity);
            Debug.Log(parentRigidbody.angularVelocity);
        }
        */
        foreach (Rigidbody gib in gibs)
        {
            gib.transform.SetParent(null);
            gib.gameObject.SetActive(true);
            gib.isKinematic = false;
            /*
            if (parentRigidbody != null)
            {
                gib.velocity = parentRigidbody.velocity;
                gib.angularVelocity = parentRigidbody.angularVelocity;
            }
            */
        }
        parentGameObject.SetActive(false);
    }
}
