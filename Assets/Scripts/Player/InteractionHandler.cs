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
    //public PropCarryingHandler objectCarrier;

    List<Collider> collidersChecked = new List<Collider>();

    public Interactable targetedInteractable { get; private set; }
    public Rigidbody targetedPhysicsProp { get; private set; }
    public RaycastHit hitData { get; private set; }

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

        // Obtain initial colliders
        collidersChecked.AddRange(Physics.OverlapSphere(aimOrigin, interactionRange, detectionMask));

        #region Sort by which is closest to the centre of the camera

        // By sorting first we don't need to fully check every single option, only until we find a valid one
        Vector2 aimScreenPoint = referenceCamera.WorldToScreenPoint(aimOrigin + aimDirection);
        MiscFunctions.SortListWithOnePredicate(collidersChecked, (c) =>
        {
            // Get screen position of bounds centre, and compare it to the position of the player's reticle
            // (use sqrMagnitude since we don't need the exact distance)
            Vector2 screenPosition = referenceCamera.WorldToScreenPoint(c.bounds.center);
            return (screenPosition - aimScreenPoint).sqrMagnitude;
        });

        #endregion

        #region Get first entry that meets the criteria

        foreach (Collider c in collidersChecked)
        {
            // Check for either an Interactable or Rigidbody, ignore if not found
            Interactable i = c.GetComponentInParent<Interactable>();
            Rigidbody rb = null;
            /*
            if (objectCarrier != null)
            {
                rb = MiscFunctions.GetComponentInParentThatMeetsCriteria<Rigidbody>(c.transform, objectCarrier.CanPickUpObject);
            }
            */
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
            break;
        }

        #endregion

        // Do stuff with highlighted interactable
        //Debug.Log($"Targeted = {targetedInteractable}, {targetedPhysicsProp}");
    }




    protected virtual void AttemptInteraction()
    {
        if (targetedInteractable != null)
        {
            ContextInteractionCheck(targetedInteractable);
        }
        /*
        else if (targetedPhysicsProp != null)
        {
            PhysicsPropInteractionCheck(targetedPhysicsProp);
        }
        */
        // TO DO: do ContextInteractionCheck if a context interactable is present, otherwise try PhysicsPropInteractionCheck
    }
    void ContextInteractionCheck(Interactable target)
    {
        if (target.CanInteract(player))
        {
            Debug.Log($"Interacting with {target}");
            target.OnInteract(player);
            onInteract.Invoke(player, target);
        }
        else
        {
            // Display some kind of 'on failed interaction' message (e.g. a noise)
            Debug.Log($"Cannot interact with {target}");
            onInteractFailed.Invoke(player, target);
        }
    }
    /*
    void PhysicsPropInteractionCheck(Rigidbody target)
    {

    }
    */

}