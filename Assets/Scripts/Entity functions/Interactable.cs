using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    public bool active = true;
    public string promptMessage = "Interact";
    public string inProgressMessage = "In progress";
    public string disabledMessage = "Cannot interact";
    public bool activateOnCollision;
    public UnityEvent<Player> onInteract;

    IEnumerator cooldown;
    float cooldownTimer;
    public float Progress
    {
        get
        {
            return cooldownTimer;
        }
    }


    public virtual void OnInteract(Player interactedWith)
    {
        onInteract.Invoke(interactedWith);
        Notification<InteractionMessage>.Transmit(new InteractionMessage(interactedWith, this));
    }

    private void OnCollisionEnter(Collision collision)
    {
        Player p = collision.collider.GetComponentInParent<Player>();
        if (activateOnCollision && p != null && active)
        {
            OnInteract(p);
        }
    }


    public void PrintDebugLog(Player user)
    {
        Debug.Log(user + " interacted with " + name + " on frame " + Time.frameCount);
    }

    public void StartCooldown(float duration)
    {
        Debug.Log("Starting cooldown");

        cooldown = Cooldown(duration);
        StartCoroutine(cooldown);
    }
    public IEnumerator Cooldown(float duration)
    {
        active = false;
        cooldownTimer = 0;
        while (cooldownTimer != 1)
        {
            Debug.Log("Ending cooldown");

            cooldownTimer += Time.deltaTime / duration;
            cooldownTimer = Mathf.Clamp01(cooldownTimer);
            yield return null;
        }
        EndCooldown();
    }
    public void EndCooldown()
    {
        Debug.Log("Ending cooldown");
        StopCoroutine(cooldown);
        cooldown = null;
        cooldownTimer = 0;
        active = true;
    }
}
