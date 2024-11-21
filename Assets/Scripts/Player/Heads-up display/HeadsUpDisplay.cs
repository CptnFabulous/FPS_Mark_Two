using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class HeadsUpDisplay : MonoBehaviour
{
    [Header("General")]
    public Player controller;
    public new Camera camera;
    public AudioSource soundPlayer;
    Canvas canvas;
    RectTransform rt;
    public void PlayAudioClip(AudioClip clip)
    {
        soundPlayer.PlayOneShot(clip);
    }
    public void PlayAudioClip(DiegeticSound soundEffect)
    {
        soundEffect.Play(soundPlayer);
    }



    [Header("Detection")]
    public float observationRange = 50;
    public LayerMask relevantThingDetection = ~0;
    public bool RelevantThingObserved(float range, out RaycastHit observedObject)
    {
        return Physics.Raycast(camera.transform.position, camera.transform.forward, out observedObject, range, relevantThingDetection);
    }

    [Header("Enemy Health")]
    public ResourceMeter enemyHealthMeter;
    public void CheckIfLookingAtDamageableEntity()
    {
        
        float detectionRange = observationRange;
        if (controller.weapons.CurrentWeapon != null)
        {
            RangedAttack attack = controller.weapons.CurrentWeapon.CurrentMode as RangedAttack;
            if (attack != null)
            {
                detectionRange = attack.stats.range;
            }
        }

        Entity observedEnemy = null;
        if (RelevantThingObserved(detectionRange, out RaycastHit observedObject))
        {
            Hitbox h = observedObject.collider.GetComponent<Hitbox>();
            if (h != null) observedEnemy = h.attachedTo;
        }

        bool shouldShow = observedEnemy != null && observedEnemy.health != null && observedEnemy.health.IsAlive;
        enemyHealthMeter.gameObject.SetActive(shouldShow);
        if (shouldShow == false) return;

        Health enemyHealth = observedEnemy.health;
        Bounds entityBounds = observedEnemy.bounds;
        Vector3 meterPosition = entityBounds.center + (camera.transform.up * entityBounds.extents.magnitude);

        Vector3 meterScreenPosition = camera.WorldToScreenPoint(meterPosition);
        Vector3 canvasPosition = MiscFunctions.ScreenToAnchoredPosition(meterScreenPosition, enemyHealthMeter.rectTransform, rt);//MiscFunctions.ScreenToRectTransformSpace(meterScreenPosition, rt);
        enemyHealthMeter.gameObject.SetActive(true);
        enemyHealthMeter.rectTransform.anchoredPosition = canvasPosition;
        enemyHealthMeter.Refresh(enemyHealth.data);
    }

    [Header("Damage")]
    public UnityEvent damageEffects;
    //public UnityEvent criticalEffects;
    public UnityEvent killEffects;
    public void CheckToPlayDamageEffects(DamageMessage message)
    {
        // Don't play effects for self-damage or self-healing
        if (message.victim == controller.health) return;

        if (message.method == DamageType.Healing) return;
        if (message.method == DamageType.DeletionByGame) return;

        // Don't play effects if another enemy was directly dealing the damage
        // I need to update this later to detect if an indirect form of damage was still caused by the player (so it can appropriately reward the player for actually setting up said kills)
        Entity attacker = message.attacker;
        if (attacker is Character c && c != controller && message.method != DamageType.PhysicsImpact) return;
        //if (attacker != controller) return;

        damageEffects.Invoke();
    }

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        rt = GetComponent<RectTransform>();

        Notification<DamageMessage>.Receivers += CheckToPlayDamageEffects;
    }
    private void LateUpdate()
    {
        //UpdateWeaponHUD(controller.weapons.CurrentWeapon);
        CheckIfLookingAtDamageableEntity();
    }

    
}
