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
    public void PlayAudioClip(RandomSoundPlayer soundEffect)
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

    [Header("Weapons")]
    public GameObject weaponInterface;
    public ResourceMeter magazineMeter;
    public ResourceMeter ammoReserve;
    public Text weaponModeName;
    public Image weaponModeIcon;
    public GameObject reticle;
    public void UpdateWeaponHUD(Weapon currentWeapon)
    {
        weaponInterface.gameObject.SetActive(currentWeapon != null);
        if (currentWeapon != null)
        {
            SetWeaponModeFeatures(currentWeapon.CurrentMode);
        }
    }
    public void HideWeaponHUD()
    {
        weaponInterface.gameObject.SetActive(false);
    }
    public void SetWeaponModeFeatures(WeaponMode currentMode)
    {
        weaponModeName.text = currentMode.name;
        weaponModeIcon.sprite = currentMode.icon;
        RefreshModeValues(currentMode);
    }
    public void RefreshModeValues(WeaponMode currentMode)
    {
        
        
        // Check if attack is a ranged mode
        RangedAttack rangedMode = currentMode as RangedAttack;
        magazineMeter.gameObject.SetActive(rangedMode != null);
        ammoReserve.gameObject.SetActive(rangedMode != null);
        if (rangedMode != null)
        {
            RangedAttackUIUpdate(rangedMode);
        }
    }
    public void RangedAttackUIUpdate(RangedAttack currentMode)
    {
        // If weapon consumes ammo, show reserve
        bool consumesAmmo = currentMode.stats.ConsumesAmmo;
        ammoReserve.gameObject.SetActive(consumesAmmo);
        if (consumesAmmo)
        {
            ammoReserve.gameObject.SetActive(true);
            Resource remainingAmmo = controller.weapons.ammo.GetValues(currentMode.stats.ammoType);

            if (currentMode.magazine != null) // If magazine is present, change ammo bar to show reserve excluding magazine amount
            {
                remainingAmmo.current -= currentMode.magazine.ammo.current;
                remainingAmmo.max -= (int)currentMode.magazine.ammo.max;
            }

            ammoReserve.Refresh(remainingAmmo);
        }

        // If weapon has a magazine, show values
        magazineMeter.gameObject.SetActive(currentMode.magazine != null);
        if (currentMode.magazine != null)
        {
            magazineMeter.Refresh(currentMode.magazine.ammo);
        }

        // Disable reticle if player is aiming down weapon sights
        // Show reticle If ADS is null, OR if ADS is present but player is not aiming/transitioning and hideDefaultReticle is false
        GunADS ads = currentMode.optics;
        bool notHiddenDueToAds = ads == null || (!(ads.IsAiming || ads.IsTransitioning) && !ads.hideMainReticle);
        bool notInWeaponSelector = controller.weapons.weaponSelector.active == false;
        reticle.SetActive(notHiddenDueToAds && notInWeaponSelector);
    }


    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        rt = GetComponent<RectTransform>();

        Notification<DamageMessage>.Receivers += CheckToPlayDamageEffects;
    }
    private void LateUpdate()
    {
        UpdateWeaponHUD(controller.weapons.CurrentWeapon);
        CheckIfLookingAtDamageableEntity();
    }

    
}
