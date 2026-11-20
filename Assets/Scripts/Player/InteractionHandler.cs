using CptnFabulous.MiscUtility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractionHandler : MonoBehaviour
{
    [Header("Detection")]
    public float interactionRange = 2;
    public float detectionAngle = 60;
    public DetectionProfile detection;

    [Header("Player data")]
    public Player player;
    public Camera referenceCamera;
    public SingleInput input;

    [Header("Interaction")]
    public UnityEvent<Player, Interactable> onInteract;
    public UnityEvent<Player, Interactable> onInteractFailed;
    public PropCarryingHandler objectCarrier;


    Collider[] colliderArray = new Collider[64];
    AngleCheck.ResultData<(Interactable, Rigidbody)> results;

    public Interactable targetedInteractable { get; private set; }
    public Rigidbody targetedPhysicsProp { get; private set; }
    public RaycastHit hitData { get; private set; }
    public Bounds targetBounds { get; private set; }
    public bool canInteractWithTarget { get; private set; }
    public string message { get; private set; }

    Ray interactionRay => new Ray(referenceCamera.transform.position, referenceCamera.transform.forward);

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
        string msg = null;

        bool angleCheck = AngleCheck.CheckForObjectsInCone(interactionRay, detectionAngle, interactionRange, detection.mask, colliderArray, out results, ValidInteractableFound);

        // Cache the data on whatever we hit this frame
        targetedInteractable = results.returnedValue.Item1;
        targetedPhysicsProp = results.returnedValue.Item2;
        hitData = results.hitData;

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
            targetBounds = targetedPhysicsProp.CalculateBoundsOfRigidbodyChain();
        }
    }



    bool ValidInteractableFound(Collider c, out (Interactable, Rigidbody) returnedValues)
    {
        bool check = ValidInteractableFound(c, out Interactable i, out Rigidbody rb);
        returnedValues = (i, rb);
        return check;
    }
    bool ValidInteractableFound(Collider c, out Interactable i, out Rigidbody rb)
    {
        i = null;
        rb = null;

        // Check that the target object can be seen on screen.
        bool insideCamera = TransformUtility.PointIsInsideCamera(referenceCamera, c.bounds.center);
        if (!insideCamera) return false;

        // Check for either an Interactable or Rigidbody, ignore if not found
        i = c.GetComponentInParent<Interactable>();
        if (i != null) return true;

        if (objectCarrier != null)
        {
            // Check for a root rigidbody, and then check if the player can pick it up
            rb = c.GetComponentInParent<Rigidbody>();
            rb = PhysicsCache.GetRootRigidbody(rb);
            if (objectCarrier.CanPickUpObject(rb) == false) rb = null;
            if (rb != null) return true;
        }

        
        //return i != null || rb != null;
        return false;
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