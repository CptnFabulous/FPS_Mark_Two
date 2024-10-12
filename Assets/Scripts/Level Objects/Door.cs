using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Door : MonoBehaviour
{
    public Rigidbody doorBody;
    public HingeJoint joint;
    public Interactable handleInteractable;
    public float angleForOpen = 30;
    public Vector3 openForce = new Vector3(0, 100, 0);

    [Header("Locking")]
    public DoorLock lockingMechanism;
    [SerializeField] bool startLocked;
    public Vector3 lockRotation = Vector3.zero;

    [Header("External functions")]
    [SerializeField] string openPrompt = "Open";
    [SerializeField] string closePrompt = "Close";
    [SerializeField] string lockedPrompt = "Locked";
    [SerializeField] string unlockPrompt = "Unlock";
    public UnityEvent<Player> onOpen;
    public UnityEvent<Player> onClose;
    public UnityEvent onUnlock;
    public UnityEvent onLock;

    bool open => joint.angle > angleForOpen;

    public bool isLocked
    {
        get => doorBody.isKinematic;
        set
        {
            // Set ability for door to move
            doorBody.isKinematic = value;
            // If locked, force to shut position
            if (value) doorBody.transform.localRotation = Quaternion.Euler(lockRotation);
            // Invoke events
            (value ? onLock : onUnlock).Invoke();
        }
    }

    private void Awake()
    {
        isLocked = startLocked;

        handleInteractable.canInteract += CanOpen;
        handleInteractable.onInteract.AddListener(TriggerOpenOrClose);
    }

    public bool CanOpen(Player player, out string message)
    {
        // If unlocked, return true
        if (isLocked == false)
        {
            message = open ? closePrompt : openPrompt;
            return true;
        }

        // Otherwise return lock-specific data (check if the player has the means to unlock it)
        if (lockingMechanism != null)
        {
            bool canOpen = lockingMechanism.CanOpen(player);
            message = canOpen ? unlockPrompt : lockingMechanism.lockedMessage;
            return canOpen;
        }

        // if no lock is present, display generic locked prompt
        message = lockedPrompt;
        return false;
    }


    public void TriggerOpenOrClose(Player player)
    {
        if (isLocked)
        {
            isLocked = false;
            //return;
        }

        // Apply force in appropriate direction, and trigger the correct event
        if (open)
        {
            doorBody.AddTorque(-openForce);
            onClose.Invoke(player);
        }
        else
        {
            doorBody.AddTorque(openForce);
            onOpen.Invoke(player);
        }
    }


}

public abstract class DoorLock : MonoBehaviour
{
    public string lockedMessage = "Locked";

    public abstract bool CanOpen(Player player);
}