using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractionHandler : MonoBehaviour
{
    [Header("Detection")]
    public float interactionRange = 2;
    public float detectionAngle = 60;
    public LayerMask detectionMask = ~0;

    [Header("Player data")]
    public Player player;
    public Camera referenceCamera;
    public SingleInput input;

    [Header("Interaction")]
    public UnityEvent<Player, Interactable> onInteract;
    public UnityEvent<Player, Interactable> onInteractFailed;
    public PropCarryingHandler objectCarrier;

    public Interactable targetedInteractable { get; private set; }
    public Rigidbody targetedPhysicsProp { get; private set; }
    public RaycastHit hitData { get; private set; }
    public Bounds targetBounds { get; private set; }
    public bool canInteractWithTarget { get; private set; }
    public string message { get; private set; }

    Vector3 aimOrigin => referenceCamera.transform.position;
    Vector3 aimDirection => referenceCamera.transform.forward;

    private void OnEnable()
    {
        input.onActionPerformed.AddListener((_) => AttemptInteraction());
    }
    private void OnDisable()
    {
        input.onActionPerformed.RemoveListener((_) => AttemptInteraction());
    }
    private void Update()
    {
        // Reset values and set up values to assign
        (Interactable, Rigidbody) returnedValues;
        RaycastHit rh = new RaycastHit();
        string msg = null;

        bool angleCheck = AngleCheck.CheckForObjectsInCone(aimOrigin, aimDirection, detectionAngle, interactionRange, detectionMask, out returnedValues, out rh, ColliderIsInteractable);

        // Cache the data on whatever we hit this frame
        targetedInteractable = returnedValues.Item1;
        targetedPhysicsProp = returnedValues.Item2;
        hitData = rh;

        // Obtain and cache additional data
        if (targetedInteractable != null)
        {
            // Cache interactability data
            canInteractWithTarget = targetedInteractable.CanInteract(player, out msg);
            message = msg;
            targetBounds = targetedInteractable.collider.bounds;
        }
        else if (targetedPhysicsProp != null)
        {
            canInteractWithTarget = true;
            targetBounds = MiscFunctions.CombinedBounds(PhysicsCache.GetChildColliders(targetedPhysicsProp));
        }
    }

    bool ColliderIsInteractable(Collider c, out (Interactable, Rigidbody) returnedValues)
    {
        bool check = ColliderIsInteractable(c, out Interactable i, out Rigidbody rb);
        returnedValues = (i, rb);
        return check;
    }
    bool ColliderIsInteractable(Collider c, out Interactable i, out Rigidbody rb)
    {
        // Check for either an Interactable or Rigidbody, ignore if not found
        i = c.GetComponentInParent<Interactable>();
        rb = null;
        if (objectCarrier != null)
        {
            // Check for a root rigidbody, and then check if the player can pick it up
            rb = c.GetComponentInParent<Rigidbody>();
            rb = PhysicsCache.GetRootRigidbody(rb);
            if (objectCarrier.CanPickUpObject(rb) == false) rb = null;
        }

        return i != null || rb != null;
    }

    protected virtual void AttemptInteraction()
    {
        if (targetedInteractable != null)
        {
            ContextInteractionCheck(targetedInteractable);
        }
        else if (targetedPhysicsProp != null)
        {
            PhysicsPropInteractionCheck(targetedPhysicsProp);
        }
    }
    void ContextInteractionCheck(Interactable target)
    {
        if (canInteractWithTarget)
        {
            target.OnInteract(player);
            onInteract.Invoke(player, target);
        }
        else
        {
            onInteractFailed.Invoke(player, target);
        }
    }
    void PhysicsPropInteractionCheck(Rigidbody target)
    {
        if (canInteractWithTarget)
        {
            objectCarrier.Pickup(target);
        }
        else
        {
            //objectCarrier.onPickupFailed.Invoke(target);
        }
    }
}