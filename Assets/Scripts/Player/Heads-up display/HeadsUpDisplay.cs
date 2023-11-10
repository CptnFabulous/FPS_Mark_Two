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
    Health observedEnemyHealth;
    public void CheckIfLookingAtDamageableEntity()
    {
        observedEnemyHealth = null;

        float detectionRange = observationRange;
        if (controller.weapons.CurrentWeapon != null)
        {
            RangedAttack attack = controller.weapons.CurrentWeapon.CurrentMode as RangedAttack;
            if (attack != null)
            {
                detectionRange = attack.stats.range;
            }
        }

        if (RelevantThingObserved(detectionRange, out RaycastHit observedObject))
        {
            Hitbox h = observedObject.collider.GetComponent<Hitbox>();
            if (h != null)
            {
                observedEnemyHealth = h.sourceHealth;
            }
        }

        ShowEnemyHealthMeter(observedEnemyHealth);
    }
    public void ShowEnemyHealthMeter(Health enemyHealth)
    {
        enemyHealthMeter.gameObject.SetActive(false);
        if (enemyHealth == null || enemyHealth.IsAlive == false)
        {
            return;
        }

        Bounds entityBounds = enemyHealth.HitboxBounds;
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
