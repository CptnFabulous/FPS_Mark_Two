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
    public void UpdateWeaponHUD(Weapon currentWeapon)
    {
        weaponInterface.gameObject.SetActive(currentWeapon != null);
        if (currentWeapon == null) return;

        WeaponMode mode = currentWeapon.CurrentMode;
        weaponModeName.text = mode.name;
        weaponModeIcon.sprite = mode.icon;






        // Check if attack is a ranged mode
        RangedAttack rangedAttack = mode as RangedAttack;
        magazineMeter.gameObject.SetActive(rangedAttack != null);
        ammoReserve.gameObject.SetActive(rangedAttack != null);

        if (rangedAttack == null) return;

        // If weapon consumes ammo, show reserve
        bool consumesAmmo = rangedAttack.consumesAmmo;
        ammoReserve.gameObject.SetActive(consumesAmmo);
        if (consumesAmmo)
        {
            Resource remainingAmmo = controller.weapons.ammo.GetValues(rangedAttack.stats.ammoType);

            if (rangedAttack.magazine != null) // If magazine is present, change ammo bar to show reserve excluding magazine amount
            {
                remainingAmmo.current -= rangedAttack.magazine.ammo.current;
                remainingAmmo.max -= (int)rangedAttack.magazine.ammo.max;
            }

            ammoReserve.Refresh(remainingAmmo);
        }

        // If weapon has a magazine, show values
        magazineMeter.gameObject.SetActive(rangedAttack.magazine != null);
        if (rangedAttack.magazine != null)
        {
            magazineMeter.Refresh(rangedAttack.magazine.ammo);
        }
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
