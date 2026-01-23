using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class DoorBase : MonoBehaviour
{
    public Interactable handleInteractable;

    [Header("Locking")]
    public DoorLock lockingMechanism;
    [SerializeField] bool startLocked;
    [SerializeField] string lockedPrompt = "Locked";
    [SerializeField] string unlockPrompt = "Unlock";

    [Header("Events")]
    public UnityEvent<Player> onOpen;
    public UnityEvent<Player> onClose;
    public UnityEvent onUnlock;
    public UnityEvent onLock;

    public abstract bool isOpen { get; }
    public bool isLocked
    {
        get => IsLocked();
        set
        {
            SetLock(value);
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
            message = OpenMessage(player, isOpen);
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
        if (isOpen)
        {
            Close();
            onClose.Invoke(player);
        }
        else
        {
            Open();
            onOpen.Invoke(player);
        }
    }

    public abstract string OpenMessage(Player player, bool isOpen);
    protected abstract void Open();
    protected abstract void Close();
    protected abstract bool IsLocked();
    protected abstract void SetLock(bool locked);
}

public abstract class DoorLock : MonoBehaviour
{
    public string lockedMessage = "Locked";

    public abstract bool CanOpen(Player player);
}