using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealthHUD : MonoBehaviour
{
    

    public Player playerTracking;
    public ResourceMeter healthMeter;
    public UnityEvent damageEffects;
    public UnityEvent healEffects;

    [Header("Directional indicators")]
    public DirectionalHUDIndicator indicatorPrefab;
    public Transform screenCentre;
    List<DirectionalHUDIndicator> currentIndicators = new List<DirectionalHUDIndicator>();

    private void Start()
    {
        healthMeter.Refresh(playerTracking.health.data);
        EventHandler.Subscribe(CheckToRunEffects, true);

        indicatorPrefab.gameObject.SetActive(false);
        indicatorPrefab.animation.playOnAwake = true;
        indicatorPrefab.animation.looping = false;
    }

    void CheckToRunEffects(DamageMessage message)
    {
        #region Meter, damage and heal effects
        if (message.victim != playerTracking.health)
        {
            return;
        }

        healthMeter.Refresh(playerTracking.health.data);

        if (message.amount < 0)
        {
            healEffects.Invoke();
            return;
        }
        
        damageEffects.Invoke();

        #endregion

        #region Directional damage indicators
        if (message.attacker == null)
        {
            return;
        }

        // Check if an existing indicator is marking the new attacker
        currentIndicators.RemoveAll((i) => i == null);
        DirectionalHUDIndicator existing = currentIndicators.Find((i) =>
        {
            return i.targetCharacter == message.attacker || i.targetTransform == message.attacker.transform;
        });
        if (existing != null)
        {
            // If so, restart the current indicator's animation, and don't make a new one
            existing.animation.Play();
            return;
        }



        Debug.Log("Checking to assign indicator");

        DirectionalHUDIndicator newIndicator = Instantiate(indicatorPrefab, screenCentre);
        newIndicator.Setup(playerTracking, screenCentre);

        Character attackingCharacter = message.attacker as Character;
        if (attackingCharacter != null)
        {
            newIndicator.targetCharacter = attackingCharacter;
        }
        else
        {
            newIndicator.targetTransform = message.attacker.transform;
        }

        currentIndicators.Add(newIndicator);
        newIndicator.gameObject.SetActive(true);
        #endregion
    }
}
