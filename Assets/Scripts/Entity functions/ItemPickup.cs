using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] Interactable interactable;
    [SerializeField] DiegeticSound pickupNoise;

    private void Awake()
    {
        interactable.onInteract.AddListener(OnPickup);
        interactable.canInteract += CanInteract;
    }
    public virtual bool CanInteract(Player player) => true;
    public virtual void OnPickup(Player player)
    {
        Destroy(gameObject);
    }

    /*
    [SerializeField] float timeToPickUp = 0.5f;
    [SerializeField] AnimationCurve pickupAnimationCurve = ;

    IEnumerator PickupSequence(Player player)
    {
        interactable.active = false;

        Vector3 startPosition = transform.position;
        
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / timeToPickUp;
            t = Mathf.Clamp01(t);
            transform.position = Vector3.Lerp(startPosition, player.transform.position, t);
        }
    }
    */
}
