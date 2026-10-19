using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetObjectsActiveStateEnMasse : MonoBehaviour
{
    public GameObject[] gameobjects;

    public void SetActive(bool active)
    {
        Debug.Log($"{this}: setting state to {active}");
        foreach (GameObject go in gameobjects)
        {
            go.SetActive(active);
        }
    }
}
