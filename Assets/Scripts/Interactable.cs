using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    public string prompt;
    public UnityEvent<Player> onInteract;




    public void SendTestMessage(Player user)
    {
        Debug.Log(user + " interacted with " + name + " on frame " + Time.frameCount);
    }
}
