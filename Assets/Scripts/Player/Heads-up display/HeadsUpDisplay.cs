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

        Character observedEnemy = null;
        if (RelevantThingObserved(detectionRange, out RaycastHit observedObject))
        {
            Hitbox h = observedObject.collider.GetComponent<Hitbox>();
            if (h != null)
            {
                observedEnemy = h.attachedTo;
            }
        }

        ShowEnemyHealthMeter(observedEnemy);
    }
    public void ShowEnemyHealthMeter(Character enemy)
    {
        bool shouldShow = enemy != null && enemy.health.IsAlive;
        enemyHealthMeter.gameObject.SetActive(shouldShow);
        if (shouldShow == false) return;

        Health enemyHealth = enemy.health;
        Bounds entityBounds = enemy.bounds;
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
        if (message.attacker != controller)
        {
            return;
        }

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
