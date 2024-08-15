using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    public bool active = true;
    public bool activateOnCollision;
    public Entity parentEntity;
    [Tooltip("If left blank, will instead use the name of the parent entity, or the object name itself")]
    public string displayName;
    public string promptMessage = "Interact";
    public string inProgressMessage = "In progress";
    public string disabledMessage = "Cannot interact";

    public UnityEvent<Player> onInteract;
    public System.Func<Player, bool> canInteract;

    public Collider collider => c ??= GetComponent<Collider>();
    Collider c;

    IEnumerator cooldown;
    float cooldownTimer;
    public float Progress => cooldownTimer;

    public bool CanInteract(Player player) => active && (canInteract == null || canInteract.Invoke(player));
    public virtual void OnInteract(Player interactedWith)
    {
        onInteract.Invoke(interactedWith);
        Notification<InteractionMessage>.Transmit(new InteractionMessage(interactedWith, this));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (activateOnCollision == false) return;

        Debug.Log("Checking collision for player");
        Player p = collision.collider.GetComponentInParent<Player>();
        if (p == null) return;

        Debug.Log("Player is present, checking if interactable");
        if (CanInteract(p))
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
    IEnumerator Cooldown(float duration)
    {
        active = false;
        cooldownTimer = 0;
        while (cooldownTimer != 1)
        {
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
