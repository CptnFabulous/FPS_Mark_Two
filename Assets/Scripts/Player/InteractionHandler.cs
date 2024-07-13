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

    List<Collider> collidersChecked = new List<Collider>();

    public Interactable targetedInteractable { get; private set; }
    public Rigidbody targetedPhysicsProp { get; private set; }
    public RaycastHit hitData { get; private set; }
    public Bounds targetBounds { get; private set; }

    Vector3 aimOrigin => referenceCamera.transform.position;
    Vector3 aimDirection => referenceCamera.transform.forward;

    private void Awake()
    {
        input.onActionPerformed.AddListener((_) => AttemptInteraction());
    }
    private void Update()
    {
        // Reset values
        targetedInteractable = null;
        targetedPhysicsProp = null;
        hitData = new RaycastHit();
        collidersChecked.Clear();

        #region Obtain and sort colliders

        // Obtain initial colliders
        collidersChecked.AddRange(Physics.OverlapSphere(aimOrigin, interactionRange, detectionMask));
        // Sort the list first, by which is closest to the centre of the camera
        // This way we don't have to check the whole list, only until we find one valid option
        Vector2 aimScreenPoint = referenceCamera.WorldToScreenPoint(aimOrigin + aimDirection);
        MiscFunctions.SortListWithOnePredicate(collidersChecked, (c) =>
        {
            // Get screen position of bounds centre, and compare it to the position of the player's reticle
            // (use sqrMagnitude since we don't need the exact distance)
            Vector2 screenPosition = referenceCamera.WorldToScreenPoint(c.bounds.center);
            return (screenPosition - aimScreenPoint).sqrMagnitude;
        });

        #endregion

        // Get first entry that meets the criteria
        foreach (Collider c in collidersChecked)
        {
            // Check for either an Interactable or Rigidbody, ignore if not found
            Interactable i = c.GetComponentInParent<Interactable>();
            Rigidbody rb = null;
            if (objectCarrier != null)
            {
                // Check for a root rigidbody, and then check if the player can pick it up
                rb = c.GetComponentInParent<Rigidbody>();
                rb = PhysicsCache.GetRootRigidbody(rb);
                if (objectCarrier.CanPickUpObject(rb) == false) rb = null;
            }
            if (i == null && rb == null) continue;

            // Check if within field of view, ignore if not
            if (Vector3.Angle(aimDirection, c.bounds.center - aimOrigin) > detectionAngle) continue;

            // Check for line of sight, ignore if the raycast didn't hit the desired collider, or at all
            Ray ray = new Ray(aimOrigin, c.bounds.center - aimOrigin);
            if (Physics.Raycast(ray, out RaycastHit rh, interactionRange, detectionMask) == false) continue;
            if (rh.collider != c) continue;

            // We've found the closest interactable! Mark it as the highlighted one
            targetedInteractable = i;
            targetedPhysicsProp = rb;
            hitData = rh;
            // Get bounds
            if (i != null)
            {
                targetBounds = i.collider.bounds;
            }
            else if (rb != null)
            {
                targetBounds = MiscFunctions.CombinedBounds(PhysicsCache.GetChildColliders(rb));
            }
            
            break;
        }
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
        if (target.CanInteract(player))
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
        if (objectCarrier.CanPickUpObject(target))
        {
            objectCarrier.Pickup(target);
            objectCarrier.onPickup.Invoke(target);
        }
        else
        {
            objectCarrier.onPickupFailed.Invoke(target);
        }
    }

}